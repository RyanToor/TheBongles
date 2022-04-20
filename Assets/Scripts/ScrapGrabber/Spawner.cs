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
    public float startSpeed, speedIncreaseRate, endTime, maxAngle, maxYDisplacement, escapeSpeedMultiplier;
    public SpawnSet[] spawnSets;
    public bool spawnLeft, spawnRight;

    [HideInInspector]
    public List<GameObject> spawnedObjects = new List<GameObject>(), objectsToRemove = new List<GameObject>(), destroyRequests = new List<GameObject>();

    private float speed;
    private float[] timesSinceLastSpawn, spawnChangeDurations, nextRandomIntervals;
    private Vector2 prevSpawnPos, prevSpawnDim, isPrevSpawnLeft;
    private Bounds playArea;
    private TransformAccessArray transformAccessArray;
    private NativeList<bool> rotatingObjects, moveVertical, isEscaping;
    private NativeList<float> speedMultipliers, seeds;
    private NativeList<float3> startPosList;
    private List<bool> willEscape = new List<bool>();
    private bool[] isRandomIntervalSet;
    private LevelManager_ScrapGrabber levelManager;

    // Start is called before the first frame update
    void Start()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_ScrapGrabber>();
        transformAccessArray = new TransformAccessArray(1);
        speedMultipliers = new NativeList<float>(0, Allocator.Persistent);
        seeds = new NativeList<float>(0, Allocator.Persistent);
        startPosList = new NativeList<float3>(0, Allocator.Persistent);
        rotatingObjects = new NativeList<bool>(0, Allocator.Persistent);
        moveVertical = new NativeList<bool>(0, Allocator.Persistent);
        isEscaping = new NativeList<bool>(0, Allocator.Persistent);
        playArea = GetComponent<BoxCollider2D>().bounds;
        timesSinceLastSpawn = new float[spawnSets.Length];
        spawnChangeDurations = new float[spawnSets.Length];
        nextRandomIntervals = new float[spawnSets.Length];
        isRandomIntervalSet = new bool[spawnSets.Length];
        for (int i = 0; i < spawnSets.Length; i++)
        {
            spawnChangeDurations[i] = spawnSets[i].timeEnd - spawnSets[i].timeStart;
        }
    }

    // Update is called once per frame
    void Update()
    {
        speed = startSpeed + speedIncreaseRate * Time.timeSinceLevelLoad;
        for (int i = 0; i < spawnSets.Length; i++)
        {
            if (!levelManager.isBellActive || levelManager.isBellActive && !spawnSets[i].isDisabledByBell)
            {
                timesSinceLastSpawn[i] += Time.deltaTime;
            }
            if (CheckSpawn(i))
            {
                Spawn(i);
            }
        }
        CalculateObjectTransform calculateFloatTransform = new CalculateObjectTransform
        {
            time = Time.time,
            deltaTime = Time.deltaTime,
            maxAngle = maxAngle,
            maxYDisplacement = maxYDisplacement,
            startPosList = startPosList,
            rotatingObjects = rotatingObjects,
            speedMultipliers = speedMultipliers,
            seeds = seeds,
            speed = speed,
            moveVertical = moveVertical,
            isEscaping = isEscaping
        };
        JobHandle floatingObjectsJob = calculateFloatTransform.Schedule(transformAccessArray);
        floatingObjectsJob.Complete();
        if (Application.isEditor)
        {
            EditorUpdate();
        }
    }

    private void LateUpdate()
    {
        List<GameObject> objectsToDestroy = new List<GameObject>();
        foreach (GameObject objectToDestroy in destroyRequests)
        {
            objectsToDestroy.Add(objectToDestroy);
        }
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (Mathf.Abs(spawnedObjects[i].transform.position.x) > playArea.extents.x + 1 || spawnedObjects[i].transform.position.y > 6f)
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

    private bool CheckSpawn(int spawnSetIndex)
    {
        SpawnSet spawnSet = spawnSets[spawnSetIndex];
        float spawnChangeFraction = (Time.timeSinceLevelLoad - spawnSet.timeStart) / spawnChangeDurations[spawnSetIndex];
        if (Time.timeSinceLevelLoad < spawnSet.timeStart && !spawnSet.spawnBeforeStart)
        {
            return false;
        }
        else if (Time.timeSinceLevelLoad > spawnSet.timeEnd && !spawnSet.spawnAfterEnd)
        {
            return false;
        }
        else if (timesSinceLastSpawn[spawnSetIndex] > Mathf.Lerp(spawnSet.startMinInterval, spawnSet.endMinInterval, spawnChangeFraction))
        {
            if (!isRandomIntervalSet[spawnSetIndex])
            {
                nextRandomIntervals[spawnSetIndex] = UnityEngine.Random.Range(Mathf.Lerp(spawnSet.startMinInterval, spawnSet.endMinInterval, spawnChangeFraction), Mathf.Lerp(spawnSet.startMaxInterval, spawnSet.endMaxInterval, spawnChangeFraction));
                isRandomIntervalSet[spawnSetIndex] = true;
            }
            if (timesSinceLastSpawn[spawnSetIndex] - nextRandomIntervals[spawnSetIndex] > Mathf.Lerp(spawnSet.startMinInterval, spawnSet.endMinInterval, spawnChangeFraction))
            {
                isRandomIntervalSet[spawnSetIndex] = false;
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    private void Spawn(int spawnSetIndex)
    {
        timesSinceLastSpawn[spawnSetIndex] = 0;
        SpawnSet spawnSet = spawnSets[spawnSetIndex];
        GameObject objectToSpawn = spawnSet.prefabs[UnityEngine.Random.Range(0, spawnSet.prefabs.Length)];
        bool isSpawnLeft = UnityEngine.Random.value > 0.5f;
        Vector3 spawnPoint;
        if (spawnSet.moveVertical)
        {
            spawnPoint = new Vector3(UnityEngine.Random.Range(-5.4f, 5.4f), playArea.min.y - 3f);
        }
        else
        {
            spawnPoint = new Vector3(spawnLeft && spawnRight ? isSpawnLeft ? playArea.min.x : playArea.max.x : spawnLeft ? playArea.min.x : playArea.max.x, UnityEngine.Random.Range(playArea.min.y, playArea.max.y));
        }
        GameObject newObject = Instantiate(objectToSpawn, spawnPoint, Quaternion.identity, transform);
        newObject.transform.localScale = Vector3.Scale(newObject.transform.localScale, newObject.transform.position.x > 0 ? new Vector3(-1, 1, 1) : Vector3.one);
        rotatingObjects.Add(spawnSet.isRotatabale);
        moveVertical.Add(spawnSet.moveVertical);
        isEscaping.Add(false);
        spawnedObjects.Add(newObject);
        transformAccessArray.Add(newObject.transform);
        startPosList.Add(newObject.transform.position);
        speedMultipliers.Add(UnityEngine.Random.Range(spawnSet.minSpeedMultiplier, spawnSet.maxSpeedMultiplier) * newObject.transform.position.x > 0 && !spawnSet.moveVertical ? -1 : 1);
        seeds.Add(UnityEngine.Random.Range(0, 100000));
        willEscape.Add(spawnSet.willEscape);
        if (newObject.TryGetComponent(out Obstacle obstacleScript))
        {
            obstacleScript.spawner = this;
        }
        else
        {
            foreach (Transform child in newObject.transform)
            {
                if (child.TryGetComponent(out Obstacle childObstacleScript))
                {
                    childObstacleScript.spawner = this;
                }
            }
        }
        timesSinceLastSpawn[spawnSetIndex] = 0;
    }

    private void RemoveFromLists(GameObject objectToRemove)
    {
        int indexToRemove;
        if (spawnedObjects.Contains(objectToRemove))
        {
            indexToRemove = spawnedObjects.IndexOf(objectToRemove);
        }
        else
        {
            indexToRemove = -1;
        }
        if (indexToRemove >= 0)
        {
            spawnedObjects.RemoveAtSwapBack(indexToRemove);
            transformAccessArray.RemoveAtSwapBack(indexToRemove);
            seeds.RemoveAtSwapBack(indexToRemove);
            startPosList.RemoveAtSwapBack(indexToRemove);
            speedMultipliers.RemoveAtSwapBack(indexToRemove);
            rotatingObjects.RemoveAtSwapBack(indexToRemove);
            willEscape.RemoveAtSwapBack(indexToRemove);
            moveVertical.RemoveAtSwapBack(indexToRemove);
            isEscaping.RemoveAtSwapBack(indexToRemove);
        }
    }

    public void Escape(int index = -1)
    {
        List<int> escapingIndicies = new List<int>();
        if (index < 0)
        {
            for (int i = 0; i < willEscape.Count; i++)
            {
                if (willEscape[i])
                {
                    escapingIndicies.Add(i);
                    isEscaping[i] = true;
                }
            }
        }
        else
        {
            if (willEscape[index])
            {
                escapingIndicies.Add(index);
                willEscape[index] = false;
                isEscaping[index] = true;
            }
        }
        for (int i = 0; i < escapingIndicies.Count; i++)
        {
            if (moveVertical[escapingIndicies[i]])
            {
                spawnedObjects[escapingIndicies[i]].GetComponent<Animator>().SetTrigger("Escape");
                speedMultipliers[escapingIndicies[i]] = Mathf.Abs(speedMultipliers[i]) * escapeSpeedMultiplier;
            }
            else
            {
                if (spawnedObjects[escapingIndicies[i]].transform.position.x >= 0)
                {
                    if (Mathf.Sign(speedMultipliers[escapingIndicies[i]]) < 0)
                    {
                        spawnedObjects[escapingIndicies[i]].GetComponent<Animator>().SetTrigger("Escape");
                    }
                    speedMultipliers[escapingIndicies[i]] = Mathf.Abs(speedMultipliers[i]) * escapeSpeedMultiplier;
                }
                else
                {
                    if (Mathf.Sign(speedMultipliers[escapingIndicies[i]]) > 0)
                    {
                        spawnedObjects[escapingIndicies[i]].GetComponent<Animator>().SetTrigger("Escape");
                    }
                    speedMultipliers[escapingIndicies[i]] = Mathf.Abs(speedMultipliers[i]) * -escapeSpeedMultiplier;
                }
            }
            spawnedObjects[escapingIndicies[i]].GetComponent<Animator>().speed = escapeSpeedMultiplier;
            AudioManager.Instance.PlayAudioAtObject("FishEscaping", gameObject, 20, false);
        }
    }

    [BurstCompile]
    private struct CalculateObjectTransform : IJobParallelForTransform
    {
        [ReadOnly] public NativeList<bool> rotatingObjects, moveVertical, isEscaping;
        [ReadOnly] public NativeList<float> speedMultipliers, seeds;
        [ReadOnly] public NativeList<float3> startPosList;
        public float maxAngle, maxYDisplacement, time, deltaTime, speed;

        public void Execute(int index, TransformAccess transform)
        {
            if (moveVertical[index])
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + speedMultipliers[index] * speed * deltaTime * (isEscaping[index]? 1 : ((math.sin(time + seeds[index]) + 0.5f) / 2)), startPosList[index].z);
            }
            else
            {
                transform.position = new Vector3(transform.position.x + speedMultipliers[index] * speed * deltaTime, startPosList[index].y + maxYDisplacement * math.sin(time + seeds[index]), startPosList[index].z);
            }
            if (rotatingObjects[index] == true)
            {
                transform.localRotation = Quaternion.SlerpUnclamped(Quaternion.AngleAxis(-maxAngle, Vector3.forward), Quaternion.AngleAxis(maxAngle, Vector3.forward), (math.cos(time + seeds[index]) + 1) / 2);
            }
        }
    }

    private void EditorUpdate()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Escape();
        }
    }

    private void OnDisable()
    {
        speedMultipliers.Dispose();
        startPosList.Dispose();
        seeds.Dispose();
        rotatingObjects.Dispose();
        moveVertical.Dispose();
        isEscaping.Dispose();
        transformAccessArray.Dispose();
    }

    [System.Serializable]
    public struct SpawnSet
    {
        public string name;
        public bool isDisabledByBell, moveVertical, isRotatabale, willEscape, spawnBeforeStart, spawnAfterEnd;
        public GameObject[] prefabs;
        public float timeStart, timeEnd, minSpeedMultiplier, maxSpeedMultiplier, startMinInterval, endMinInterval, startMaxInterval, endMaxInterval;
    }
}
