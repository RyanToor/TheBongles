using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Burst;

public class Spawner : MonoBehaviour
{
    [Range(0, 100)]
    public float startObstacleSpawnChance, endObstacleSpawnChance;
    public float startSpeed, minSpeedMultiplier, maxSpeedMultiplier, speedIncreaseRate, startMinSpawnPeriod, endMinSpawnPeriod, startMaxSpawnPeriod, endMaxSpawnPeriod, endTime, maxAngle, maxYDisplacement;
    public GameObject[] collectables, obstacles;
    public bool spawnLeft, spawnRight;

    [HideInInspector]
    public List<GameObject> spawnedObjects = new List<GameObject>(), objectsToRemove = new List<GameObject>();

    private float timeSinceLastSpawn, nextSpawnPeriod, speed;
    private Vector2 prevSpawnPos, prevSpawnDim, isPrevSpawnLeft;
    private Bounds playArea;
    private TransformAccessArray transformAccessArray;
    private NativeList<bool> rotatingObjects;
    private NativeList<float> speedMultipliers;
    private NativeList<float> seeds;
    private NativeList<float> ages;
    private NativeList<float3> startPosList;

    // Start is called before the first frame update
    void Start()
    {
        transformAccessArray = new TransformAccessArray(1);
        speedMultipliers = new NativeList<float>(0, Allocator.Persistent);
        seeds = new NativeList<float>(0, Allocator.Persistent);
        ages = new NativeList<float>(0, Allocator.Persistent);
        startPosList = new NativeList<float3>(0, Allocator.Persistent);
        rotatingObjects = new NativeList<bool>(0, Allocator.Persistent);
        playArea = GetComponent<BoxCollider2D>().bounds;
        Spawn();
    }

    // Update is called once per frame
    void Update()
    {
        speed = startSpeed + speedIncreaseRate * Time.timeSinceLevelLoad;
        for (int i = 0; i < ages.Length; i++)
        {
            ages[i] += Time.deltaTime;
        }
        timeSinceLastSpawn += Time.deltaTime;
        if (timeSinceLastSpawn >= nextSpawnPeriod)
        {
            Spawn();
        }
        CalculateObjectTransform calculateFloatTransform = new CalculateObjectTransform
        {
            time = Time.time,
            maxAngle = maxAngle,
            maxYDisplacement = maxYDisplacement,
            startPosList = startPosList,
            rotatingObjects = rotatingObjects,
            speedMultipliers = speedMultipliers,
            ages = ages,
            seeds = seeds,
            speed = speed
        };
        JobHandle floatingObjectsJob = calculateFloatTransform.Schedule(transformAccessArray);
        floatingObjectsJob.Complete();
    }

    private void LateUpdate()
    {
        List<GameObject> objectsToDestroy = new List<GameObject>();
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (Mathf.Abs(spawnedObjects[i].transform.position.x) > playArea.extents.x + 1)
            {
                objectsToDestroy.Add(spawnedObjects[i]);
            }
        }
        foreach (GameObject objectToDestroy in objectsToDestroy)
        {
            RemoveFromLists(objectToDestroy);
            Destroy(objectToDestroy);
        }
        foreach (GameObject objectToRemove in objectsToRemove)
        {
            RemoveFromLists(objectToRemove);
        }
        objectsToRemove.Clear();
    }

    private void RemoveFromLists(GameObject objectToRemove)
    {
        int indexToRemove = spawnedObjects.IndexOf(objectToRemove);
        if (indexToRemove >= 0)
        {
            spawnedObjects.RemoveAtSwapBack(indexToRemove);
            transformAccessArray.RemoveAtSwapBack(indexToRemove);
            seeds.RemoveAtSwapBack(indexToRemove);
            ages.RemoveAtSwapBack(indexToRemove);
            startPosList.RemoveAtSwapBack(indexToRemove);
            speedMultipliers.RemoveAtSwapBack(indexToRemove);
            rotatingObjects.RemoveAtSwapBack(indexToRemove);
        }
    }

    private void Spawn()
    {
        bool spawnObstacle = UnityEngine.Random.Range(0, 100) < Mathf.Lerp(startObstacleSpawnChance, endObstacleSpawnChance, Time.timeSinceLevelLoad / endTime);
        GameObject objectToSpawn =  spawnObstacle? obstacles[UnityEngine.Random.Range(0, obstacles.Length)] : collectables[UnityEngine.Random.Range(0, collectables.Length)];
        bool isSpawnLeft = UnityEngine.Random.value > 0.5f;
        GameObject newObject = Instantiate(objectToSpawn, new Vector3(spawnLeft && spawnRight ? isSpawnLeft ? playArea.min.x : playArea.max.x : spawnLeft ? playArea.min.x : playArea.max.x, UnityEngine.Random.Range(playArea.min.y, playArea.max.y)), Quaternion.identity, transform);
        if (newObject.transform.position.x > 0)
        {
            newObject.transform.localScale = Vector3.Scale(newObject.transform.localScale, new Vector3(-1, 1, 1));
        }
        if (spawnObstacle)
        {
            rotatingObjects.Add(false);
        }
        else
        {
            rotatingObjects.Add(true);
        }
        spawnedObjects.Add(newObject);
        transformAccessArray.Add(newObject.transform);
        ages.Add(0);
        startPosList.Add(newObject.transform.position);
        speedMultipliers.Add(UnityEngine.Random.Range(minSpeedMultiplier, maxSpeedMultiplier) * newObject.transform.position.x > 0 ? -1 : 1);
        seeds.Add(UnityEngine.Random.Range(0, 100000));
        nextSpawnPeriod = UnityEngine.Random.Range(Mathf.Lerp(startMinSpawnPeriod, endMinSpawnPeriod, Time.timeSinceLevelLoad / endTime), Mathf.Lerp(startMaxSpawnPeriod, endMaxSpawnPeriod, Time.timeSinceLevelLoad / endTime));
        timeSinceLastSpawn = 0;
    }

    [BurstCompile]
    private struct CalculateObjectTransform : IJobParallelForTransform
    {
        [ReadOnly] public NativeList<bool> rotatingObjects;
        [ReadOnly] public NativeList<float> speedMultipliers;
        [ReadOnly] public NativeList<float3> startPosList;
        [ReadOnly] public NativeList<float> seeds;
        [ReadOnly] public NativeList<float> ages;
        public float maxAngle, maxYDisplacement, time, speed;

        public void Execute(int index, TransformAccess transform)
        {
            transform.position = startPosList[index] + new float3(0, maxYDisplacement * math.sin(time + seeds[index]), 0) + new float3(speedMultipliers[index] * speed * ages[index], 0, 0);
            if (rotatingObjects[index] == true)
            {
                transform.localRotation = Quaternion.SlerpUnclamped(Quaternion.AngleAxis(-maxAngle, Vector3.forward), Quaternion.AngleAxis(maxAngle, Vector3.forward), (math.cos(time + seeds[index]) + 1) / 2);
            }
        }
    }

    private void OnDisable()
    {
        ages.Dispose();
        speedMultipliers.Dispose();
        startPosList.Dispose();
        speedMultipliers.Dispose();
        rotatingObjects.Dispose();
        transformAccessArray.Dispose();
    }
}
