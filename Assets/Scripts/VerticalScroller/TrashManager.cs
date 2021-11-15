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
    public int minTrash, maxTrash;
    public float minspeed, maxspeed;
    public GameObject trashPrefab;
    public List<Sprite> trashSprites;

    private Dictionary<int, List<GameObject>> currentTrash = new Dictionary<int, List<GameObject>>();
    private Dictionary<int, List<Trash>> loadedTrash = new Dictionary<int, List<Trash>>();
    private int chunkHeight;
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
                yPos[counter] = currentTrashList[i].yPos;
                transforms[counter] = currentObjectList[i].transform;
                counter++;
            }
        }
        trashTransforms.SetTransforms(transforms);
        TrashScroller trashScroller = new TrashScroller
        {
            speeds = speeds, startSides = startSides, yPos = yPos, time = Time.time
        };
        JobHandle trashScrollerJob = trashScroller.Schedule(trashTransforms);
        trashScrollerJob.Complete();
        speeds.Dispose();
        yPos.Dispose();
        startSides.Dispose();
    }

    public void ToggleTrash(int chunkIndex, bool isEnabled)
    {
        if (isEnabled)
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
        }
        for (int i = 0; i < UnityEngine.Random.Range(minTrash, maxTrash); i++)
        {
            loadedTrash[index].Add(new Trash() { speed = UnityEngine.Random.Range(minspeed, maxspeed), startSide = Mathf.Sign(UnityEngine.Random.value - 1), yPos = (UnityEngine.Random.value + index) * chunkHeight });
        }
        foreach (Trash trash in loadedTrash[index])
        {
            InstantiateTrash(trash, index);
        }
    }

    private void InstantiateTrash(Trash trash, int chunkIndex)
    {
        GameObject newTrash = Instantiate(trashPrefab, Vector3.down * trash.yPos, Quaternion.identity, gameObject.transform);
        newTrash.GetComponent<SpriteRenderer>().sprite = trashSprites[UnityEngine.Random.Range(0, trashSprites.Count)];
        if (!currentTrash.ContainsKey(chunkIndex))
        {
            currentTrash[chunkIndex] = new List<GameObject>();
        }
        currentTrash[chunkIndex].Add(newTrash);
    }

    public struct TrashScroller : IJobParallelForTransform
    {
        public NativeArray<float> speeds, startSides, yPos;
        public float time;

        public void Execute(int index, TransformAccess transform)
        {
            transform.position = new float3(startSides[index] * 32 * math.sin(time * speeds[index]), -yPos[index], 0);
        }
    }

    [BurstCompile]
    public struct Trash
    {
        public float speed;
        public float startSide;
        public float yPos;
    }
    private void OnDisable()
    {
        trashTransforms.Dispose();
    }
}
