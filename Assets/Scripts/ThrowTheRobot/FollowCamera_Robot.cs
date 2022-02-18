using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera_Robot : MonoBehaviour
{
    public bool stopRightMovement;
    public float cameraLerpSpeed, maxWaterOffset, deepWaterDepth;
    public Color[] waterPlateDeepColours;

    private GameObject robot;
    private int robotMinChunk = int.MaxValue;
    private Camera cameraComponent;
    private Transform backgroundPlateContainer, waterPlates, skyPlate;
    private Vector3 startOffset, desiredPosition, startPos, backgroundStartPos, waterStartOffset, initialSkyPlateScale, skyPlateOffset;
    private float backgroundParallaxFactor, initialOrthographicSize, chunkWidth;
    private Color[] waterPlateShallowColours;

    private void Awake()
    {
        chunkWidth = GameObject.Find("Level").GetComponent<LevelBuilder>().tilePixelWidth / 100;
        stopRightMovement = !Application.isEditor;
        startPos = transform.position;
        cameraComponent = GetComponent<Camera>();
        initialOrthographicSize = cameraComponent.orthographicSize;
        waterPlates = transform.Find("WaterPlates");
        skyPlate = transform.Find("SkyPlate");
        initialSkyPlateScale = skyPlate.localScale;
        skyPlateOffset = transform.position - skyPlate.position;
        Transform level = GameObject.Find("Level").transform;
        backgroundParallaxFactor = level.gameObject.GetComponent<LevelBuilder>().backgroundParallaxFactor;
        backgroundPlateContainer = level.Find("Backgrounds");
        backgroundStartPos = backgroundPlateContainer.position;
        waterPlateShallowColours = new Color[waterPlateDeepColours.Length];
        for (int i = 0; i < waterPlateShallowColours.Length; i++)
        {
            waterPlateShallowColours[i] = waterPlates.GetChild(i).GetComponent<SpriteRenderer>().color;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        robot = GameObject.FindGameObjectWithTag("Player");
        startOffset = transform.position - robot.transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        int robotCurrentChunk = robot.GetComponent<Robot>().isLanded? Mathf.CeilToInt(robot.transform.position.x / chunkWidth) : int.MaxValue;
        if (robotCurrentChunk < robotMinChunk)
        {
            robotMinChunk = robotCurrentChunk;
        }
        desiredPosition = robot.transform.position + startOffset;
        desiredPosition = new Vector3(Mathf.Clamp(desiredPosition.x, 0, stopRightMovement? robotMinChunk * chunkWidth + 0.64f : float.MaxValue), desiredPosition.y, desiredPosition.z);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, cameraLerpSpeed);
        float cameraZoom = Mathf.Clamp(transform.position.y + maxWaterOffset, initialOrthographicSize, int.MaxValue);
        cameraComponent.orthographicSize = cameraZoom;
        backgroundPlateContainer.position = backgroundStartPos + new Vector3((transform.position.x - startPos.x) * backgroundParallaxFactor, (Mathf.Clamp(transform.position.y, int.MinValue, startPos.y) - startPos.y) * backgroundParallaxFactor, 0);
        waterPlates.localScale = Vector3.one * (cameraZoom / initialOrthographicSize);
        skyPlate.localScale = (cameraZoom / initialOrthographicSize) * initialSkyPlateScale;
        skyPlate.position = Vector3.Scale(transform.position, new Vector3(1, 0, -1));
        waterPlates.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, float.MinValue, -5.4f * (cameraZoom / initialOrthographicSize))) + waterStartOffset;
        for (int i = 0; i < waterPlateDeepColours.Length; i++)
        {
            waterPlates.GetChild(i).GetComponent<SpriteRenderer>().color = Color.Lerp(waterPlateShallowColours[i], waterPlateDeepColours[i], Mathf.Clamp(-transform.position.y, 0, float.MaxValue) / deepWaterDepth);
        }
    }
}
