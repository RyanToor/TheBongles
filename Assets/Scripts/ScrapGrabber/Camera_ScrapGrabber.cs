using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_ScrapGrabber : MonoBehaviour
{
    public Material glowMaterial;
    public float perlinSpeed;
    public ParallaxLayerSet[] parallaxLayerSets;

    private ParallaxLayer[] parallaxLayerSet;
    private GameObject[] parallaxPlates;
    private Vector2[] perlinCoordinates;

    // Start is called before the first frame update
    void Start()
    {
        parallaxLayerSet = parallaxLayerSets[Random.Range(0, parallaxLayerSets.Length)].parallaxLayers;
        parallaxPlates = new GameObject[parallaxLayerSet.Length];
        for (int i = 0; i < parallaxLayerSet.Length; i++)
        {
            parallaxPlates[i] = new GameObject("Plate" + i.ToString());
            parallaxPlates[i].transform.parent = transform;
            parallaxPlates[i].transform.position = Vector3.Scale(Camera.main.transform.position, -Vector3.one);
            SpriteRenderer newSpriteRenderer = parallaxPlates[i].AddComponent<SpriteRenderer>();
            newSpriteRenderer.sprite = parallaxLayerSet[i].sprite;
            newSpriteRenderer.sortingLayerName = "Background";
            newSpriteRenderer.sortingOrder = i == 0? -1 : i;
            if (parallaxLayerSet[i].glow)
            {
                newSpriteRenderer.material = glowMaterial;
            }
        }
        perlinCoordinates = new Vector2[2];
        for (int i = 0; i < 2; i++)
        {
            float perlinAngle = Random.Range(0, 2 * Mathf.PI);
            float perlinMagnitude = Random.Range(0, 10000);
            perlinCoordinates[i] = new Vector2(perlinMagnitude * Mathf.Cos(perlinAngle), perlinMagnitude * Mathf.Sin(perlinAngle));
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 2; i++)
        {
            perlinCoordinates[i] += perlinCoordinates[i].normalized * Time.deltaTime * perlinSpeed;
        }
        for (int i = 0; i < parallaxPlates.Length; i++)
        {
            parallaxPlates[i].transform.position = new Vector3((Mathf.Clamp(Mathf.PerlinNoise(perlinCoordinates[0].x, perlinCoordinates[0].y), 0, 1) * 2 - 1) * 10.88f * parallaxLayerSet[i].parallaxMultiplier, (Mathf.Clamp(Mathf.PerlinNoise(perlinCoordinates[1].x, perlinCoordinates[1].y), 0, 1) * 2 - 1) * 4.84f * parallaxLayerSet[i].parallaxMultiplier, parallaxPlates[i].transform.position.z);
        }
    }

    [System.Serializable]
    public struct ParallaxLayer
    {
        public bool glow;
        [Range(0, 1)]
        public float parallaxMultiplier;
        public Sprite sprite;
    }

    [System.Serializable]
    public struct ParallaxLayerSet
    {
        public ParallaxLayer[] parallaxLayers;
    }
}
