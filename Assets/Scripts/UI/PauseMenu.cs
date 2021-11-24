using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject loadScreen;

    public static bool GameIsPaused = false;
    [Range(0, 1)]
    public float lerpSpeed;
    public Vector3 closedPos, openPos;

    public Animator rightPanel;

    public float lerpPos = 0, lerpDir = -1;
    private RectTransform rightPaneTransform;
    private AudioManager audioManager;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = GameObject.Find("SoundManager").GetComponent<AudioManager>();
        rightPaneTransform = transform.Find("RightPanel").GetComponent<RectTransform>();
        rightPaneTransform.localPosition = closedPos;
    }

    // Update is called once per frame
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
            audioManager.PlaySFX("Click");
        }

        if (lerpPos != Mathf.Clamp(lerpDir, 0, 1))
        {
            lerpPos = Mathf.Clamp(Mathf.Lerp(0, 1, lerpPos + lerpDir * lerpSpeed), 0, 1);
            rightPaneTransform.localPosition = Vector3.Lerp(closedPos, openPos, lerpPos);
        }
    }
    public void Resume()
    {
        OpenCloseMenu();
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    void Pause()
    {
        OpenCloseMenu();
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void pauseGame()
    {
        if (GameIsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void OpenCloseMenu()
    {
        lerpDir *= -1;
    }

    public void QuitGame()
    {
        Application.Quit();
        PlayerPrefs.SetInt("isLoaded", 1);
    }

    public void backToMap()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        GameObject newLoadScreen = Instantiate(loadScreen, new Vector3(960, 540, 0), Quaternion.identity);
        DontDestroyOnLoad(newLoadScreen);
        SceneManager.LoadScene("Map");
    }
    public void ResetGame()
    {
        PlayerPrefs.DeleteAll();
    }
}