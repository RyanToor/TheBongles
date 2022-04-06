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
    [SerializeField]
    public UpgradeSpriteArray[] upgradeSprites;
    public ControlPromptSet[] upgradeControlPrompts;

    private int tutorialPage;
    private float lerpPos = 0, lerpDir = -1;
    private RectTransform rightPaneTransform;
    private AudioManager audioManager;

    // Start is called before the first frame update
    void Start()
    {
        transform.Find("UpgradeBook").gameObject.SetActive(false);
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
            transform.Find("UpgradeBook").gameObject.SetActive(false);
        }
        else
        {
            GameManager.Instance.PauseGame(true);
        }
    }

    public void UpgradeBook(int pageTurn)
    {
        tutorialPage += pageTurn;
        sceneAnimator.gameObject.SetActive(tutorialPage == 0);
        sceneAnimator.SetInteger("Scene", SceneManager.GetActiveScene().buildIndex);
        prevPage.interactable = tutorialPage > 0;
        nextPage.interactable = tutorialPage < availableTutorialPages.Length;
        for (int i = 0; i < upgradeImages.Length; i++)
        {
            upgradeImages[i].SetActive(tutorialPage != 0);
            if (upgradeImages[i].activeSelf)
            {
                int upgradeTier = Mathf.Clamp(GameManager.Instance.upgrades[availableTutorialPages[tutorialPage - 1]][i], 0, upgradeSprites[availableTutorialPages[tutorialPage - 1]].upgradeSprites[i].sprites.Length);
                upgradeImages[i].GetComponent<Image>().sprite = upgradeTier < 1 ? null : upgradeSprites[availableTutorialPages[tutorialPage - 1]].upgradeSprites[i].sprites[upgradeTier - 1];
                upgradeImages[i].GetComponent<Image>().color = upgradeTier < 1 ? Color.clear : Color.white;
                for (int j = 0; j < upgradeImages[i].transform.childCount; j++)
                {
                    upgradeImages[i].transform.GetChild(j).gameObject.SetActive(j < upgradeTier);
                    if (upgradeImages[i].transform.GetChild(j).gameObject.activeSelf)
                    {
                        upgradeImages[i].transform.GetChild(j).GetComponent<Animator>().SetInteger("Minigame", availableTutorialPages[tutorialPage - 1] + 1);
                        upgradeImages[i].transform.GetChild(j).GetComponent<Animator>().SetInteger("Upgrade", i + 1);
                        upgradeImages[i].transform.GetChild(j).GetComponent<Animator>().SetInteger("Tier", j + 1);
                    }
                }
            }
        }
        foreach (GameObject divider in dividers)
        {
            divider.SetActive(tutorialPage != 0);
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