using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScrapGrabberUI : MinigameUI
{
    public float dangerTime, flashPeriod;
    public Color dangerColour;

    private Color normalColour;
    private bool isFlashing;
    private Text timeRemainingText;

    LevelManager_ScrapGrabber levelManager;
    protected override void Start()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_ScrapGrabber>();
        timeRemainingText = transform.Find("ReadoutPanel/TrashPanel/Time").GetComponent<Text>();
        normalColour = timeRemainingText.color;
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        secondaryCount = (float)System.Math.Round(levelManager.remainingTime, 0);
        if (0 < secondaryCount && secondaryCount < dangerTime && !isFlashing)
        {
            StartCoroutine(Flash());
            isFlashing = true;
        }
        base.Update();
    }

    private IEnumerator Flash()
    {
        float duration = 0;
        while (levelManager.remainingTime < dangerTime && levelManager.remainingTime > 0)
        {
            timeRemainingText.color = duration % flashPeriod < flashPeriod / 2 ? dangerColour : normalColour;
            duration += Time.deltaTime;
            yield return null;
        }
        timeRemainingText.color = normalColour;
        isFlashing = false;
    }

    private void FixedUpdate()
    {
        timeRemainingText.text = System.Math.Round(levelManager.remainingTime, 0).ToString() + " s";
    }

    public override void EndGame()
    {
        Time.timeScale = 0;
        secondaryCount = Mathf.Ceil(trashScore * trashCount + Time.timeSinceLevelLoad);
        base.EndGame();
    }
}
