using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera_Robot : MonoBehaviour
{
    public float cameraLerpSpeed, maxWaterOffset, deepWaterDepth;
    public Color[] waterPlateDeepColours;

    private GameObject robot;
    private Camera cameraComponent;
    private Transform backgroundPlateContainer, waterPlates;
    private Vector3 startOffset, desiredOffset, startPos, backgroundStartPos, waterStartOffset;
    private float backgroundParallaxFactor, initialOrthographicSize;
    private Color[] waterPlateShallowColours;

    private void Awake()
    {
        startPos = transform.position;
        cameraComponent = GetComponent<Camera>();
        initialOrthographicSize = cameraComponent.orthographicSize;
        waterPlates = transform.Find("WaterPlates");
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
        desiredOffset = robot.transform.position + startOffset;
        if (desiredOffset.x < 0)
        {
            desiredOffset = Vector3.Scale(desiredOffset, new Vector3(0, 1, 1));
        }
        transform.position = Vector3.Lerp(transform.position, desiredOffset, cameraLerpSpeed);
        float cameraZoom = Mathf.Clamp(transform.position.y + maxWaterOffset, initialOrthographicSize, int.MaxValue);
        cameraComponent.orthographicSize = cameraZoom;
        backgroundPlateContainer.position = backgroundStartPos + new Vector3((transform.position.x - startPos.x) * backgroundParallaxFactor, (Mathf.Clamp(transform.position.y, int.MinValue, startPos.y) - startPos.y) * backgroundParallaxFactor, 0);
        waterPlates.localScale = Vector3.one * (cameraZoom / initialOrthographicSize);
        waterPlates.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, float.MinValue, -5.4f * (cameraZoom / initialOrthographicSize))) + waterStartOffset;
        for (int i = 0; i < waterPlateDeepColours.Length; i++)
        {
            waterPlates.GetChild(i).GetComponent<SpriteRenderer>().color = Color.Lerp(waterPlateShallowColours[i], waterPlateDeepColours[i], Mathf.Clamp(-transform.position.y, 0, float.MaxValue) / deepWaterDepth);
        }
    }
}
