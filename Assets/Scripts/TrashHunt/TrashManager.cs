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
    public float minSpeed, maxSpeed, maxBob, bobSpeed, rotationSpeed;
    [Range(0, 100)]
    public float maxIsDangerousChance;
    public GameObject trashPrefab;
    public Color dangerousSonarColour, SafeSonarColour;
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

    // Update is called once per frame
    void Update()
    {
        List<int2> currentObjectsToRemove = new(objectsToRemove);
        foreach (int2 removeObject in currentObjectsToRemove)
        {
            if (currentTrash.ContainsKey(removeObject.x))
            {
                if (currentTrash[removeObject.x].ElementAtOrDefault(removeObject.y) != null)
                {
                    Destroy(currentTrash[removeObject.x][removeObject.y]);
                    objectsToRemove.Remove(removeObject);
                    currentTrash[removeObject.x].RemoveAt(removeObject.y);
                    currentTrash[removeObject.x].TrimExcess();
                    loadedTrash[removeObject.x].RemoveAt(removeObject.y);
                    loadedTrash[removeObject.x].TrimExcess();
                }
            }
        }
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
            speeds = speeds, startSides = startSides, yPos = yPos, time = Time.time, offsets = offsets, maxBob = maxBob, bobSpeed = bobSpeed, rotationSpeed = rotationSpeed
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
                int tempSpriteIndex;
                bool isDangerous = UnityEngine.Random.Range(0, 100) > (100 - Mathf.Clamp(index, 0, maxIsDangerousChance));
                if (!isDangerous)
                {
                    tempSpriteIndex = UnityEngine.Random.Range(0, trashSprites.Count);
                }
                else
                {
                    tempSpriteIndex = UnityEngine.Random.Range(0, dangerousTrashSprites.Count);
                }
                loadedTrash[index].Add(new Trash()
                {
                    speed = UnityEngine.Random.Range(minSpeed, Mathf.Clamp(((float)index / maxTrashDepth) * maxSpeed, minSpeed, maxSpeed)),
                    startSide = Mathf.Sign(UnityEngine.Random.value - 0.5f),
                    yPos = (UnityEngine.Random.value + index) * chunkHeight,
                    isDangerous = isDangerous,
                    spriteIndex = tempSpriteIndex,
                    offset = UnityEngine.Random.value * 360f
                });
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
        newTrash.GetComponent<SpriteRenderer>().sprite = (trash.isDangerous) ? dangerousTrashSprites[trash.spriteIndex] : trashSprites[trash.spriteIndex];
        if (trash.isDangerous)
        {
            newTrash.GetComponent<Animator>().SetInteger("TrashIndex", trash.spriteIndex + 1);
            newTrash.GetComponent<SpriteRenderer>().sortingOrder = 6;
            newTrash.transform.GetChild(0).GetComponent<SpriteRenderer>().color = dangerousSonarColour;
        }
        else
        {
            newTrash.GetComponent<Animator>().enabled = false;
            if (GameManager.Instance.upgrades[0][2] > 1)
            {
                newTrash.transform.GetChild(0).GetComponent<SpriteRenderer>().color = SafeSonarColour;
            }
            else
            {
                newTrash.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.clear;
            }
        }
        PolygonCollider2D newCollider = newTrash.AddComponent<PolygonCollider2D>();
        newCollider.isTrigger = true;
    }

    [BurstCompile]
    public struct TrashScroller : IJobParallelForTransform
    {
        public NativeArray<float> speeds, startSides, yPos, offsets;
        public float time, maxBob, bobSpeed, rotationSpeed;

        public void Execute(int index, TransformAccess transform)
        {
            transform.position = new float3(startSides[index] * 32 * math.sin(time * speeds[index] + offsets[index]), -yPos[index] + maxBob * math.sin(offsets[index] + bobSpeed * time), 0);
            transform.rotation = Quaternion.AngleAxis(time * rotationSpeed * speeds[index], new float3(0, 0, (offsets[index] - 180f) / math.abs(offsets[index] - 180f)));
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
    public int spriteIndex;
    public float offset;
}