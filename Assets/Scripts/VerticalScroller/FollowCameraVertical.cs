using System.Collections.Generic;
using System;
using UnityEngine;

public class FollowCameraVertical : MonoBehaviour
{
    public GameObject target;
    [Range(0, 10)]
    public float lerpSpeed;
    public float minDistance, backgroundPlateSeparation, deepColourDepth;
    public int backgroundPlatesPerLevel;
    public List<BackgroundSprites> backgroundSprites;
    public List<Sprite> backgroundChasms;
    public List<ColourPair> depthColours;

    private Vector3 desiredPos;
    private Transform[] backgroundPlateContainers = new Transform[3];
    private List<GameObject[]> backgroundPlates = new List<GameObject[]>();
    private List<GameObject> chasmPlates = new List<GameObject>();
    private List<Sprite[]> loadedBackgrounds = new List<Sprite[]>();
    private List<Sprite> loadedChasms = new List<Sprite>();
    private float[] offset = new float[3], chasmOffset = new float[3];
    private int[] positionIndex = new int[3], chasmPositionIndex = new int[3];
    private SpriteRenderer[] colourPlates;

    private void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            colourPlates = new SpriteRenderer[3] { transform.Find("Background").GetComponent<SpriteRenderer>(), transform.Find("Midground").GetComponent<SpriteRenderer>(), transform.Find("Foreground").GetComponent<SpriteRenderer>() };
            Transform backgroundPlateTransform = transform.Find("Background_" + (i + 1).ToString());
            Transform chasmPlateTransform = transform.Find("ChasmPlates");
            backgroundPlateContainers[i] = backgroundPlateTransform;
            backgroundPlates.Add(new GameObject[2] { backgroundPlateTransform.Find("Left").gameObject, backgroundPlateTransform.Find("Right").gameObject });
            chasmPlates.Add(chasmPlateTransform.Find("ChasmPlate_" + (i +1).ToString()).gameObject);
            offset[i] = (-1080 / 32 - backgroundPlateSeparation - 0.75f) * i - 1080 / 128;
            chasmOffset[i] = (-1080 / 32 - backgroundPlateSeparation - 0.75f) * i - 1080 / 256;
            positionIndex[i] = i;
            chasmPositionIndex[i] = i;
            LoadBackgroundPlate(i, 0);
            LoadChasmPlate(i, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        desiredPos = new Vector3(0, Mathf.Clamp(target.transform.position.y, -Mathf.Infinity, 0),-10f);
        transform.position = Vector3.Lerp(transform.position, desiredPos, lerpSpeed);
        if ((desiredPos - transform.position).magnitude < minDistance)
        {
            transform.position = desiredPos;
        }
        for (int i = 0; i < backgroundPlateContainers.Length; i++)
        {
            backgroundPlateContainers[i].localPosition = -transform.position / 2 + offset[i] * Vector3.up;
            if (backgroundPlateContainers[i].localPosition.y > 1080 / 17 + backgroundPlateSeparation && transform.position.y < -1080 / 64)
            {
                LoadBackgroundPlate(i, -1);
            }
            else if (backgroundPlateContainers[i].localPosition.y < -1080 / 17 + backgroundPlateSeparation && transform.position.y < -1080 / 16)
            {
                LoadBackgroundPlate(i, 1);
            }
            if (transform.position.y > -1080 / 64)
            {
                backgroundPlateContainers[i].localPosition = Vector3.Lerp(1080 / 64 * Vector3.down, Vector3.zero, Mathf.Abs(transform.position.y) / (1080 / 64)) + (offset[i] + 1080 / 128) * Vector3.up;
            }
        }
        for (int i = 0; i < chasmPlates.Count; i++)
        {
            chasmPlates[i].transform.localPosition = -transform.position / 4 + chasmOffset[i] * Vector3.up;
            if (chasmPlates[i].transform.localPosition.y > 1080 / 17 + backgroundPlateSeparation && transform.position.y < -1080 / 64)
            {
                LoadChasmPlate(i, -1);
            }
            else if (chasmPlates[i].transform.localPosition.y < -1080 / 17 + backgroundPlateSeparation && transform.position.y < -1080 / 16)
            {
                LoadChasmPlate(i, 1);
            }
            if (transform.position.y > -1080 / 64)
            {
                chasmPlates[i].transform.localPosition = Vector3.Lerp(1080 / 64 * Vector3.down, Vector3.zero, Mathf.Abs(transform.position.y) / (1080 / 64)) + (chasmOffset[i] + 1080 / 256) * Vector3.up;
            }
        }
        for (int i = 0; i < colourPlates.Length; i++)
        {
            colourPlates[i].color = Color.Lerp(depthColours[i].shallowColor, depthColours[i].deepColour, -transform.position.y / deepColourDepth);
        }
    }

    private void LoadBackgroundPlate(int plateIndex, int direction)
    {
        offset[plateIndex] += (3240 / 32 + backgroundPlateSeparation + 0.25f) * direction;
        positionIndex[plateIndex] -= direction * 3;
        Sprite[] newSprites;
        if (direction <= 0 && positionIndex[plateIndex] >= loadedBackgrounds.Count)
        { 
            newSprites = GenerateBackgroundPlate(Mathf.Clamp(Mathf.FloorToInt(positionIndex[plateIndex] / backgroundPlatesPerLevel), 0, backgroundSprites.Count - 1));
        }
        else
        {
            newSprites = loadedBackgrounds[positionIndex[plateIndex]];
        }
        for (int i = 0; i < 2; i++)
        {
            backgroundPlates[plateIndex][i].GetComponent<SpriteRenderer>().sprite = newSprites[i];
        }
    }

    private Sprite[] GenerateBackgroundPlate(int newLoadedIndex)
    {
        int[] availableIndicies = new int[backgroundSprites[newLoadedIndex].sprites.Count];
        for (int i = 0; i < availableIndicies.Length; i++)
        {
            availableIndicies[i] = i;
        }
        Sprite[] newSprites = new Sprite[3];
        for (int i = 0; i < 3; i++)
        {
            if (i < 2)
            {
                int chosenIndex = UnityEngine.Random.Range(0, availableIndicies.Length);
                newSprites[i] = backgroundSprites[newLoadedIndex].sprites[availableIndicies[chosenIndex]].sprite;
                availableIndicies[chosenIndex] = availableIndicies[availableIndicies.Length - 1];
                Array.Resize(ref availableIndicies, availableIndicies.Length - 1);
            }
        }
        return newSprites;
    }

    private void LoadChasmPlate(int plateIndex, int direction)
    {
        chasmOffset[plateIndex] += (3240 / 32 + backgroundPlateSeparation + 0.25f) * direction;
        chasmPositionIndex[plateIndex] -= direction * 3;
        Sprite newSprite;
        if (direction <= 0 && positionIndex[plateIndex] >= loadedBackgrounds.Count)
        {
            newSprite = GenerateChasmPlate();
        }
        else
        {
            newSprite = loadedChasms[chasmPositionIndex[plateIndex]];
        }
        chasmPlates[plateIndex].GetComponent<SpriteRenderer>().sprite = newSprite;
    }

    private Sprite GenerateChasmPlate()
    {
        Sprite newSprite = backgroundChasms[UnityEngine.Random.Range(0, backgroundChasms.Count)];
        loadedChasms.Add(newSprite);
        return newSprite;
    }
}

[System.Serializable]
public struct BackgroundSprites
{
    public string name;
    public List<BackgroundSprite> sprites;
}

[System.Serializable]
public struct BackgroundSprite
{
    public Sprite sprite;
    public bool crossesFrame;
}

[System.Serializable]
public struct ColourPair
{
    public Color shallowColor;
    public Color deepColour;
}