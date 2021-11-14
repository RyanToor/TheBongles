using System.Collections.Generic;
using System;
using UnityEngine;

public class FollowCameraVertical : MonoBehaviour
{
    public GameObject target;
    [Range(0, 10)]
    public float lerpSpeed;
    public float minDistance, backgroundPlateSeparation;
    public int backgroundPlatesPerLevel;
    public List<BackgroundSprites> backgroundSprites;

    private Vector3 desiredPos;
    private Transform[] backgroundPlateContainers = new Transform[3];
    private List<GameObject[]> backgroundPlates = new List<GameObject[]>();
    private List<Sprite[]> loadedBackgrounds = new List<Sprite[]>();
    private float[] offset = new float[3];
    private int[] positionIndex = new int[3];

    private void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            string backgroundPlateName = "Background_" + (i + 1).ToString();
            Transform backgroundPlateTransform = transform.Find(backgroundPlateName);
            backgroundPlateContainers[i] = backgroundPlateTransform;
            backgroundPlates.Add(new GameObject[2] { backgroundPlateTransform.Find("Left").gameObject, backgroundPlateTransform.Find("Right").gameObject });
            offset[i] = (-1080 / 32 - backgroundPlateSeparation) * i;
            positionIndex[i] = i;
            LoadBackgroundPlate(i, 0);
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
            backgroundPlateContainers[i].localPosition = - transform.position / 2 + offset[i] * Vector3.up;
            if (backgroundPlateContainers[i].localPosition.y > 1080 / 17 +backgroundPlateSeparation && transform.position.y < -1080 / 64)
            {
                LoadBackgroundPlate(i, -1);
            }
            else if (backgroundPlateContainers[i].localPosition.y < -1080 / 17 + backgroundPlateSeparation && transform.position.y < -1080 / 16)
            {
                LoadBackgroundPlate(i, 1);
            }
        }
    }

    private void LoadBackgroundPlate(int plateIndex, int direction)
    {
        Vector3 platePos = backgroundPlateContainers[plateIndex].localPosition;
        offset[plateIndex] += (3240 / 32 + backgroundPlateSeparation) * direction;
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
        Sprite[] newSprites = new Sprite[2];
        for (int i = 0; i < 2; i++)
        {
            int chosenIndex = UnityEngine.Random.Range(0, availableIndicies.Length);
            newSprites[i] = backgroundSprites[newLoadedIndex].sprites[availableIndicies[chosenIndex]].sprite;
            availableIndicies[chosenIndex] = availableIndicies[availableIndicies.Length - 1];
            Array.Resize(ref availableIndicies, availableIndicies.Length - 1);
        }
        loadedBackgrounds.Add(newSprites);
        return newSprites;
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
