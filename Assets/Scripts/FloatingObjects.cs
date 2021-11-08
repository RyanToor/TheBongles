using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Burst;

public class FloatingObjects : MonoBehaviour
{
    public float maxAngle, maxYDisplacement;

    [HideInInspector]
    public List<GameObject> objectsToRemove = new List<GameObject>(), objectsToAdd = new List<GameObject>();

    private List<GameObject> floatingObjects = new List<GameObject>();
    private NativeList<float> seeds;
    private NativeList<float3> startPosList;
    public TransformAccessArray transformAccessArray;

    // Start is called before the first frame update
    void Start()
    {
        transformAccessArray = new TransformAccessArray(1);
        startPosList = new NativeList<float3>(0, Allocator.Persistent);
        seeds = new NativeList<float>(0, Allocator.Persistent);
        for (int i = 0; i < floatingObjects.Count; i++)
        {
            Vector3 floatingObjectPosition = floatingObjects[i].transform.position;
            startPosList.Add(new float3(floatingObjects[i].transform.position));
            seeds.Add(UnityEngine.Random.value * 2 * math.PI);
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject objectToAdd in objectsToAdd)
        {
            floatingObjects.Add(objectToAdd);
            seeds.Add(UnityEngine.Random.Range(0, 100000));
            transformAccessArray.Add(objectToAdd.transform);
            startPosList.Add(objectToAdd.transform.position);
        }
        foreach (GameObject objectToRemove in objectsToRemove)
        {
            int indexToRemove = floatingObjects.IndexOf(objectToRemove);
            floatingObjects.RemoveAt(indexToRemove);
            seeds.RemoveAt(indexToRemove);
            startPosList.RemoveAt(indexToRemove);
            Destroy(objectToRemove);
        }
        Transform[] currentTransforms = new Transform[floatingObjects.Count];
        for (int i = 0; i < floatingObjects.Count; i++)
        {
            currentTransforms[i] = floatingObjects[i].transform;
        }
        transformAccessArray.SetTransforms(currentTransforms);
        objectsToAdd.Clear();
        objectsToRemove.Clear();
        CalculateFloatTransform calculateFloatTransform = new CalculateFloatTransform
        {
            time = Time.time, maxAngle = maxAngle, maxYDisplacement = maxYDisplacement,
            startPosArray = startPosList,
            seeds = seeds
        };
        JobHandle floatingObjectsJob = calculateFloatTransform.Schedule(transformAccessArray);
        floatingObjectsJob.Complete();
    }

    [BurstCompile]
    private struct CalculateFloatTransform : IJobParallelForTransform
    {
        [ReadOnly]public NativeArray<float3> startPosArray;
        [ReadOnly]public NativeList<float> seeds;
        public float maxAngle, maxYDisplacement, time;

        public void Execute(int index, TransformAccess transform)
        {
            transform.position = startPosArray[index] + new float3(0, maxYDisplacement * math.sin(time + seeds[index]), 0);
            transform.localRotation = Quaternion.SlerpUnclamped(Quaternion.AngleAxis(-maxAngle, Vector3.forward), Quaternion.AngleAxis(maxAngle, Vector3.forward), (math.cos(time + seeds[index]) + 1) / 2);
        }
    }

    private void OnApplicationQuit()
    {
        seeds.Dispose();
        startPosList.Dispose();
        transformAccessArray.Dispose();
    }
}