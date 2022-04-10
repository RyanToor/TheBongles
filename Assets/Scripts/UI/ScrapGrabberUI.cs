using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScrapGrabberUI : MinigameUI
{
    LevelManager_ScrapGrabber levelManager;
    protected override void Start()
    {
        starScoreIndex = 2;
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_ScrapGrabber>();
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        secondaryCount = (float)System.Math.Round(levelManager.remainingTime, 0);
        base.Update();
    }

    public override void EndGame()
    {
        Time.timeScale = 0;
        secondaryCount = Mathf.Ceil(trashScore * trashCount + Time.timeSinceLevelLoad);
        base.EndGame();
    }
}
