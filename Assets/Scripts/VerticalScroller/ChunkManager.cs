using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Tilemaps;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using System.Linq;
using System.Collections;
using System;

public class ChunkManager : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase ruleTile;
    public GameObject player, chunk;
    public int chunkBuffer, width, height, maxChasmDeviation, maxIrregularity, rowsConvertedPerFrame, tilesPlacedPerFrame;
    public float chasmStartWidth, chasmMinWidth, chasmWidthAsymptoteLevel, chasmFrequency, irregularityFrequency;

    private int widthSeed, irregularitySeedL, irregularitySeedR, currentChunk;
    private List<int> enabledChunks = new List<int>();
    private List<Vector3Int[]> chunkMatrices = new List<Vector3Int[]>();

    // Start is called before the first frame update
    void Start()
    {
        if (width - chasmStartWidth - maxChasmDeviation - 2 * maxIrregularity < 1)
        {
            maxChasmDeviation = width - 2 - (int)Mathf.Ceil(chasmStartWidth) - 2 * maxIrregularity;
        }
        widthSeed = UnityEngine.Random.Range(0, 100000);
        irregularitySeedL = UnityEngine.Random.Range(0, 100000);
        irregularitySeedR = UnityEngine.Random.Range(0, 100000);
        CheckChunks(0);
    }

    // Update is called once per frame
    void Update()
    {
        int newChunk = (int)Mathf.Floor(Mathf.Abs(player.transform.position.y) / height);
        if (newChunk != currentChunk)
        {
            print(currentChunk + "--->" + newChunk);
            currentChunk = newChunk;
            CheckChunks(currentChunk);
        }
    }

    private void CheckChunks(int currentChunkIndex)
    {
        for (int i = (int)math.clamp(currentChunk - chunkBuffer, 0, math.INFINITY); i <= currentChunk + chunkBuffer; i++)
        {
            if (chunkMatrices.Count > i)
            {
                if (!enabledChunks.Contains(i))
                {
                    StartCoroutine(LoadChunk(i, true));
                    enabledChunks.Add(i);
                }
            }
            else
            {
                GenerateChunk(i);
            }
        }
        for (int i = 0; i < enabledChunks.Count; i++)
        {
            int enabledChunkIndex = enabledChunks[i];
            if (enabledChunkIndex > currentChunkIndex + chunkBuffer || enabledChunkIndex < currentChunkIndex - chunkBuffer)
            {
                StartCoroutine(LoadChunk(enabledChunks[i], false));
            }
        }
    }

    private void GenerateChunk(int chunkIndex)
    {
        float chasmWidth = ((chasmStartWidth - chasmMinWidth) / (Mathf.Pow(chunkIndex, 2) * (1 / chasmWidthAsymptoteLevel) + 1)) + chasmMinWidth;
        NativeArray<int2> lRThickness = new NativeArray<int2>(height, Allocator.TempJob);

        CalculateLRThicknessJob calculateLRThickness = new CalculateLRThicknessJob
        {
            chunkIndex = chunkIndex,
            width = width,
            height = height,
            maxChasmDeviation = maxChasmDeviation,
            maxIrregularity = maxIrregularity,
            widthSeed = widthSeed,
            irregularitySeedL = irregularitySeedL,
            irregularitySeedR = irregularitySeedR,
            chasmWidth = chasmWidth,
            chasmWidthAsymptoteLevel = chasmWidthAsymptoteLevel,
            chasmFrequency = chasmFrequency,
            irregularityFrequency = irregularityFrequency,
            lRThickness = lRThickness
        };

        JobHandle handle = calculateLRThickness.Schedule(height, 10);
        handle.Complete();
        int tileCount = 0;
        foreach (int2 thicknessPair in lRThickness)
        {
            tileCount += thicknessPair.x + thicknessPair.y + 2;
        }
        int2[] tempArray = new int2[lRThickness.Length];
        lRThickness.CopyTo(tempArray);
        StartCoroutine(GenerateTileMatrix(chunkIndex, tempArray, tileCount));
        lRThickness.Dispose();
    }

    IEnumerator LoadChunk(int chunkMatrixIndex, bool isLoaded)
    {
        Vector3Int[] chunkMatrix = chunkMatrices[chunkMatrixIndex];
        TileBase[] fillTiles;
        int numOps = 0;
        switch (isLoaded)
        {
            case true: fillTiles = Enumerable.Repeat(ruleTile, chunkMatrix.Length).ToArray(); break;
            case false: fillTiles = Enumerable.Repeat<TileBase>(null, chunkMatrix.Length).ToArray(); print("Tried to Destroy Chunk"); break;
        }
        print(Mathf.Ceil(chunkMatrix.Length / tilesPlacedPerFrame));
        for (int i = 0; i < Mathf.Ceil(chunkMatrix.Length / tilesPlacedPerFrame); i++)
        {
            for (int j = numOps * tilesPlacedPerFrame; j < Mathf.Clamp((numOps + 1) * tilesPlacedPerFrame, 0, chunkMatrix.Length); j++)
            {
                Vector3Int[] currentTiles = new Vector3Int[tilesPlacedPerFrame];
                Array.Copy(chunkMatrix, numOps * tilesPlacedPerFrame, currentTiles, 0, tilesPlacedPerFrame);
                tilemap.SetTiles(currentTiles, fillTiles);
            }
            numOps++;
            yield return null;
        }
        if (!isLoaded)
        {
            enabledChunks.Remove(chunkMatrixIndex);
        }
    }

    IEnumerator GenerateTileMatrix(int chunkIndex, int2[] lRThickness, int tileCount)
    {
        int counter = 0;
        int runCount = 0;
        Vector3Int[] positions = new Vector3Int[tileCount];
        TileBase[] tiles = new TileBase[tileCount];
        for (int i = 0; i < Mathf.Ceil((float)height / rowsConvertedPerFrame); i++)
        {
            for (int row = runCount * rowsConvertedPerFrame; row < (runCount + 1) * rowsConvertedPerFrame; row++)
            {
                for (int j = 0; j <= lRThickness[row].x; j++)
                {
                    positions[counter] = new Vector3Int(j - width / 2, -row - chunkIndex * height, 0);
                    tiles[counter] = ruleTile;
                    counter++;
                }
                for (int j = 0; j < lRThickness[row].y; j++)
                {
                    positions[counter] = new Vector3Int(width / 2 - j, -row - chunkIndex * height, 0);
                    tiles[counter] = ruleTile;
                    counter++;
                }
            }
            runCount++;
            yield return null;
        }
        chunkMatrices.Insert(chunkIndex, positions);
        StartCoroutine(LoadChunk(chunkIndex, true));
    }

    [BurstCompile]
    private struct CalculateLRThicknessJob : IJobParallelFor
    {
        public int chunkIndex, width, height, maxChasmDeviation, maxIrregularity, widthSeed, irregularitySeedL, irregularitySeedR;
        public float chasmWidth, chasmWidthAsymptoteLevel, chasmFrequency, irregularityFrequency;
        public NativeArray<int2> lRThickness;
        public void Execute(int index)
        {
            int centrePoint = width / 2 + (int)math.round(noise.cnoise(new float2(widthSeed, (chunkIndex + (float)index / height) * chasmFrequency)) * maxChasmDeviation);
            int lThickness = (int)math.clamp((centrePoint - chasmWidth / 2 - maxIrregularity + math.round((noise.cnoise(new float2(irregularitySeedL, (chunkIndex * height + (float)index / height) * irregularityFrequency)) / 2 + 0.5f) * maxIrregularity)), 1, width);
            int rThickness = (int)math.clamp((width - centrePoint - chasmWidth / 2 - maxIrregularity + math.round((noise.cnoise(new float2(irregularitySeedR, (chunkIndex * height + (float)index / height) * irregularityFrequency)) / 2 + 0.5f) * maxIrregularity)), 1, width);
            lRThickness[index] = new int2(lThickness, rThickness);
        }
    }
}
