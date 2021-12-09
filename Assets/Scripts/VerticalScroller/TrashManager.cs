using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using System.Linq;
using Unity.Jobs;

public class TrashManager : MonoBehaviour
{
    public int minTrash, maxTrash, maxTrashDepth, maxDangerousTrashDepth;
    public float minSpeed, maxSpeed, maxBob, bobSpeed;
    [Range(0, 100)]
    public float maxIsDangerousChance;
    public GameObject trashPrefab;
    public List<Sprite> trashSprites, dangerousTrashSprites;

    [HideInInspector]
    public Dictionary<int, List<Trash>> loadedTrash = new Dictionary<int, List<Trash>>();
    [HideInInspector]
    public int chunkHeight;
    [HideInInspector]
    public Dictionary<int, List<GameObject>> currentTrash = new Dictionary<int, List<GameObject>>();
    [HideInInspector]
    public List<int2> objectsToRemove, currentlyUnIndexed;

    private TransformAccessArray trashTransforms;

    // Start is called before the first frame update
    void Start()
    {
        trashTransforms = new TransformAccessArray(1);
        chunkHeight = GameObject.Find("Grid").GetComponent<ChunkManager>().height;
    }

    private void LateUpdate()
    {
    }

    // Update is called once per frame
    void Update()
    {
        foreach (int2 removeObject in objectsToRemove)
        {
            if (currentTrash.ContainsKey(removeObject.x))
            {
                if (currentTrash[removeObject.x].ElementAtOrDefault(removeObject.y) != null)
                {
                    Destroy(currentTrash[removeObject.x][removeObject.y]);
                    currentTrash[removeObject.x].RemoveAt(removeObject.y);
                    currentTrash[removeObject.x].TrimExcess();
                    loadedTrash[removeObject.x].RemoveAt(removeObject.y);
                    loadedTrash[removeObject.x].TrimExcess();
                }
                else
                {
                    currentlyUnIndexed.Add(removeObject);
                }
            }
            else
            {
                currentlyUnIndexed.Add(removeObject);
            }
        }
        objectsToRemove.Clear();
        objectsToRemove = currentlyUnIndexed;
        currentlyUnIndexed.Clear();
        int[] keys = currentTrash.Keys.ToArray();
        int arrayLength = 0;
        foreach (int key in keys)
        {
            foreach (GameObject entry in currentTrash[key])
            {
                arrayLength++;
            }
        }
        Transform [] transforms = new Transform[arrayLength];
        NativeArray<float> speeds = new NativeArray<float>(arrayLength, Allocator.TempJob);
        NativeArray<float> startSides = new NativeArray<float>(arrayLength, Allocator.TempJob);
        NativeArray<float> yPos = new NativeArray<float>(arrayLength, Allocator.TempJob);
        NativeArray<float> offsets = new NativeArray<float>(arrayLength, Allocator.TempJob);
        int counter = 0;
        foreach (int key in keys)
        {
            List<Trash> currentTrashList = loadedTrash[key];
            List<GameObject> currentObjectList = currentTrash[key];
            for (int i = 0; i < currentTrashList.Count; i++)
            {
                speeds[counter] = currentTrashList[i].speed;
                startSides[counter] = currentTrashList[i].startSide;
                yPos[counter] = currentTrashList[i].yPos;
                transforms[counter] = currentObjectList[i].transform;
                offsets[counter] = currentTrashList[i].offset;
                counter++;
            }
        }
        trashTransforms.SetTransforms(transforms);
        TrashScroller trashScroller = new TrashScroller
        {
            speeds = speeds, startSides = startSides, yPos = yPos, time = Time.time, offsets = offsets, maxBob = maxBob, bobSpeed = bobSpeed
        };
        JobHandle trashScrollerJob = trashScroller.Schedule(trashTransforms);
        trashScrollerJob.Complete();
        speeds.Dispose();
        yPos.Dispose();
        startSides.Dispose();
        offsets.Dispose();
    }

    public void ToggleTrash(int chunkIndex, bool isEnabled)
    {
        if (isEnabled)
        {
            if (!currentTrash.ContainsKey(chunkIndex))
            {
                if (loadedTrash.ContainsKey(chunkIndex))
                {
                    foreach (Trash trash in loadedTrash[chunkIndex])
                    {
                        InstantiateTrash(trash, chunkIndex);
                    }
                }
                else
                {
                    GenerateTrash(chunkIndex);
                }
            }
        }
        else
        {
            if (currentTrash.ContainsKey(chunkIndex))
            {
                foreach (GameObject oldTrash in currentTrash[chunkIndex])
                {
                    Destroy(oldTrash);
                }
                currentTrash.Remove(chunkIndex);
            }
        }
    }

    private void GenerateTrash(int index)
    {
        if (!loadedTrash.ContainsKey(index))
        {
            loadedTrash.Add(index, new List<Trash>());
            int chunkTrash = UnityEngine.Random.Range(minTrash, Mathf.FloorToInt(Mathf.Clamp((float)index / maxTrashDepth * maxTrash, minTrash, maxTrash)));
            for (int i = 0; i < chunkTrash; i++)
            {
                Sprite tempSprite;
                bool isDangerous = UnityEngine.Random.Range(0, 100) > (100 - Mathf.Clamp(index, 0, maxIsDangerousChance));
                if (!isDangerous)
                {
                    tempSprite = trashSprites[UnityEngine.Random.Range(0, trashSprites.Count)];
                }
                else
                {
                    tempSprite = dangerousTrashSprites[UnityEngine.Random.Range(0, dangerousTrashSprites.Count)];
                }
                loadedTrash[index].Add(new Trash()
                {
                    speed = UnityEngine.Random.Range(minSpeed, Mathf.Clamp(((float)index / maxTrashDepth) * maxSpeed, minSpeed, maxSpeed)),
                    startSide = Mathf.Sign(UnityEngine.Random.value - 0.5f),
                    yPos = (UnityEngine.Random.value + index) * chunkHeight,
                    isDangerous = isDangerous,
                    sprite = tempSprite,
                    offset = UnityEngine.Random.value * 360f,
                }); ;
            }
        }
        foreach (Trash trash in loadedTrash[index])
        {
            InstantiateTrash(trash, index);
        }
    }

    private void InstantiateTrash(Trash trash, int chunkIndex)
    {
        GameObject newTrash = Instantiate(trashPrefab, Vector3.down * trash.yPos, Quaternion.identity, gameObject.transform);
        if (!currentTrash.ContainsKey(chunkIndex))
        {
            currentTrash[chunkIndex] = new List<GameObject>();
        }
        currentTrash[chunkIndex].Add(newTrash);
        newTrash.GetComponent<SpriteRenderer>().sprite = loadedTrash[chunkIndex][currentTrash[chunkIndex].Count - 1].sprite;
        PolygonCollider2D newCollider = newTrash.AddComponent<PolygonCollider2D>();
        newCollider.isTrigger = true;
    }

    [BurstCompile]
    public struct TrashScroller : IJobParallelForTransform
    {
        public NativeArray<float> speeds, startSides, yPos, offsets;
        public float time, maxBob, bobSpeed;

        public void Execute(int index, TransformAccess transform)
        {
            transform.position = new float3(startSides[index] * 32 * math.sin(time * speeds[index] + offsets[index]), -yPos[index] + maxBob * math.sin(offsets[index] + bobSpeed * time), 0);
        }
    }

    private void OnDisable()
    {
        trashTransforms.Dispose();
    }
}
public struct Trash
{
    public float speed;
    public float startSide;
    public float yPos;
    public bool isDangerous;
    public Sprite sprite;
    public float offset;
}