using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    public int levelLength;
    public Vector3 offset;
    public GameObject tilePrefab;
    public BiomeTiles[] levelTiles;

    private List<int> floorDisplacements = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        PlaceFloor();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void PlaceFloor()
    {
        for (int i = 0; i < levelLength; i++)
        {
            int randTileIndex = Random.Range(0, levelTiles[0].floorTiles.Length);
            Tile selectedTile = levelTiles[0].floorTiles[randTileIndex];
            int totalDisplacement = 0;
            foreach (int displacement in floorDisplacements)
            {
                totalDisplacement += displacement;
            }
            GameObject newTile = Instantiate(tilePrefab, new Vector3(i * 10.24f, floorDisplacements.Count == 0? 0 : totalDisplacement * -0.01f, 0) + offset, Quaternion.identity, gameObject.transform);
            newTile.GetComponent<SpriteRenderer>().sprite = selectedTile.sprite;
            newTile.AddComponent<PolygonCollider2D>();
            floorDisplacements.Add(selectedTile.yDifference);
        }
    }

    [System.Serializable]
    public struct Tile
    {
        public Sprite sprite;
        public int yDifference;
    }

    [System.Serializable]
    public struct BiomeTiles
    {
        public Tile[] floorTiles;
    }
}
