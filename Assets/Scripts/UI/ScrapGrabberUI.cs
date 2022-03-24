using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScrapGrabberUI : MonoBehaviour
{
    public Text glassReadout, timeReadout;
    public float dangerTime, flashPeriod, trashScore, scoreBarRate, starPulseMaxScale, starPulsePeriod;
    public Color dangerColour;
    public int[] starValues;
    public GameObject[] stars;

    private Color normalColour;
    private bool isFlashing;
    private float score, trashCount;
    private GameObject gameOver, pauseMenu, readouts;
    private Text scoreText;
    private Slider fillBar;

    LevelManager_ScrapGrabber levelManager;
    private void Awake()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_ScrapGrabber>();
        normalColour = timeReadout.color;
        gameOver = transform.Find("GameOver").gameObject;
        pauseMenu = transform.Find("Pause").gameObject;
        readouts = transform.Find("ReadoutPanel").gameObject;
        scoreText = gameOver.transform.Find("EndScreen/Score/Highscore_Num").GetComponent<Text>();
        fillBar = transform.Find("GameOver/EndScreen/Score/FillBar").GetComponent<Slider>();
        foreach (GameObject star in stars)
        {
            star.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        trashCount = levelManager.glass;
        glassReadout.text = trashCount.ToString();
        if (0 < levelManager.remainingTime && levelManager.remainingTime < dangerTime && !isFlashing)
        {
            StartCoroutine(Flash());
            isFlashing = true;
        }
    }

    private IEnumerator Flash()
    {
        float duration = 0;
        while (levelManager.remainingTime < dangerTime && levelManager.remainingTime > 0)
        {
            timeReadout.color = duration % flashPeriod < flashPeriod / 2 ? dangerColour : normalColour;
            duration += Time.deltaTime;
            yield return null;
        }
        timeReadout.color = normalColour;
        isFlashing = false;
    }

    private void FixedUpdate()
    {
        timeReadout.text = System.Math.Round(levelManager.remainingTime, 0).ToString() + " s";
    }

    public void EndGame()
    {
        gameOver.SetActive(true);
        pauseMenu.SetActive(false);
        readouts.SetActive(false);
        transform.Find("GameOver/EndScreen/Score/Glass").GetComponent<Text>().text = levelManager.glass.ToString();
        score = Mathf.Ceil(trashScore * trashCount);
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
                if (currentValue >= starValues[starsScored + 1])
                {
                    stars[starsScored].SetActive(true);
                    StartCoroutine(StarPulse(stars[starsScored]));
                    starsScored++;
                }
            }
            yield return null;
        }
        if (starsScored > GameManager.Instance.highscoreStars[1])
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
