using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VerticalScrollerUI : MonoBehaviour
{
    public float trashScore, scoreBarRate, starPulsePeriod, starPulseMaxScale;
    public int[] starValues;
    public GameObject[] stars, healthObjects;

    private Player player;
    private LevelManager levelManager;
    private Text trashCounterText, depthCounterText, scoreText;
    private float trashCount, depthCount, score;
    private Slider fillBar;

    // Start is called before the first frame update
    void Start()
    {
        fillBar = transform.Find("GameOver/EndScreen/Score/FillBar").GetComponent<Slider>();
        GameObject gameOverPanel = transform.Find("GameOver").gameObject;
        scoreText = gameOverPanel.transform.Find("EndScreen/Score/Highscore_Num").GetComponent<Text>();
        gameOverPanel.SetActive(false);
        foreach (GameObject star in stars)
        {
            star.SetActive(false);
        }
        player = GameObject.Find("Player").GetComponent<Player>();
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
        //GetComponent<Collider2D>().enabled = false;
        trashCounterText = transform.Find("ReadoutPanel/Plastic").GetComponent<Text>();
        depthCounterText = transform.Find("ReadoutPanel/DepthPanel/Depth").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        trashCount = levelManager.plastic;
        trashCounterText.text = trashCount.ToString();
        depthCount = Mathf.Abs(Mathf.Clamp(Mathf.Ceil(player.transform.position.y), -Mathf.Infinity, 0));
        depthCounterText.text = depthCount.ToString();
        for (int i = 0; i < healthObjects.Length; i++)
        {
            healthObjects[i].SetActive(i < player.health);
        }
    }

    public void EndGame()
    {
        transform.Find("GameOver/EndScreen/Score/Plastic").GetComponent<Text>().text = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>().plastic.ToString();
        transform.Find("GameOver/EndScreen/Score/Depth").GetComponent<Text>().text = depthCount.ToString();
        score = trashScore * trashCount + depthCount;
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
        if (starsScored > GameManager.Instance.highscoreStars[0])
        {
            GameManager.Instance.highscoreStars[0] = starsScored;
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