using System.Collections;
using System.Collections.Generic;
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
    public int[] availableTutorialPages;
    public Animator sceneAnimator;
    public GameObject[] upgradeImages, dividers;
    public Button prevPage, nextPage;
    public int tutorialPages = 1;
    public GameObject tutorialButton;
    public UpgradeSpriteArray[] upgradeSprites;
    public ControlPromptSet[] upgradeControlPrompts;

    private int tutorialPage;
    private float lerpPos = 0, lerpDir = -1;
    private RectTransform rightPaneTransform;
    private AudioManager audioManager;

    private void Awake()
    {
        transform.Find("UpgradeBook").gameObject.SetActive(false);
    }

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
        if (SceneManager.GetActiveScene().name == "Map")
        {
            tutorialButton.SetActive(TutorialButtonEnabled());
        }
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
            transform.Find("UpgradeBook").gameObject.SetActive(false);
            LevelManager levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
            if (levelManager.isCursorVisible && SceneManager.GetActiveScene().name != "Map")
            {
                levelManager.StopMouseVisibleCoroutine();
                levelManager.isCursorVisible = false;
            }
        }
        else
        {
            GameManager.Instance.PauseGame(true);
        }
    }

    public void UpgradeBook(int pageTurn = 0)
    {
        GameManager.Instance.PauseGame(true);
        tutorialPage += pageTurn;
        sceneAnimator.gameObject.SetActive(tutorialPage < tutorialPages);
        sceneAnimator.SetInteger("Scene", SceneManager.GetActiveScene().buildIndex);
        sceneAnimator.SetInteger("Page", tutorialPage);
        prevPage.interactable = tutorialPage > 0;
        List<int> currentTutorialPages = new List<int>();
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
        nextPage.interactable = tutorialPage < currentTutorialPages.Count + tutorialPages - 1;
        for (int i = 0; i < upgradeControlPrompts.Length; i++)
        {
            foreach (ControlPrompt prompt in upgradeControlPrompts[i].controlPrompts)
            {
                prompt.promptObject.SetActive(false);
            }
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
                foreach (ControlPrompt prompt in upgradeControlPrompts[i].controlPrompts)
                {
                    if (prompt.control == upgradeSprites[currentTutorialPages[tutorialPage - tutorialPages]].upgradeSprites[i].input)
                    {
                        prompt.promptObject.SetActive(true);
                        if (prompt.control == InputType.PrimaryInput)
                        {
                            prompt.promptObject.transform.GetChild(0).GetComponent<Text>().text = "F";
                        }
                        else if (prompt.control == InputType.SecondaryInput)
                        {
                            prompt.promptObject.transform.GetChild(0).GetComponent<Text>().text = "B";
                        }
                    }
                }
                for (int j = 0; j < upgradeImages[i].transform.childCount - 2; j++)
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

    public void CloseUpgradeBook()
    {
        if (lerpDir < 0)
        {
            GameManager.Instance.PauseGame(false);
        }
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
        public GameObject promptObject;
    }

    [System.Serializable]
    public struct ControlPromptSet
    {
        public ControlPrompt[] controlPrompts;
    }
}