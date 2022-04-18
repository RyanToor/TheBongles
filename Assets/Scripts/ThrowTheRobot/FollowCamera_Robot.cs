using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera_Robot : MonoBehaviour
{
    public float cameraLerpSpeed, maxWaterOffset, deepWaterDepth, waterSurfaceScrollSpeed;
    public Color[] waterPlateDeepColours;
    public Transform[] waterSurfaces;

    private GameObject robot;
    private Camera cameraComponent;
    private Transform backgroundPlateContainer, waterPlates, skyPlate, backdrop;
    private Vector3 startOffset, desiredPosition, startPos, initialSkyPlateScale;
    private float initialOrthographicSize, cameraMaxX;
    private Color[] waterPlateShallowColours;
    private LevelBuilder levelBuilder;
    private float[] parallaxMultipliers;
    private LevelManager_Robot levelManager;
    private Robot robotScript;

    private void Awake()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Robot>();
        robot = GameObject.FindGameObjectWithTag("Player");
        robotScript = robot.GetComponent<Robot>();
        startPos = transform.position;
        cameraComponent = GetComponent<Camera>();
        initialOrthographicSize = cameraComponent.orthographicSize;
        waterPlates = transform.Find("WaterPlates");
        skyPlate = transform.Find("SkyPlate");
        backdrop = transform.Find("Backdrop");
        initialSkyPlateScale = skyPlate.localScale;
        Transform level = GameObject.Find("Level").transform;
        levelBuilder = level.gameObject.GetComponent<LevelBuilder>();
        parallaxMultipliers = new float[levelBuilder.levelTileLibrary.backgroundParralaxLevels.Length];
        for (int i = 0; i < parallaxMultipliers.Length; i++)
        {
            parallaxMultipliers[i] = levelBuilder.levelTileLibrary.backgroundParralaxLevels[Mathf.Clamp(i, 0, levelBuilder.levelTileLibrary.backgroundParralaxLevels.Length - 1)].parallaxMultiplier;
        }
        backgroundPlateContainer = level.Find("Backgrounds");
        waterPlateShallowColours = new Color[waterPlateDeepColours.Length];
        for (int i = 0; i < waterPlateShallowColours.Length; i++)
        {
            waterPlateShallowColours[i] = waterPlates.GetChild(i).GetComponent<SpriteRenderer>().color;
        }
        cameraMaxX = 0.64f;
        foreach (int biomeLength in levelBuilder.biomeLengths)
        {
            cameraMaxX += biomeLength * levelBuilder.tilePixelWidth / 100;
        }
        cameraMaxX += levelBuilder.tilePixelWidth / 100;
        PositionElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        startOffset = transform.position - robot.transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (levelManager.State != LevelState.launch)
        {
            PositionElements();
        }
    }

    private void PositionElements()
    {
        desiredPosition = robot.transform.position + startOffset;
        desiredPosition = new Vector3(Mathf.Clamp(desiredPosition.x, 0, levelManager.State == LevelState.move && robot.GetComponent<Robot>().stopRightMovement ? Mathf.Clamp(robotScript.rightMostPoint - 9.6f, 0, cameraMaxX) : cameraMaxX), desiredPosition.y, desiredPosition.z);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, cameraLerpSpeed);
        float cameraZoom = Mathf.Clamp(transform.position.y + maxWaterOffset, initialOrthographicSize, int.MaxValue);
        cameraComponent.orthographicSize = cameraZoom;
        for (int i = 0; i < levelBuilder.levelTileLibrary.backgroundParralaxLevels.Length; i++)
        {
            if (backgroundPlateContainer.transform.Find(i.ToString()) != null)
            {
                backgroundPlateContainer.transform.Find(i.ToString()).position = new Vector3((transform.position.x - startPos.x) * (1 - parallaxMultipliers[i]), (Mathf.Clamp(transform.position.y, int.MinValue, startPos.y) - startPos.y) * (1 - parallaxMultipliers[i]), 0);
            }
        }
        waterPlates.localScale = Vector3.one * (cameraZoom / initialOrthographicSize);
        skyPlate.localScale = (cameraZoom / initialOrthographicSize) * initialSkyPlateScale;
        skyPlate.position = Vector3.Scale(transform.position, new Vector3(1, 0, -1));
        backdrop.localScale = (cameraZoom / initialOrthographicSize) * initialSkyPlateScale;
        backdrop.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y + 5.4f, float.MinValue, 0), 10f);
        waterPlates.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, float.MinValue, -5.4f * (cameraZoom / initialOrthographicSize)));
        for (int i = 0; i < waterPlateDeepColours.Length; i++)
        {
            waterPlates.GetChild(i).GetComponent<SpriteRenderer>().color = Color.Lerp(waterPlateShallowColours[i], waterPlateDeepColours[i], Mathf.Clamp(-transform.position.y, 0, float.MaxValue) / deepWaterDepth);
        }
        foreach (Transform waterSurface in waterSurfaces)
        {
            waterSurface.position += new Vector3(waterSurfaceScrollSpeed * Time.deltaTime, 0, 0);
            if (Mathf.Abs(waterSurface.position.x - transform.position.x) > 60)
            {
                waterSurface.position = new Vector3(waterSurface.position.x - Mathf.Sign(waterSurface.position.x - transform.position.x) * 120, 0, waterSurface.position.z);
            }
        }
    }
}
