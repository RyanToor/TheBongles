using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera_Robot : MonoBehaviour
{
    public float cameraLerpSpeed;

    private GameObject robot;
    private Transform backgroundPlateContainer;
    private Vector3 startOffset, desiredOffset, startPos, backgroundStartPos;
    private float backgroundParallaxFactor;

    private void Awake()
    {
        startPos = transform.position;
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
        backgroundPlateContainer.position = backgroundStartPos + new Vector3((transform.position.x - startPos.x) * backgroundParallaxFactor, 0, 0);
    }
}
