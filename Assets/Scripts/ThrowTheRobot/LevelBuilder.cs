using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    public float backgroundParallaxFactor;
    public Vector3 offset;
    public GameObject tilePrefab;
    public int backgroundTileOffset, backgroundYOffset;
    public int[] biomeLengths;
    public BiomeTiles levelTileLibrary;
    public PuzzlePrefabArray[] biomePuzzlePrefabs;

    private List<int> floorDisplacements = new List<int>(), backgroundDisplacements = new List<int>();
    private int levelLength = 0;
    private int[] biomeEndPoints;

    // Start is called before the first frame update
    void Awake()
    {
        foreach (int biomeLength in biomeLengths)
        {
            levelLength += biomeLength;
        }
        biomeEndPoints = new int[biomeLengths.Length];
        for (int i = 0; i < biomeEndPoints.Length; i++)
        {
            biomeEndPoints[i] = 0;
            for (int j = 0; j < i + 1; j++)
            {
                biomeEndPoints[i] += biomeLengths[j];
            }
        }
        PlaceFloor();
        PlaceBackground();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void PlaceFloor()
    {
        for (int i = 0; i < levelLength; i++)
        {
            int currentBiome = biomeEndPoints.Length - 1;
            for (int j = 0; j < biomeEndPoints.Length; j++)
            {
                if (i < biomeEndPoints[j])
                {
                    currentBiome = j;
                    break;
                }
            }
            int randPuzzleIndex = Random.Range(0, biomePuzzlePrefabs[currentBiome].puzzlePrefabs.Length);
            int totalDisplacement = 0;
            foreach (int displacement in floorDisplacements)
            {
                totalDisplacement += displacement;
            }
            GameObject newTile = Instantiate(biomePuzzlePrefabs[currentBiome].puzzlePrefabs[randPuzzleIndex], new Vector3(i * 10.24f, totalDisplacement * -0.01f, 0) + offset, Quaternion.identity, gameObject.transform.Find("Puzzles"));
            Sprite newTileSprite = newTile.transform.Find("Ground").GetComponent<SpriteRenderer>().sprite;
            newTile.transform.Find("Ground").gameObject.AddComponent<PolygonCollider2D>();
            foreach (Tile tile in levelTileLibrary.floorTiles)
            {
                if (tile.sprite == newTileSprite)
                {
                    floorDisplacements.Add(tile.yDifference);
                    break;
                }
            }
        }
    }

    private void PlaceBackground()
    {
        int transitionsPlaced = 0;
        for (int i = -backgroundTileOffset; i < levelLength * backgroundParallaxFactor; i++)
        {
            int totalDisplacement = 0;
            foreach (int displacement in backgroundDisplacements)
            {
                totalDisplacement += displacement;
            }
            int currentBiome = biomeEndPoints.Length - 1;
            for (int j = 0; j < biomeEndPoints.Length; j++)
            {
                if (Mathf.Ceil((i + 2 * (transitionsPlaced + 1)) / backgroundParallaxFactor) < biomeEndPoints[j])
                {
                    currentBiome = j;
                    break;
                }
            }
            int currentBackgroundIndex = 2 * currentBiome;
            if (currentBiome > transitionsPlaced)
            {
                transitionsPlaced = currentBiome;
                currentBackgroundIndex--;
            }
            GameObject newBackground = Instantiate(tilePrefab, new Vector3(i * 10.24f, backgroundYOffset + totalDisplacement * -0.01f, 0) + offset, Quaternion.identity, transform.Find("Backgrounds"));
            Tile newBackgroundTile = levelTileLibrary.backgroundPlates[currentBackgroundIndex].plates[Random.Range(0, Mathf.Clamp(levelTileLibrary.backgroundPlates[currentBackgroundIndex].plates.Length - 1, 0, int.MaxValue))];
            newBackground.GetComponent<SpriteRenderer>().sprite = newBackgroundTile.sprite;
            newBackground.GetComponent<SpriteRenderer>().sortingLayerName = "Background";
            backgroundDisplacements.Add(newBackgroundTile.yDifference);
        }
    }

    [System.Serializable]
    public struct Tile
    {
        public Sprite sprite;
        public int yDifference;
    }

    [System.Serializable]
    public struct backgroundPlateArray
    {
        public string name;
        public Tile[] plates;
    }

    [System.Serializable]
    public struct BiomeTiles
    {
        public Tile[] floorTiles;
        public backgroundPlateArray[] backgroundPlates;
    }

    [System.Serializable]
    public struct PuzzlePrefabArray
    {
        public string name;
        public GameObject[] puzzlePrefabs;
    }
}
