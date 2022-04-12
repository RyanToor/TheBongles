using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_ScrapGrabber : MonoBehaviour
{
    public Material glowMaterial;
    public float perlinSpeed, rayTranslateSpeed, rayFadeSpeed;
    public ParallaxLayerSet[] parallaxLayerSets;
    public GameObject[] rayLayers;

    [HideInInspector]
    public bool isInitialised;

    private ParallaxLayer[] parallaxLayerSet;
    private GameObject[] parallaxPlates;
    private Vector2[] perlinCoordinates;
    private Vector2[][] rayPerlinCoordinates;
    private float[] rayMaxAlphas;

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
            parallaxPlates[i].transform.localScale = Vector3.Lerp(Vector3.one * (1080f / 2048f), Vector3.one, parallaxLayerSet[i].parallaxMultiplier);
            SpriteRenderer newSpriteRenderer = parallaxPlates[i].AddComponent<SpriteRenderer>();
            newSpriteRenderer.sprite = parallaxLayerSet[i].sprite;
            newSpriteRenderer.sortingLayerName = "Background";
            newSpriteRenderer.sortingOrder = i == 0? -1 : i + 1;
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
        rayMaxAlphas = new float[rayLayers.Length];
        for (int i = 0; i < rayMaxAlphas.Length; i++)
        {
            rayMaxAlphas[i] = rayLayers[i].GetComponent<SpriteRenderer>().color.a;
        }
        rayPerlinCoordinates = new Vector2[rayLayers.Length][];
        for (int i = 0; i < rayLayers.Length; i++)
        {
            rayPerlinCoordinates[i] = new Vector2[2];
            for (int j = 0; j < 2; j++)
            {
                float perlinAngle = Random.Range(0, 2 * Mathf.PI);
                float perlinMagnitude = Random.Range(0, 10000);
                rayPerlinCoordinates[i][j] = new Vector2(perlinMagnitude * Mathf.Cos(perlinAngle), perlinMagnitude * Mathf.Sin(perlinAngle));
            }
        }
        isInitialised = true;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 2; i++)
        {
            perlinCoordinates[i] += perlinSpeed * Time.deltaTime * perlinCoordinates[i].normalized;
        }
        for (int i = 0; i < rayLayers.Length; i++)
        {
            rayPerlinCoordinates[i][0] += rayTranslateSpeed * Time.deltaTime * rayPerlinCoordinates[i][0].normalized;
            rayPerlinCoordinates[i][1] += rayFadeSpeed * Time.deltaTime * rayPerlinCoordinates[i][1].normalized;
        }
        for (int i = 0; i < parallaxPlates.Length; i++)
        {
            parallaxPlates[i].transform.position = new Vector3((Mathf.Clamp(Mathf.PerlinNoise(perlinCoordinates[0].x, perlinCoordinates[0].y), 0, 1) * 2f - 1f) * 10.88f * parallaxLayerSet[i].parallaxMultiplier, (Mathf.Clamp(Mathf.PerlinNoise(perlinCoordinates[1].x, perlinCoordinates[1].y), 0, 1) * 2f - 1f) * 4.84f * parallaxLayerSet[i].parallaxMultiplier, parallaxPlates[i].transform.position.z);
        }
        for (int i = 0; i < rayLayers.Length; i++)
        {
            rayLayers[i].transform.position = new Vector3((Mathf.Clamp(Mathf.PerlinNoise(rayPerlinCoordinates[i][0].x, rayPerlinCoordinates[i][0].y), 0, 1) * 2f - 1f) * 1.24f, rayLayers[i].transform.position.y, rayLayers[i].transform.position.z);
            rayLayers[i].GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, Mathf.Lerp(0, rayMaxAlphas[i], Mathf.Clamp(Mathf.PerlinNoise(rayPerlinCoordinates[i][0].x, rayPerlinCoordinates[i][0].y), 0, 1)));
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
