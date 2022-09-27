using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Range(0, 1)]
    public float lerpSpeed;
    public Button openCloseButton, settingsButton;
    public Vector3 closedPos, openPos;
    public Slider musicSlider, sFXSlider;
    public MuteObjectsArray[] muteObjects;
    public int[] availableTutorialPages;
    public Animator sceneAnimator;
    public GameObject[] upgradeImages, dividers;
    public Button prevPage, nextPage, noButton;
    public int tutorialPages = 1, tutorialPage, startPromptPage;
    public GameObject defaultButton, tutorialButtonObject;
    public UpgradeSpriteArray[] upgradeSprites;
    public ControlPromptSet[] upgradeControlPrompts;

    [SerializeField] private TMPro.TextMeshProUGUI vibrationText, playstationText, promptsText;
    [SerializeField] private Image vibrationImage, playstationImage, promptsImage;
    [SerializeField] private Sprite vibrateOffPlaystation, vibrateOffGamepad, vibrateOnPlaystation, vibrateOnGamepad, controllerPlaystation, controllerGamepad, promptsOff, promptsOn;
    [SerializeField] private Toggle vibrationToggle, playstationToggle, promptsToggle;
    [SerializeField] private GameObject exitButton, pauseMenu, sureQuitMenu;
    [SerializeField] private float saveConfirmationPeriod;

    private float lerpPos = 0, lerpDir = -1;
    private RectTransform rightPaneTransform;
    private AudioManager audioManager;
    private List<int> currentTutorialPages;
    private Coroutine saveConfirmation;

    private void Awake()
    {
        transform.Find("UpgradeBook").gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        InputManager.Instance.Menu += OpenCloseMenu;
        audioManager = GameObject.Find("SoundManager").GetComponent<AudioManager>();
        rightPaneTransform = transform.Find("RightPanel").GetComponent<RectTransform>();
        rightPaneTransform.localPosition = closedPos;
        musicSlider.value = GameManager.Instance.musicVolume;
        sFXSlider.value = GameManager.Instance.sFXVolume;
        ToggleMute("Music");
        ToggleMute("SFX");
        if (SceneManager.GetActiveScene().name == "Map")
        {
            tutorialButtonObject.SetActive(TutorialButtonEnabled());
        }
        if (GameManager.Instance.isWeb && exitButton)
        {
            exitButton.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Save";
        }
        bool vibrateEnabled = GameManager.Instance.allowVibration;
        bool isPlaystation = GameManager.Instance.playstationLayout;
        bool promptsEnabled = GameManager.Instance.promptsEnabled;
        vibrationToggle.isOn = vibrateEnabled;
        playstationToggle.isOn = isPlaystation;
        promptsToggle.isOn = promptsEnabled;
        vibrationText.text = vibrateEnabled ? "Vibrations On" : "Vibrations Off";
        playstationText.text = isPlaystation ? "Playstation Icons" : "Gamepad Icons";
        promptsText.text = promptsEnabled ? "Prompts On" : "Prompts Off";
        vibrationImage.sprite = isPlaystation ? vibrateEnabled ? vibrateOnPlaystation : vibrateOffPlaystation : vibrateEnabled ? vibrateOnGamepad : vibrateOffGamepad;
        playstationImage.sprite = isPlaystation ? controllerPlaystation : controllerGamepad;
        promptsImage.sprite = promptsEnabled ? promptsOn : promptsOff;
        InputManager.Instance.SwitchControlScheme += UpdateTutorialPrompts;
    }

    public bool TutorialButtonEnabled()
    {
        for (int i = 0; i < GameManager.Instance.upgrades.Length; i++)
        {
            for (int j = 0; j < GameManager.Instance.upgrades[i].Length; j++)
            {
                if (GameManager.Instance.upgrades[i][j] > 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void StartPrompt()
    {
        tutorialPage = startPromptPage;
        tutorialButtonObject.GetComponent<Button>().onClick?.Invoke();
    }

    // Update is called once per frame
    private void Update()
    {
        if (lerpPos != Mathf.Clamp(lerpDir, 0, 1))
        {
            lerpPos = Mathf.Clamp(Mathf.Lerp(0, 1, lerpPos + lerpDir * lerpSpeed), 0, 1);
            rightPaneTransform.localPosition = Vector3.Lerp(closedPos, openPos, lerpPos);
        }
    }

    public void OpenCloseMenu()
    {
        audioManager.PlaySFX("Click");
        lerpDir *= -1;
        if (lerpDir == -1)
        {
            GameManager.Instance.PauseGame(false);
            transform.Find("RightPanel/Settings").gameObject.SetActive(false);
            transform.Find("RightPanel/PauseMenu").gameObject.SetActive(true);
            transform.Find("UpgradeBook").gameObject.SetActive(false);
            LevelManager levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
            if (Cursor.visible && SceneManager.GetActiveScene().name != "Map")
            {
                levelManager.StopMouseVisibleCoroutine();
                InputManager.Instance.EnableCursor(false);
            }
            InputManager.Instance.CloseMenu(transform);
        }
        else
        {
            InputManager.Instance.EnableUIInput();
            GameManager.Instance.PauseGame(true);
            InputManager.Instance.SetSelectedButton(defaultButton);
            InputManager.Instance.SetBackButton(openCloseButton);
        }
    }

    public void SetSelectedButton(GameObject button)
    {
        InputManager.Instance.SetSelectedButton(button);
    }

    public void SetBackButton(Button button)
    {
        InputManager.Instance.SetBackButton(button);
    }

    public void RemoveLatestBackButton()
    {
        InputManager.Instance.RemoveLatestBackButton();
    }

    public void UpgradeBook(int pageTurn = 0)
    {
        InputManager.Instance.EnableUIInput();
        GameManager.Instance.PauseGame(true);
        GameObject currentButton = EventSystem.current.currentSelectedGameObject;
        tutorialPage += pageTurn;
        sceneAnimator.gameObject.SetActive(tutorialPage < tutorialPages);
        sceneAnimator.SetInteger("Scene", SceneManager.GetActiveScene().buildIndex);
        sceneAnimator.SetInteger("Page", tutorialPage);
        UpdateTutorialPrompts();
        if (!(prevPage.interactable = tutorialPage > 0) && currentButton == prevPage.gameObject)
        {
            InputManager.Instance.SetSelectedButton(nextPage.gameObject);
        }
        if (!(nextPage.interactable = tutorialPage < currentTutorialPages.Count + tutorialPages - 1) && currentButton == nextPage.gameObject)
        {
            InputManager.Instance.SetSelectedButton(prevPage.gameObject);
        }
        for (int i = 0; i < upgradeImages.Length; i++)
        {
            upgradeImages[i].SetActive(tutorialPage >= tutorialPages);
            if (upgradeImages[i].activeSelf)
            {
                int upgradeTier = Mathf.Clamp(GameManager.Instance.upgrades[currentTutorialPages[tutorialPage - tutorialPages]][i], 0, upgradeSprites[currentTutorialPages[tutorialPage - tutorialPages]].upgradeSprites[i].sprites.Length);
                if (upgradeTier < 1)
                {
                    upgradeImages[i].SetActive(false);
                    continue;
                }
                upgradeImages[i].GetComponent<Image>().sprite = upgradeSprites[currentTutorialPages[tutorialPage - tutorialPages]].upgradeSprites[i].sprites[Mathf.Clamp(upgradeTier - 1, 0, int.MaxValue)];
                for (int j = 0; j < 3; j++)
                {
                    upgradeImages[i].transform.GetChild(j).gameObject.SetActive(j < upgradeTier);
                    if (upgradeImages[i].transform.GetChild(j).gameObject.activeSelf)
                    {
                        upgradeImages[i].transform.GetChild(j).GetComponent<Animator>().SetInteger("Minigame", currentTutorialPages[tutorialPage - tutorialPages] + 1);
                        upgradeImages[i].transform.GetChild(j).GetComponent<Animator>().SetInteger("Upgrade", i + 1);
                        upgradeImages[i].transform.GetChild(j).GetComponent<Animator>().SetInteger("Tier", j + 1);
                    }
                }
            }
        }
        foreach (GameObject divider in dividers)
        {
            divider.SetActive(tutorialPage >= tutorialPages);
        }
    }

    public void UpdateTutorialPrompts()
    {
        currentTutorialPages = new List<int>();
        for (int i = 0; i < availableTutorialPages.Length; i++)
        {
            if (currentTutorialPages.Contains(availableTutorialPages[i]) || GameManager.Instance.storyPoint < 3)
            {
                break;
            }
            for (int j = 0; j < GameManager.Instance.upgrades[availableTutorialPages[i]].Length; j++)
            {
                if (GameManager.Instance.upgrades[availableTutorialPages[i]][j] > 0)
                {
                    currentTutorialPages.Add(availableTutorialPages[i]);
                    break;
                }
            }
        }
        if (currentTutorialPages.Count > 0)
        {
            for (int i = 0; i < upgradeControlPrompts.Length; i++)
            {
                GameObject promptObject = null;
                InputType promptType = upgradeSprites[currentTutorialPages[Mathf.Clamp(tutorialPage - tutorialPages, 0, int.MaxValue)]].upgradeSprites[i].input;
                foreach (GameObject possiblePromptObject in upgradeControlPrompts[i].possiblePrompts)
                {
                    possiblePromptObject.SetActive(false);
                }
                foreach (ControlPrompt prompt in upgradeControlPrompts[i].controlPrompts)
                {
                    if (prompt.control == promptType)
                    {
                        promptObject = InputManager.Instance.inputMethod == "Keyboard&Mouse" ? prompt.keyboardPromptObject : prompt.gamepadPromptObject;
                        promptObject.SetActive(true);
                        break;
                    }
                }
                if (InputManager.Instance.inputMethod == "Gamepad" && promptType != InputType.Passive)
                {
                    promptObject.GetComponent<Image>().sprite = InputManager.Instance.abilityPrompts[(int)promptType - 1].abilityPrompts[GameManager.Instance.PlaystationLayout ? 2 : 1].sprite;
                }
                else if (InputManager.Instance.inputMethod == "Keyboard&Mouse" && (int)promptType > 1)
                {
                    promptObject.transform.GetChild(0).GetComponent<Text>().text = (int)promptType == 2 ? "F" : "B";
                }
            }
        }
    }

    public void CloseUpgradeBook()
    {
        if (lerpDir < 0)
        {
            GameManager.Instance.PauseGame(false);
        }
        InputManager.Instance.CloseMenu(transform.Find("UpgradeBook"));
    }

    public void ExitButton()
    {
        if (GameManager.Instance.isWeb)
        {
            GameManager.Instance.SaveGame();
            GameManager.Instance.SaveSettings();
            if (saveConfirmation == null)
            {
                saveConfirmation = StartCoroutine(ConfirmWebSave());
            }
        }
        else
        {
            sureQuitMenu.SetActive(true);
            pauseMenu.SetActive(false);
            SetBackButton(noButton);
            SetSelectedButton(noButton.gameObject);
        }
    }

    private IEnumerator ConfirmWebSave()
    {
        exitButton.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Game Saved";
        float duration = 0;
        while (duration < saveConfirmationPeriod)
        {
            duration += Time.unscaledDeltaTime;
            yield return null;
        }
        exitButton.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Save";
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
            if (toggleType == "Music")
            {
                muteObject.transform.Find("Cross").gameObject.SetActive(GameManager.Instance.musicMuted == 0);
            }
            else
            {
                muteObject.transform.Find("Cross").gameObject.SetActive(GameManager.Instance.sFXMuted == 0);
            }
        }
    }

    public void SetVibration(bool isEnabled)
    {
        GameManager.Instance.AllowVibration = isEnabled;
        vibrationText.text = isEnabled ? "Vibrations On" : "Vibrations Off";
        vibrationImage.sprite = GameManager.Instance.playstationLayout ? isEnabled ? vibrateOnPlaystation : vibrateOffPlaystation : isEnabled ? vibrateOnGamepad : vibrateOffGamepad;
        if (isEnabled)
        {
            InputManager.Instance.Vibrate(0.5f, 0.35f);
        }
    }

    public void SetPlaystationPrompts(bool isPlaystation)
    {
        GameManager.Instance.PlaystationLayout = isPlaystation;
        playstationImage.sprite = isPlaystation ? controllerPlaystation : controllerGamepad;
        bool vibrateEnabled = GameManager.Instance.allowVibration;
        vibrationImage.sprite = GameManager.Instance.playstationLayout ? vibrateEnabled ? vibrateOnPlaystation : vibrateOffPlaystation : vibrateEnabled ? vibrateOnGamepad : vibrateOffGamepad;
        playstationText.text = isPlaystation ? "Playstation Icons" : "Gamepad Icons";
    }

    public void SetPrompts(bool promptsEnabled)
    {
        GameManager.Instance.PromptsEnabled = promptsEnabled;
        promptsImage.sprite = promptsEnabled ? promptsOn : promptsOff;
        promptsText.text = promptsEnabled ? "Prompts On" : "Prompts Off";
    }

    private void OnDestroy()
    {
        InputManager.Instance.Menu -= OpenCloseMenu;
        InputManager.Instance.SwitchControlScheme -= UpdateTutorialPrompts;
    }

    [System.Serializable]
    public struct MuteObjectsArray
    {
        public string name;
        public GameObject[] buttons;
    }

    [System.Serializable]
    public struct UpgradeSpriteArray
    {
        public SpriteArray[] upgradeSprites;
    }

    [System.Serializable]
    public struct SpriteArray
    {
        public InputType input;
        public Sprite[] sprites;
    }

    [System.Serializable]
    public enum InputType
    {
        Passive,
        Jump,
        PrimaryInput,
        SecondaryInput
    }

    [System.Serializable]
    public struct ControlPrompt
    {
        public InputType control;
        public GameObject keyboardPromptObject;
        public GameObject gamepadPromptObject;
    }

    [System.Serializable]
    public struct ControlPromptSet
    {
        public GameObject[] possiblePrompts;
        public ControlPrompt[] controlPrompts;
    }
}