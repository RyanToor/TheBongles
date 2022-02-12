using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera_Robot : MonoBehaviour
{
    public float cameraLerpSpeed, maxWaterOffset;

    private GameObject robot;
    private Camera cameraComponent;
    private Transform backgroundPlateContainer;
    private Vector3 startOffset, desiredOffset, startPos, backgroundStartPos;
    private float backgroundParallaxFactor, initialOrthographicSize;

    private void Awake()
    {
        startPos = transform.position;
        cameraComponent = GetComponent<Camera>();
        initialOrthographicSize = cameraComponent.orthographicSize;
        Transform level = GameObject.Find("Level").transform;
        backgroundParallaxFactor = level.gameObject.GetComponent<LevelBuilder>().backgroundParallaxFactor;
        backgroundPlateContainer = level.Find("Backgrounds");
        backgroundStartPos = backgroundPlateContainer.position;
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
        transform.position = Vector3.Lerp(transform.position, desiredOffset, cameraLerpSpeed);
        cameraComponent.orthographicSize = Mathf.Clamp(transform.position.y + maxWaterOffset, initialOrthographicSize, int.MaxValue);
        backgroundPlateContainer.position = backgroundStartPos + new Vector3((transform.position.x - startPos.x) * backgroundParallaxFactor, (Mathf.Clamp(transform.position.y, int.MinValue, startPos.y) - startPos.y) * backgroundParallaxFactor, 0);
    }
}
