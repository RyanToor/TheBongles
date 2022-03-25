using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThrowTheRobotUI : MinigameUI
{
    public Transform boostFill;
    public float boostFadeRate, boostLerpSpeed;

    private LevelManager_Robot levelManager;
    private Text pieReadout;
    private Robot robotScript;
    private float boostDesiredXScale;
    private Vector3 initialBoostFillScale;

    // Start is called before the first frame update
    protected override void Start()
    {
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager_Robot>();
        pieReadout = transform.Find("ReadoutPanel/Pie Panel/Pies").GetComponent<Text>();
        initialBoostFillScale = boostFill.localScale;
        robotScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Robot>();
        Color currentColour = boostFill.gameObject.GetComponent<Image>().color;
        boostFill.gameObject.GetComponent<Image>().color = new Color(currentColour.r, currentColour.g, currentColour.b, ((levelManager.State == LevelState.launch || levelManager.State == LevelState.fly) && robotScript.maxBoostFuel > 0) ? 1 : 0);
        currentColour = boostFill.parent.gameObject.GetComponent<Image>().color;
        boostFill.parent.gameObject.GetComponent<Image>().color = new Color(currentColour.r, currentColour.g, currentColour.b, ((levelManager.State == LevelState.launch || levelManager.State == LevelState.fly) && robotScript.maxBoostFuel > 0) ? 1 : 0);
        boostFill.localScale = new Vector3(robotScript.maxBoostFuel > 0 ? initialBoostFillScale.x * robotScript.boostFuel / robotScript.maxBoostFuel : 0, initialBoostFillScale.y, initialBoostFillScale.z);
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        pieReadout.text = levelManager.Pies.ToString();
        Color currentColour = boostFill.gameObject.GetComponent<Image>().color;
        boostFill.gameObject.GetComponent<Image>().color = new Color(currentColour.r, currentColour.g, currentColour.b, Mathf.Clamp(currentColour.a + (((levelManager.State == LevelState.launch || levelManager.State == LevelState.fly) && robotScript.maxBoostFuel > 0)? 1 : -1) * boostFadeRate * Time.deltaTime, 0, 1));
        currentColour = boostFill.parent.gameObject.GetComponent<Image>().color;
        boostFill.parent.gameObject.GetComponent<Image>().color = new Color(currentColour.r, currentColour.g, currentColour.b, Mathf.Clamp(currentColour.a + (((levelManager.State == LevelState.launch || levelManager.State == LevelState.fly) && robotScript.maxBoostFuel > 0)? 1 : -1) * boostFadeRate * Time.deltaTime, 0, 1));
        boostDesiredXScale = robotScript.maxBoostFuel > 0? initialBoostFillScale.x * robotScript.boostFuel / robotScript.maxBoostFuel : 0;
        boostFill.localScale = new Vector3(Mathf.Lerp(boostFill.localScale.x, boostDesiredXScale, boostLerpSpeed * Time.deltaTime), initialBoostFillScale.y, initialBoostFillScale.z);
        base.Update();
    }

    public override void EndGame()
    {
        secondaryCount = Mathf.CeilToInt(levelManager.totalThrowDistance);
        base.EndGame();
    }
}
