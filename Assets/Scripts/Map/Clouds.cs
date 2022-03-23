using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

public class Clouds : MonoBehaviour
{
    public float radius, displacementSpeed, wiggleAmplitude, wiggleSpeed, cloudBorder, windSystemsEmissionRate;
    public GameObject cloudObject, map;
    public Transform[] windSystems;

    private float width, height, angle;
    private float3 displacement;
    private int2 moveDir;
    private List<GameObject> clouds = new List<GameObject>(), tempClouds = new List<GameObject>();
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
        foreach (Vector2 sample in sampler.Samples())
        {
            Vector2 samplePos = new Vector2(sample.x - (width / 2), sample.y - (height / 2));
            if (!(samplePos.magnitude > height))
            {
                GameObject newCloud = (GameObject)Instantiate(cloudObject, new Vector3(samplePos.x, samplePos.y, 0), Quaternion.identity, gameObject.transform);
                clouds.Add(newCloud);
            }
        }
        seeds = new NativeArray<int>(clouds.Count, Allocator.Persistent);
        for (int i = 0; i < clouds.Count; i++)
        {
            seeds[i] = UnityEngine.Random.Range(0, 100000);
            basePosList.Add(clouds[i].transform.position);
        }
        foreach (Transform windSystem in windSystems)
        {
            windSystem.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
            ParticleSystem.EmissionModule emissionRate = windSystem.gameObject.GetComponent<ParticleSystem>().emission;
            emissionRate.rateOverTime = windSystemsEmissionRate / windSystems.Length;
        }
        Debug.Log(clouds.Count + " Clouds Instantiated at " + angle + " Radians.");
    }

    private void Update()
    {
        NativeArray<float3> basePos = new NativeArray<float3>(basePosList.Count, Allocator.TempJob);
        NativeArray<float3> positions = new NativeArray<float3>(basePosList.Count, Allocator.TempJob);
        NativeArray<bool> teleportedClouds = new NativeArray<bool>(basePosList.Count, Allocator.TempJob);

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
            teleportedClouds = teleportedClouds
        };
        JobHandle handle = job.Schedule(clouds.Count, 500);
        handle.Complete();
        
        for (int i = 0; i < basePosList.Count; i++)
        {
            if (teleportedClouds[i])
            {
                clouds[i].GetComponent<Cloud>().currentAlpha = 0;
                Color col = clouds[i].transform.Find("Sprite").GetComponent<SpriteRenderer>().color;
                clouds[i].transform.Find("Sprite").GetComponent<SpriteRenderer>().color = new Color(col.r, col.g, col.b, 0);
                StartCoroutine(clouds[i].GetComponent<Cloud>().Fade(1));
                GameObject newCloud = Instantiate(cloudObject, clouds[i].transform.position, clouds[i].transform.rotation, gameObject.transform);
                StartCoroutine(newCloud.GetComponent<Cloud>().Fade(-1));
                newCloud.transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = clouds[i].transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite;
                newCloud.GetComponent<Cloud>().originalCloud = false;
                tempClouds.Add(newCloud);
            }
            clouds[i].transform.position = positions[i];
            basePosList[i] = basePos[i];
        }
        positions.Dispose();
        basePos.Dispose();
        teleportedClouds.Dispose();
        List<GameObject> oldClouds = new List<GameObject>();
        foreach (GameObject cloud in tempClouds)
        {
            if (cloud.transform.Find("Sprite").GetComponent<SpriteRenderer>().color.a == 0)
            {
                oldClouds.Add(cloud);
            }
        }
        foreach (GameObject cloud in oldClouds)
        {
            tempClouds.Remove(cloud);
            Destroy(cloud);
        }
    }

    [BurstCompile]
    private struct CalculateCloudPosition : IJobParallelFor
    {
    public NativeArray<float3> basePos, positions;
        public NativeArray<bool> teleportedClouds;
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
                teleportedClouds[i] = true;
            }
            else if (basePos[i].y > cloudsHeight / 2 && moveDir.y > 0 || basePos[i].y < -cloudsHeight / 2 && moveDir.y < 0)
            {
                basePos[i] = new float3(basePos[i].x, basePos[i].y - moveDir.y * cloudsHeight, basePos[i].z);
                teleportedClouds[i] = true;
            }
            else
            {
                teleportedClouds[i] = false;
            }
            positions[i] = new float3((wiggleAmplitude * (Mathf.PerlinNoise(wiggleSpeed * time, seeds[i]) - 0.5f)) + basePos[i].x, basePos[i].y, basePos[i].z);
        }
    }

    private void OnDisable()
    {
        seeds.Dispose();
    }
}