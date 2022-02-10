using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera_Robot : MonoBehaviour
{
    public float cameraLerpSpeed;

    private GameObject robot;
    private Vector3 startOffset, desiredOffset;
    private LevelManager_Robot levelManager;

    private void Awake()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Robot>();
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
        if (levelManager.isLaunched)
        {
            transform.position = Vector3.Lerp(transform.position, desiredOffset, cameraLerpSpeed);
        }
    }
}
