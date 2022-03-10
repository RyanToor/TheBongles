using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    public float tilePixelWidth, backgroundVerticalBuffer, birdInitialOffset, birdMinSeparation, birdMaxSeparation,
        cloudRadius, cloudMinHeight, cloudMaxHeight, cloudBorder;
    public Vector3 offset;
    public GameObject tilePrefab, bubblePrefab;
    public int[] biomeLengths;
    public GameObject birdPrefab, piePrefab, blockerPrefab, cloudPrefab, anchorPrefab;
    public Sprite[] blockerSprites;
    public BiomeTiles levelTileLibrary;
    public PuzzlePrefabArray[] biomePuzzlePrefabs;

    [HideInInspector] public bool isLevelBuilt = false;

    private List<int> floorDisplacements = new List<int>(), backgroundDisplacements = new List<int>();
    private int levelLength = 0;
    private int[] biomeEndPoints;
    LevelManager_Robot levelManager;

    // Start is called before the first frame update
    void Awake()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Robot>();
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
        PlaceLevel();
    }

    private void PlaceLevel()
    {
        int lastTileIndex = int.MaxValue;
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
            int randPuzzleIndex = 0;
            if (biomePuzzlePrefabs[currentBiome].puzzlePrefabs.Length > 1)
            {
                while (true)
                {
                    randPuzzleIndex = Random.Range(0, biomePuzzlePrefabs[currentBiome].puzzlePrefabs.Length);
                    if (randPuzzleIndex != lastTileIndex)
                    {
                        lastTileIndex = randPuzzleIndex;
                        break;
                    }
                }
            }
            int totalDisplacement = 0;
            foreach (int displacement in floorDisplacements)
            {
                totalDisplacement += displacement;
            }
            GameObject newTile = Instantiate(biomePuzzlePrefabs[currentBiome].puzzlePrefabs[randPuzzleIndex], new Vector3(i * tilePixelWidth / 100, totalDisplacement * -0.01f, 0) + offset, Quaternion.identity, gameObject.transform.Find("Puzzles"));
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
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager_Robot>();
        levelManager.floatingDecorations.objectsToAdd.AddRange(GameObject.FindGameObjectsWithTag("UpgradeButton"));
        if (levelManager.isTrashRandom)
        {
            foreach (GameObject trash in GameObject.FindGameObjectsWithTag("RandomTrash"))
            {
                levelManager.floatingTrash.objectsToAdd.Add(trash);
            }
        }
        for (int i = 0; i < Mathf.Min(5 - levelManager.pies, biomeEndPoints.Length); i++)
        {
            Instantiate(piePrefab, new Vector3(Random.Range(biomeEndPoints.Length - 2 - i < 0? 4 : biomeEndPoints[biomeEndPoints.Length - 2 - i] + 1, biomeEndPoints[biomeEndPoints.Length - 1 - i] - 1) * tilePixelWidth / 100, 0, 0), Quaternion.identity, transform);
        }
        PlaceBackground();
    }

    private void PlaceBackground()
    {
        for (int parallaxLevel = 0; parallaxLevel < levelTileLibrary.backgroundParralaxLevels.Length; parallaxLevel++)
        {
            int transitionsPlaced = 0;
            Tile lastBackgroundTile = default;
            for (int i = levelTileLibrary.backgroundParralaxLevels[parallaxLevel].startOffset.x; i < Mathf.Ceil(levelLength * levelTileLibrary.backgroundParralaxLevels[parallaxLevel].parallaxMultiplier) + 1; i++)
            {
                int totalDisplacement = 0, totalLevelDisplacement = 0, currentBiome = 0;
                foreach (int displacement in backgroundDisplacements)
                {
                    totalDisplacement += displacement;
                }
                for (int j = 0; j < i / levelTileLibrary.backgroundParralaxLevels[parallaxLevel].parallaxMultiplier; j++)
                {
                    if (j <= floorDisplacements.Count - 1)
                    {
                        totalLevelDisplacement += floorDisplacements[j];
                    }
                }
                for (int j = biomeLengths.Length - 1; j > 0; j--)
                {
                    int prevBiomeTiles = 0;
                    for (int biomeNumber = 0; biomeNumber < j; biomeNumber++)
                    {
                        prevBiomeTiles += biomeLengths[biomeNumber];
                    }
                    float parallaxI = i / levelTileLibrary.backgroundParralaxLevels[parallaxLevel].parallaxMultiplier;
                    if (parallaxI > prevBiomeTiles)
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
                if (transform.Find("Backgrounds/" + parallaxLevel.ToString()) == null)
                {
                    GameObject newContainer = new GameObject(parallaxLevel.ToString());
                    newContainer.transform.parent = transform.Find("Backgrounds");
                    newContainer.transform.localPosition = Vector3.zero;
                }
                GameObject newBackground = Instantiate(tilePrefab, offset.y * Vector3.up + new Vector3(i * tilePixelWidth / 100, levelTileLibrary.backgroundParralaxLevels[parallaxLevel].startOffset.y + (levelTileLibrary.backgroundParralaxLevels[parallaxLevel].isTileable? totalDisplacement : totalLevelDisplacement * levelTileLibrary.backgroundParralaxLevels[parallaxLevel].parallaxMultiplier) * -0.01f, 0), Quaternion.identity, transform.Find("Backgrounds/" + parallaxLevel.ToString()));
                List<Tile> currentBiomeTiles = new List<Tile>(levelTileLibrary.backgroundParralaxLevels[parallaxLevel].backgroundPlates[currentBackgroundIndex].plates);
                if (currentBiomeTiles.Count > 1 && !lastBackgroundTile.Equals(default(Tile)))
                {
                    currentBiomeTiles.Remove(lastBackgroundTile);
                }
                List<Tile> fittingBackgroundTiles = new List<Tile>();
                foreach (Tile tile in currentBiomeTiles)
                {
                    if (totalDisplacement + tile.yDifference - totalLevelDisplacement < backgroundVerticalBuffer)
                    {
                        fittingBackgroundTiles.Add(tile);
                    }
                }
                if (fittingBackgroundTiles.Count > 0)
                {
                    currentBiomeTiles = fittingBackgroundTiles;
                }
                Tile newBackgroundTile = currentBiomeTiles[Random.Range(0, Mathf.Clamp(currentBiomeTiles.Count - 1, 0, int.MaxValue))];
                newBackground.GetComponent<SpriteRenderer>().sprite = newBackgroundTile.sprite;
                newBackground.GetComponent<SpriteRenderer>().sortingLayerName = "Background";
                newBackground.GetComponent<SpriteRenderer>().sortingOrder = parallaxLevel;
                backgroundDisplacements.Add(newBackgroundTile.yDifference);
                lastBackgroundTile = newBackgroundTile;
            }
        }
        PlaceElements();
    }

    private void PlaceElements()
    {
        float birdCoverage = birdInitialOffset;
        while (birdCoverage < levelLength * tilePixelWidth / 100f)
        {
            birdCoverage += Random.Range(birdMinSeparation, birdMaxSeparation);
            Instantiate(birdPrefab, Vector3.right * birdCoverage, Quaternion.identity, transform.Find("Birds"));
        }
        for (int i = 0; i < blockerSprites.Length; i++)
        {
            Vector3 newBlockerPos = new Vector3(biomeEndPoints[i] * tilePixelWidth / 100, 0, 0);
            RaycastHit2D[] rayHits = Physics2D.RaycastAll(newBlockerPos, Vector3.down);
            Vector3 anchorPoint = rayHits[rayHits.Length - 1].point;
            GameObject newBlocker = Instantiate(blockerPrefab, newBlockerPos, Quaternion.identity, transform);
            newBlocker.GetComponent<SpriteRenderer>().sprite = blockerSprites[i];
            newBlocker.AddComponent<PolygonCollider2D>();
            GameObject newAnchor = Instantiate(anchorPrefab, anchorPoint, Quaternion.identity, transform);
            newAnchor.GetComponent<ProximityElement>().anchorPoint = anchorPoint;
            newAnchor.GetComponent<ProximityElement>().blockerPoint = newBlockerPos;
            PoissonDiscSampler sampler = new PoissonDiscSampler(biomeEndPoints[i] * tilePixelWidth / 100 - (i > 0 ? biomeEndPoints[i - 1] * tilePixelWidth / 100 : 0) - (2 * cloudBorder), cloudMaxHeight - cloudMinHeight, cloudRadius);
            foreach (Vector2 sample in sampler.Samples())
            {
                Vector2 samplePos = new Vector2(sample.x + (i > 0 ? biomeEndPoints[i - 1] * tilePixelWidth / 100 : 0) + cloudBorder, sample.y + cloudMinHeight);
                Instantiate(cloudPrefab, samplePos, Quaternion.identity, transform.Find("Clouds"));
            }
        }
        levelManager.floatingSurface.objectsToAdd.AddRange(GameObject.FindGameObjectsWithTag("Minigame"));
        isLevelBuilt = true;
    }

    [System.Serializable]
    public struct Tile
    {
        public Sprite sprite;
        public int yDifference;
    }

    [System.Serializable]
    public struct BackgroundPlateArray
    {
        public string name;
        public Tile[] plates;
    }

    [System.Serializable]
    public struct BackgroundParallaxLevel
    {
        public bool isTileable;
        public float parallaxMultiplier;
        public Vector3Int startOffset;
        public BackgroundPlateArray[] backgroundPlates;
    }

    [System.Serializable]
    public struct BiomeTiles
    {
        public Tile[] floorTiles;
        public BackgroundParallaxLevel[] backgroundParralaxLevels;
    }

    [System.Serializable]
    public struct PuzzlePrefabArray
    {
        public string name;
        public GameObject[] puzzlePrefabs;
    }
}
