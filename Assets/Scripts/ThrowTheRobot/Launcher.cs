using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    public float reelSpeed, lineWidth;
    public Material lineMaterial;

    [HideInInspector]
    public bool isReeling;

    private LevelManager_Robot levelManager;
    private Vector3 reelBottom, reelTop;
    private GameObject robot;
    private bool reelStarted;

    private void Awake()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Robot>();
        robot = GameObject.FindGameObjectWithTag("Player").gameObject;
        reelBottom = transform.Find("Bubba/LineBottom").transform.position;
        reelTop = transform.Find("Bubba/LineStop").position;
        isReeling = true;
    }

    private void Update()
    {
        if (Application.isEditor)
        {
            EditorUpdate();
        }
    }

    public void Reel()
    {
        if (!reelStarted)
        {
            StartCoroutine(ReelIn());
            reelStarted = true;
        }
    }

    public IEnumerator ReelIn()
    {
        float progress = 0;
        transform.Find("Bubba").GetComponent<Animator>().SetTrigger("Reel");
        while (progress != 1)
        {
            if (isReeling)
            {
                progress += reelSpeed * Time.deltaTime;
                progress = Mathf.Clamp(progress, 0, 1);
                robot.transform.position = Vector3.Lerp(reelBottom, reelTop, progress);
                yield return new WaitForFixedUpdate();
            }
        }
        levelManager.State = LevelState.launch;
        transform.Find("Bubba").GetComponent<Animator>().SetTrigger("Eat");
        reelStarted = false;
    }

    private void EditorUpdate()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reel();
        }
    }
}
