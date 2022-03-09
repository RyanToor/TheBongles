using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThrowTheRobotUI : MonoBehaviour
{
    public Transform boostFill;
    public float scoreBarRate, starPulseMaxScale, starPulsePeriod, boostFadeRate, boostLerpSpeed;
    public int trashScore;
    public int[] starValues;
    public GameObject[] stars;

    private GameObject gameOver, pauseMenu, readouts;
    private Text metalReadout, pieReadout, scoreText;
    private Slider fillBar;
    private LevelManager_Robot levelManager;
    private Robot robotScript;
    private float score, boostDesiredXScale;
    private int trashCount;
    private Vector3 initialBoostFillScale;

    // Start is called before the first frame update
    void Start()
    {
        gameOver = transform.Find("GameOver").gameObject;
        pauseMenu = transform.Find("Pause").gameObject;
        readouts = transform.Find("ReadoutPanel").gameObject;
        scoreText = gameOver.transform.Find("EndScreen/Score/Highscore_Num").GetComponent<Text>();
        fillBar = transform.Find("GameOver/EndScreen/Score/FillBar").GetComponent<Slider>();
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager_Robot>();
        metalReadout = transform.Find("ReadoutPanel/Metal Panel/Metal").GetComponent<Text>();
        pieReadout = transform.Find("ReadoutPanel/Pie Panel/Pies").GetComponent<Text>();
        foreach (GameObject star in stars)
        {
            star.SetActive(false);
        }
        initialBoostFillScale = boostFill.localScale;
        robotScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Robot>();
    }

    // Update is called once per frame
    void Update()
    {
        trashCount = levelManager.metal;
        metalReadout.text = trashCount.ToString();
        pieReadout.text = levelManager.Pies.ToString();
        Color currentColour = boostFill.gameObject.GetComponent<Image>().color;
        boostFill.gameObject.GetComponent<Image>().color = new Color(currentColour.r, currentColour.g, currentColour.b, Mathf.Clamp(currentColour.a + ((levelManager.State == LevelState.launch || levelManager.State == LevelState.fly)? 1 : -1) * boostFadeRate * Time.deltaTime, 0, 1));
        currentColour = boostFill.parent.gameObject.GetComponent<Image>().color;
        boostFill.parent.gameObject.GetComponent<Image>().color = new Color(currentColour.r, currentColour.g, currentColour.b, Mathf.Clamp(currentColour.a + ((levelManager.State == LevelState.launch || levelManager.State == LevelState.fly)? 1 : -1) * boostFadeRate * Time.deltaTime, 0, 1));
        boostDesiredXScale = initialBoostFillScale.x * robotScript.boostFuel / robotScript.maxBoostFuel;
        boostFill.localScale = new Vector3(Mathf.Lerp(boostFill.localScale.x, boostDesiredXScale, boostLerpSpeed * Time.deltaTime), initialBoostFillScale.y, initialBoostFillScale.z);
    }

    public void EndGame()
    {
        gameOver.SetActive(true);
        pauseMenu.SetActive(false);
        readouts.SetActive(false);
        transform.Find("GameOver/EndScreen/Score/Metal").GetComponent<Text>().text = levelManager.metal.ToString();
        transform.Find("GameOver/EndScreen/Score/Distance").GetComponent<Text>().text = Mathf.Ceil(levelManager.totalThrowDistance).ToString();
        score = Mathf.Ceil(trashScore * trashCount + levelManager.totalThrowDistance);
        StartCoroutine(FillBar());
    }

    private IEnumerator FillBar()
    {
        float currentValue = 0;
        int starsScored = 0;
        while (currentValue < score)
        {
            scoreText.text = Mathf.Ceil(currentValue).ToString();
            fillBar.value = Mathf.Clamp((currentValue - starValues[starsScored]) / (starValues[Mathf.Clamp(starsScored + 1, 0, 3)] - starValues[starsScored]), 0, 1);
            currentValue += (scoreBarRate * (starValues[starsScored + 1] - starValues[starsScored]) / 1000f);
            if (starsScored < 3)
            {
                if (currentValue > starValues[starsScored + 1])
                {
                    stars[starsScored].SetActive(true);
                    StartCoroutine(StarPulse(stars[starsScored]));
                    starsScored++;
                }
            }
            yield return null;
        }
        if (starsScored > GameManager.Instance.highscoreStars[0])
        {
            GameManager.Instance.highscoreStars[1] = starsScored;
        }
        scoreText.text = score.ToString();
    }

    private IEnumerator StarPulse(GameObject star)
    {
        float currentTime = 0;
        while (currentTime < starPulsePeriod)
        {
            star.transform.localScale = Vector3.one * (1 + (starPulseMaxScale - 1) * Mathf.Sin(Mathf.PI * (currentTime / starPulsePeriod)));
            currentTime += Time.deltaTime;
            yield return null;
        }
        star.transform.localScale = Vector3.one;
    }
}
