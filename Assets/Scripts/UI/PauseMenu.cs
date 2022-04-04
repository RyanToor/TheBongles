using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
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
        musicSlider.value = GameManager.Instance.musicVolume;
        sFXSlider.value = GameManager.Instance.sFXVolume;
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

    public void OpenCloseMenu()
    {
        lerpDir *= -1;
        if (lerpDir == -1)
        {
            GameManager.Instance.PauseGame(false);
            transform.Find("RightPanel/Settings").gameObject.SetActive(false);
            transform.Find("RightPanel/PauseMenu").gameObject.SetActive(true);
            transform.Find("RightPanel/PauseMenu/UpgradeBook").gameObject.SetActive(false);
        }
        else
        {
            GameManager.Instance.PauseGame(true);
        }
    }

    public void UpgradeBook()
    {

    }

    public void QuitGame()
    {
        Application.Quit();
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
                if (toggleType == "Music")
                {
                    buttonType.gameObject.SetActive(((GameManager.Instance.musicMuted == 0) ^ (buttonType.gameObject.name == "Sound_Button")));
                }
                else
                {
                    buttonType.gameObject.SetActive(((GameManager.Instance.sFXMuted == 0) ^ (buttonType.gameObject.name == "Sound_Button")));
                }
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