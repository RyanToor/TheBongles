using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject loadScreen;

    public static bool GameIsPaused = false;
    [Range(0, 1)]
    public float lerpSpeed;
    public Vector3 closedPos, openPos;
    public Slider musicSlider, sFXSlider;
    public MuteObjectsArray[] muteObjects;

    private float lerpPos = 0, lerpDir = -1;
    private RectTransform rightPaneTransform;
    private AudioManager audioManager;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = GameObject.Find("SoundManager").GetComponent<AudioManager>();
        rightPaneTransform = transform.Find("RightPanel").GetComponent<RectTransform>();
        rightPaneTransform.localPosition = closedPos;
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.25f);
        sFXSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        ToggleMute("Music");
        ToggleMute("SFX");
    }

    // Update is called once per frame
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OpenCloseMenu();
            audioManager.PlaySFX("Click");
        }

        if (lerpPos != Mathf.Clamp(lerpDir, 0, 1))
        {
            lerpPos = Mathf.Clamp(Mathf.Lerp(0, 1, lerpPos + lerpDir * lerpSpeed), 0, 1);
            rightPaneTransform.localPosition = Vector3.Lerp(closedPos, openPos, lerpPos);
        }
    }
    /*public void Resume()
    {
        Time.timeScale = 1f;
        //GameIsPaused = false;
    }

    void Pause()
    {
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
    }*/

    public void OpenCloseMenu()
    {
        lerpDir *= -1;
        if (lerpDir == -1)
        {
            Time.timeScale = 1f;
        }
        else
        {
            Time.timeScale = 0f;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        PlayerPrefs.SetInt("isLoaded", 1);
    }

    public void BackToMap()
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
        GameObject.Find("BongleIsland").GetComponent<BongleIsland>().isReseting = true;
        SceneManager.LoadScene("Map");
    }

    private void ToggleMute(string toggleType)
    {
        GameObject[] currentMuteObjects = null;
        foreach (MuteObjectsArray muteObjectArray in muteObjects)
        {
            if (muteObjectArray.name == toggleType)
            {
                currentMuteObjects = muteObjectArray.buttons;
                break;
            }
        }
        foreach (GameObject muteObject in currentMuteObjects)
        {
            foreach (Transform buttonType in muteObject.transform)
            {
                buttonType.gameObject.SetActive((PlayerPrefs.GetInt(toggleType + "Muted", 1) == 0 ^ buttonType.gameObject.name == "Sound_Button") ? true : false);
            }
        }
    }

    [System.Serializable]
    public struct MuteObjectsArray
    {
        public string name;
        public GameObject[] buttons;
    }
}