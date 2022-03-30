using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigameUI : MonoBehaviour
{
    public float trashScore, secondaryCountMultiplier = 1, scoreBarRate, starPulsePeriod, starPulseMaxScale;
    public int[] starValues;
    public GameObject[] stars;

    protected LevelManager levelManagerBase;
    protected GameObject gameOver, pauseMenu, readouts;
    protected Text trashCounterText, secondaryCounterText, scoreText;
    protected float trashCount, secondaryCount, score;
    protected Slider fillBar;
    protected int starScoreIndex;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        levelManagerBase = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
        gameOver = transform.Find("GameOver").gameObject;
        pauseMenu = transform.Find("Pause").gameObject;
        readouts = transform.Find("ReadoutPanel").gameObject;
        fillBar = transform.Find("GameOver/EndScreen/Score/FillBar").GetComponent<Slider>();
        scoreText = gameOver.transform.Find("EndScreen/Score/Highscore_Num").GetComponent<Text>();
        trashCounterText = transform.Find("ReadoutPanel/TrashPanel/Trash").GetComponent<Text>();
        gameOver.SetActive(false);
        foreach (GameObject star in stars)
        {
            star.SetActive(false);
        }

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        trashCount = levelManagerBase.plastic + levelManagerBase.metal + levelManagerBase.glass;
        trashCounterText.text = trashCount.ToString();
        if (secondaryCounterText != null)
        {
            secondaryCounterText.text = secondaryCount.ToString();
        }
    }
    public virtual void EndGame()
    {
        Cursor.visible = true;
        AudioManager.instance.PlaySFX("GameOver");
        gameOver.SetActive(true);
        pauseMenu.SetActive(false);
        readouts.SetActive(false);
        transform.Find("GameOver/EndScreen/Score/Trash").GetComponent<Text>().text = trashCount.ToString();
        transform.Find("GameOver/EndScreen/Score/Secondary").GetComponent<Text>().text = secondaryCount.ToString();
        score = trashScore * trashCount + secondaryCount * secondaryCountMultiplier;
        StartCoroutine(FillBar());
    }

    protected virtual IEnumerator FillBar()
    {
        float currentValue = 0;
        int starsScored = 0;
        while (currentValue < score)
        {
            scoreText.text = Mathf.Ceil(currentValue).ToString();
            fillBar.value = Mathf.Clamp((currentValue - starValues[Mathf.Clamp(starsScored, 0, 2)]) / (starValues[Mathf.Clamp(starsScored + 1, 1, 3)] - starValues[starsScored]), 0, 1);
            currentValue += (scoreBarRate * (starValues[Mathf.Clamp(starsScored + 1, 1, 3)] - starValues[Mathf.Clamp(starsScored, 0, 2)]) / 1000f);
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
        if (starsScored > GameManager.Instance.highscoreStars[0])
        {
            GameManager.Instance.highscoreStars[starScoreIndex] = starsScored;
        }
        scoreText.text = score.ToString();
    }

    protected virtual IEnumerator StarPulse(GameObject star)
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

    public void LoadMap()
    {
        levelManagerBase.LoadMap();
    }
}
