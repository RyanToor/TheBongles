using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

public class Clouds : MonoBehaviour
{
    public float radius, displacementSpeed, wiggleAmplitude, wiggleSpeed, cloudBorder;
    public GameObject cloudObject, map;

    private float width, height, angle;
    private float3 displacement;
    private int2 moveDir;
    private List<GameObject> clouds = new List<GameObject>();
    NativeArray<int> seeds;
    List<float3> basePosList = new List<float3>();

    // Start is called before the first frame update
    void Start()
    {
        angle = UnityEngine.Random.Range(0, 2 * math.PI);
        displacement = new float3(displacementSpeed * Mathf.Cos(angle), displacementSpeed * Mathf.Sin(angle), 0);
        moveDir = new int2((int)math.sign(displacement.x), (int)math.sign(displacement.y));
        width = map.GetComponent<SpriteRenderer>().bounds.size.x + cloudBorder;
        height = map.GetComponent<SpriteRenderer>().bounds.size.y + cloudBorder;
        PoissonDiscSampler sampler = new PoissonDiscSampler(width, height, radius);
        int cloudAlpha = PlayerPrefs.GetInt("isLoaded", 1);
        foreach (Vector2 sample in sampler.Samples())
        {
            Vector2 samplePos = new Vector2(sample.x - (width / 2), sample.y - (height / 2));
            if (!(samplePos.magnitude > height))
            {
                GameObject newCloud = (GameObject)Instantiate(cloudObject, new Vector3(samplePos.x, samplePos.y, 0), Quaternion.identity, gameObject.transform);
                newCloud.GetComponent<Cloud>().currentAlpha = cloudAlpha;
                clouds.Add(newCloud);
            }
        }
        seeds = new NativeArray<int>(clouds.Count, Allocator.Persistent);
        for (int i = 0; i < clouds.Count; i++)
        {
            seeds[i] = UnityEngine.Random.Range(0, 100000);
            basePosList.Add(clouds[i].transform.position);
        }
        print(clouds.Count);
    }

    private void Update()
    {
        NativeArray<float3> basePos = new NativeArray<float3>(basePosList.Count, Allocator.TempJob);
        NativeArray<float3> positions = new NativeArray<float3>(basePosList.Count, Allocator.TempJob);

        for (int i = 0; i < basePosList.Count; i++)
        {
            basePos[i] = basePosList[i];
        }

        CalculateCloudPosition job = new CalculateCloudPosition
        {
            displacement = displacement,
            cloudsWidth = width,
            cloudsHeight = height,
            wiggleAmplitude = wiggleAmplitude,
            wiggleSpeed = wiggleSpeed,
            time = Time.time,
            deltaTime = Time.deltaTime,
            moveDir = moveDir,
            seeds = seeds,
            basePos = basePos,
            positions = positions,
        };
        JobHandle handle = job.Schedule(clouds.Count, 500);
        handle.Complete();
        
        for (int i = 0; i < basePosList.Count; i++)
        {
            clouds[i].transform.position = positions[i];
            basePosList[i] = basePos[i];
        }
        positions.Dispose();
        basePos.Dispose();
    }

    [BurstCompile]
    private struct CalculateCloudPosition : IJobParallelFor
    {
        public NativeArray<float3> basePos, positions;
        public float3 displacement;
        public float cloudsWidth, cloudsHeight, wiggleAmplitude, wiggleSpeed, time, deltaTime;
        public int2 moveDir;
        public NativeArray<int> seeds;
        public void Execute(int i)
        {
            basePos[i] += displacement * deltaTime;
            if (basePos[i].x > cloudsWidth / 2 && moveDir.x > 0 || basePos[i].x < -cloudsWidth / 2 && moveDir.x < 0)
            {
                basePos[i] = new float3(basePos[i].x - moveDir.x * cloudsWidth, basePos[i].y, basePos[i].z);
            }
            else if (basePos[i].y > cloudsHeight / 2 && moveDir.y > 0 || basePos[i].y < -cloudsHeight / 2 && moveDir.y < 0)
            {
                basePos[i] = new float3(basePos[i].x, basePos[i].y - moveDir.y * cloudsHeight, basePos[i].z);
            }
            positions[i] = new float3((wiggleAmplitude * (Mathf.PerlinNoise(wiggleSpeed * time, seeds[i]) - 0.5f)) + basePos[i].x, basePos[i].y, basePos[i].z);
        }
    }

    /*private void OnApplicationQuit()
    {
        seeds.Dispose();
    */

    private void OnDisable()
    {
        seeds.Dispose();
    }
}