using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VerticalScrollerUI : MonoBehaviour
{
    public float trashScore, scoreBarRate, starPulsePeriod, starPulseMaxScale;
    public int[] starValues;
    public GameObject[] stars, healthObjects;

    private Player player;
    private Text plasticCounterText, depthCounterText, scoreText;
    private float plasticCount, depthCount, score;
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
        plasticCounterText = transform.Find("ReadoutPanel/Plastic").GetComponent<Text>();
        depthCounterText = transform.Find("ReadoutPanel/DepthPanel/Depth").GetComponent<Text>();
        print(plasticCounterText.text);
    }

    // Update is called once per frame
    void Update()
    {
        plasticCount = player.collectedPlastic;
        plasticCounterText.text = plasticCount.ToString();
        depthCount = Mathf.Abs(Mathf.Clamp(Mathf.Ceil(player.transform.position.y), -Mathf.Infinity, 0));
        depthCounterText.text = depthCount.ToString();
        for (int i = 0; i < healthObjects.Length; i++)
        {
            healthObjects[i].SetActive(i < player.health);
        }
    }

    public void EndGame()
    {
        score = trashScore * plasticCount + depthCount;
        StartCoroutine(FillBar());
    }

    private IEnumerator FillBar()
    {
        float currentValue = 0;
        int starsScored = 0;
        while (currentValue < score)
        {
            scoreText.text = currentValue.ToString();
            fillBar.value = Mathf.Clamp((currentValue - starValues[starsScored]) / (starValues[Mathf.Clamp(starsScored + 1, 0, 3)] - starValues[starsScored]), 0, 1);
            currentValue += scoreBarRate;
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