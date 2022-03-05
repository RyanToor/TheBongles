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
    private GameObject hook;
    private bool reelStarted, reelUp = true;

    private void Awake()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Robot>();
        hook = transform.Find("Bubba/FishingPole/Hook").gameObject;
        reelBottom = hook.transform.position;
        reelTop = transform.Find("Bubba/FishingPole/LineStop").position;
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
        reelUp = !reelUp;
        if (!reelStarted)
        {
            StartCoroutine(ReelInOut());
            reelStarted = true;
        }
    }

    public IEnumerator ReelInOut()
    {
        transform.Find("Bubba/FishingPole").gameObject.SetActive(true);
        float progress = 0;
        transform.Find("Bubba").GetComponent<Animator>().SetTrigger("Reel");
        LineRenderer fishingLine;
        if (GetComponent<LineRenderer>() == null)
        {
            fishingLine = gameObject.AddComponent<LineRenderer>();
            fishingLine.startWidth = lineWidth;
            fishingLine.startColor = Color.black;
            fishingLine.endColor = Color.black;
            fishingLine.positionCount = 2;
            fishingLine.SetPosition(0, transform.Find("Bubba/FishingPole").transform.position);
            fishingLine.material = lineMaterial;
        }
        else
        {
            fishingLine = GetComponent<LineRenderer>();
        }
        while (progress != 1)
        {
            if (isReeling)
            {
                progress += reelSpeed * Time.deltaTime;
                progress = Mathf.Clamp(progress, 0, 1);
                hook.transform.position = Vector3.Lerp(reelUp? reelBottom : reelTop, reelUp? reelTop : reelBottom, progress);
                fishingLine.SetPosition(1, hook.transform.position);
                yield return new WaitForFixedUpdate();
            }
        }
        if (reelUp)
        {
            levelManager.State = LevelState.launch;
            Destroy(fishingLine);
            transform.Find("Bubba/FishingPole").gameObject.SetActive(false);
        }
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
