using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinigameUI : MonoBehaviour
{
    public float trashScore, secondaryCountMultiplier = 1, scoreBarRate, starPulsePeriod, starPulseMaxScale;
    public int[] starValues;
    public GameObject[] stars;
    public GameObject jumpAbilityFrame, primaryAbilityFrame;

    protected LevelManager levelManagerBase;
    protected GameObject gameOver, pauseMenu, readouts;
    protected TMPro.TextMeshProUGUI trashCounterText, secondaryCounterText, scoreText;
    protected float trashCount, secondaryCount, score;
    protected Slider fillBar;
    protected int starScoreIndex;
    protected int controlSchemeIndex;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        levelManagerBase = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
        gameOver = transform.Find("GameOver").gameObject;
        pauseMenu = transform.Find("Pause").gameObject;
        readouts = transform.Find("ReadoutPanel").gameObject;
        fillBar = transform.Find("GameOver/EndScreen/Score/FillBar").GetComponent<Slider>();
        scoreText = gameOver.transform.Find("EndScreen/Score/Highscore_Num").GetComponent<TMPro.TextMeshProUGUI>();
        trashCounterText = transform.Find("ReadoutPanel/TrashPanel/Trash").GetComponent<TMPro.TextMeshProUGUI>();
        gameOver.SetActive(false);
        foreach (GameObject star in stars)
        {
            star.SetActive(false);
        }
        starScoreIndex = SceneManager.GetActiveScene().buildIndex - 1;
        UpdateAbilityFrames();
        InputManager.Instance.SwitchControlScheme += UpdateAbilityFrames;
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

    protected virtual void UpdateAbilityFrames()
    {
        controlSchemeIndex = InputManager.Instance.inputMethod == "Keyboard&Mouse" ? 0 : GameManager.Instance.playstationLayout ? 2 : 1;
        GameObject[] frames = new GameObject[2] { jumpAbilityFrame, primaryAbilityFrame };
        for (int i = 0; i < 2; i++)
        {
            if (frames[i] != null)
            {
                frames[i].GetComponent<Animator>().SetBool("Space", controlSchemeIndex == 0 && i == 0);
                frames[i].transform.GetChild(0).gameObject.SetActive(controlSchemeIndex == 0);
                frames[i].transform.GetChild(1).gameObject.SetActive(controlSchemeIndex != 0);
                if (controlSchemeIndex == 0)
                {
                    TMPro.TextMeshProUGUI text = frames[i].transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
                    text.color = InputManager.Instance.abilityPrompts[i].abilityPrompts[controlSchemeIndex].colour;
                    text.text = InputManager.Instance.abilityPrompts[i].abilityPrompts[controlSchemeIndex].text;
                }
                else
                {
                    frames[i].transform.GetChild(1).GetComponent<Image>().sprite = InputManager.Instance.abilityPrompts[i].abilityPrompts[controlSchemeIndex].sprite;
                }
            }
        }
    }

    public virtual void EndGame()
    {
        InputManager.Instance.EnableCursor();
        gameOver.SetActive(true);
        if (InputManager.Instance.inputMethod == "Gamepad")
        {
            InputManager.Instance.SetSelectedButton(gameOver.transform.Find("EndScreen/Map_Button").gameObject);
        }
        InputManager.Instance.EnableUIInput();
        pauseMenu.SetActive(false);
        readouts.SetActive(false);
        transform.Find("GameOver/EndScreen/Score/Trash").GetComponent<TMPro.TextMeshProUGUI>().text = trashCount.ToString();
        transform.Find("GameOver/EndScreen/Score/Secondary").GetComponent<TMPro.TextMeshProUGUI>().text = secondaryCount.ToString();
        score = trashScore * trashCount + secondaryCount * secondaryCountMultiplier;
        StartCoroutine(FillBar());
        PromptManager promptManager = transform.Find("Prompts").GetComponent<PromptManager>();
        promptManager.disablePersistents = true;
        promptManager.CancelPrompt();
    }

    protected virtual IEnumerator FillBar()
    {
        float currentValue = 0;
        int starsScored = 0;
        while (currentValue < score)
        {
            scoreText.text = Mathf.Ceil(currentValue).ToString();
            fillBar.value = Mathf.Clamp((currentValue - starValues[Mathf.Clamp(starsScored, 0, 2)]) / (starValues[Mathf.Clamp(starsScored + 1, 1, 3)] - starValues[starsScored]), 0, 1);
            currentValue += (scoreBarRate * (starValues[Mathf.Clamp(starsScored + 1, 1, 3)] - starValues[Mathf.Clamp(starsScored, 0, 2)]) / 1000f) * Time.unscaledDeltaTime * 100;
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
        if (starsScored > GameManager.Instance.highscoreStars[starScoreIndex])
        {
            GameManager.Instance.highscoreStars[starScoreIndex] = starsScored;
        }
        scoreText.text = score.ToString();
        EventSystem.current.SetSelectedGameObject(gameOver.transform.Find("EndScreen/Map_Button").gameObject);
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

    private void OnDisable()
    {
        InputManager.Instance.SwitchControlScheme -= UpdateAbilityFrames;
    }
}
