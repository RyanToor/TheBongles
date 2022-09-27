using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UpgradeMenu : MonoBehaviour
{
    public Animator upgradeIconAnimator;
    public GameObject defaultButton, gamepadPrompt;
    public VideoManager videoManager;
    public float lerpSpeed, lerpDir = -1, examplePanelPauseDuration, examplePanelScaleSpeed, storyUpgradeBlinkDuration;
    public Color upgradeAvailableColour, upgradeActiveColour, barUnavailableColour, barAvailableColour;
    public List<Sprite> sprites;
    public List<TMPro.TextMeshProUGUI> trashReadouts;
    public Button upgradeButton;
    public Transform bumperButtons;
    public GameObject[] tabs, storyCostContainers;
    public UpgradePanel[] upgradeCosts;
    public Sprite[] storySprites;
    public StoryUpgradeCosts[] storyUpgradeCosts;
    public Sprite[] enabledSprites;
    public UpgradeButton[] upgradeButtons;
    public RectTransform upgradeExamplePanel;
    public Vector3 openTranslate;

    [SerializeField] private Sprite gamepadButton, playstationButton;

    private AudioManager audioManager;
    private float lerpPos = 0;
    private readonly string[] trashNames = new string[] { "Plastic", "Metal", "Glass" };
    private int currentTabIndex;
    private Coroutine currentExampleCoroutine;
    private Vector3 closedPos;
    private RectTransform leftPane;

    // Start is called before the first frame update
    void Awake()
    {
        foreach (UpgradeButton button in upgradeButtons)
        {
            button.upgradeMenu = this;
        }
    }

    private void Start()
    {
        leftPane = transform.GetChild(0).GetComponent<RectTransform>();
        closedPos = leftPane.localPosition;
        InputManager.Instance.UpgradeMenu += FlipLerpDir;
        InputManager.Instance.Shoulder += CheckShoulderTabSwitch;
        InputManager.Instance.SwitchControlScheme += ToggleGamepadIcons;
        audioManager = GameObject.Find("SoundManager").GetComponent<AudioManager>();
        currentTabIndex = 0;
    }

    private void Update()
    {
        if (lerpPos != Mathf.Clamp(lerpDir, 0, 1))
        {
            lerpPos = Mathf.Clamp(Mathf.Lerp(0, 1, lerpPos + lerpDir * lerpSpeed), 0, 1);
            leftPane.localPosition = Vector3.Lerp(closedPos, closedPos + openTranslate, lerpPos);
        }
        if (Application.isEditor)
        {
            EditorUpdate();
        }
    }

    public void RefreshReadouts()
    {
        int trashToRefresh;
        if (GameManager.Instance.trashCounts["Glass"] > 0)
        {
            trashToRefresh = 2;
            foreach (TMPro.TextMeshProUGUI readout in trashReadouts)
            {
                readout.enabled = true;
            }
        }
        else if (GameManager.Instance.trashCounts["Metal"] > 0)
        {
            trashToRefresh = 1;
            for (int i = 0; i < 2; i++)
            {
                trashReadouts[i].enabled = true;
            }
            trashReadouts[2].enabled = false;
        }
        else
        {
            trashToRefresh = 0;
            for (int i = 1; i < 3; i++)
            {
                trashReadouts[i].enabled = false;
            }
            trashReadouts[0].enabled = true;
        }
        transform.Find("LeftPanel/TrashCount").GetComponent<Image>().sprite = sprites[trashToRefresh];
        for (int i = 0; i <= trashToRefresh; i++)
        {
            trashReadouts[i].text = GameManager.Instance.trashCounts[trashReadouts[i].gameObject.name].ToString();
        }
        transform.Find("LeftPanel/TrashCount").GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
        bool upgradeAvailable = false;
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            int upgradeTier = GameManager.Instance.upgrades[currentTabIndex][i];
            upgradeButtons[i].upgradeIndicies = new Vector3Int(currentTabIndex, i, upgradeTier);
            upgradeButtons[i].upgrade = upgradeCosts[currentTabIndex].upgrades[i];
            bool currentUpgradeAvailable = UpgradeAffordabilityCheck(new Vector3Int(currentTabIndex, i, upgradeTier));
            upgradeButtons[i].UpdateContents(GameManager.Instance.upgrades[currentTabIndex][i], currentUpgradeAvailable);
            if (currentUpgradeAvailable)
            {
                upgradeAvailable = true;
            }
        }
        for (int i = 0; i < 3; i++)
        {
            if (upgradeAvailable)
            {
                break;
            }
            if (i != currentTabIndex)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (UpgradeAffordabilityCheck(new Vector3Int(i, j, GameManager.Instance.upgrades[i][j])))
                    {
                        upgradeAvailable = true;
                        break;
                    }
                }
            }
        }
        int currentLevel = GameManager.Instance.MaxRegion();
        bool storyUpgradeReady = true;
        if (currentLevel < 4 && GameManager.Instance.storyPoint != 5 && GameManager.Instance.storyPoint != 9)
        {
            transform.Find("LeftPanel/UpgradeBackground/Story").gameObject.SetActive(true);
            for (int i = 0; i < storyCostContainers.Length; i++)
            {
                if (i < currentLevel)
                {
                    Vector3 barFill;
                    storyCostContainers[i].SetActive(true);
                    storyCostContainers[i].transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = storyUpgradeCosts[currentLevel - 1].upgradeCosts[i].cost.ToString();
                    barFill = new Vector3(Mathf.Clamp((float)GameManager.Instance.trashCounts[trashNames[i]] / storyUpgradeCosts[currentLevel - 1].upgradeCosts[i].cost, 0, 1), 1, 1);
                    storyCostContainers[i].transform.Find("BarFill").localScale = barFill;
                    storyCostContainers[i].transform.Find("BarFill").GetComponent<Image>().color = (barFill.x == 1) ? barAvailableColour : barUnavailableColour;
                    if (barFill.x < 1)
                    {
                        storyUpgradeReady = false;
                    }
                }
                else
                {
                    storyCostContainers[i].SetActive(false);
                }
            }
            transform.Find("LeftPanel/UpgradeBackground/Story/UpgradeStoryButton/StoryImage").GetComponent<Image>().sprite = storySprites[currentLevel - 1];
        }
        else
        {
            transform.Find("LeftPanel/UpgradeBackground/Story").gameObject.SetActive(false);
            storyUpgradeReady = false;
        }
        if (GameManager.Instance.storyPoint > GameManager.Instance.storyMeetPoints[1])
        {
            transform.Find("LeftPanel/UpgradeBackground/EelTab/ImageCrab").GetComponent<Image>().sprite = enabledSprites[0];
        }
        if (GameManager.Instance.storyPoint > GameManager.Instance.storyMeetPoints[2])
        {
            transform.Find("LeftPanel/UpgradeBackground/EelTab/ImageWhale").GetComponent<Image>().sprite = enabledSprites[1];
            transform.Find("LeftPanel/UpgradeBackground/CrabTab/ImageWhale").GetComponent<Image>().sprite = enabledSprites[1];
        }
        upgradeButton.interactable = storyUpgradeReady;
        foreach (UpgradeButton upgradeButtonInstance in upgradeButtons)
        {
            Navigation newNavigation = upgradeButtonInstance.gameObject.GetComponent<Button>().navigation;
            newNavigation.selectOnUp = storyUpgradeReady ? upgradeButton : null;
            upgradeButtonInstance.gameObject.GetComponent<Button>().navigation = newNavigation;
        }
        transform.Find("LeftPanel/UpgradeBackground/Story/UpgradeStoryButton").GetComponent<Image>().color = storyUpgradeReady ? upgradeAvailableColour : Color.white;
        ToggleGamepadIcons();
        upgradeIconAnimator.SetBool("Available", storyUpgradeReady || upgradeAvailable);
    }

    public void FlipLerpDir()
    {
        if (GameManager.Instance.storyPoint > 1)
        {
            Debug.Log("Upgrade Menu Dir Flipped");
            lerpDir *= -1;
        }
        if (lerpDir == -1)
        {
            InputManager.Instance.CloseMenu(transform);
        }
        else
        {
            InputManager.Instance.SetSelectedButton(defaultButton);
            InputManager.Instance.SetBackButton(gameObject.transform.Find("LeftPanel/TrashCount").gameObject.GetComponent<Button>());
            InputManager.Instance.EnableUIInput();
        }
    }

    private void CheckShoulderTabSwitch(int direction)
    {
        int desiredTab = currentTabIndex + direction;
        if (desiredTab >= 0 && desiredTab < tabs.Length)
        {
            SwitchTab(tabs[desiredTab]);
            GameObject currentlySelectedObject = EventSystem.current.currentSelectedGameObject;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(currentlySelectedObject);
        }
    }

    public void SwitchTab(GameObject newTab)
    {
        int newTabIndex = System.Array.IndexOf(tabs, newTab);
        if (GameManager.Instance.storyPoint >= GameManager.Instance.storyMeetPoints[newTabIndex])
        {
            currentTabIndex = newTabIndex;
            foreach (GameObject tab in tabs)
            {
                tab.SetActive(tab == newTab);
            }
            RefreshReadouts();
        }
    }

    public void Upgrade(Vector3Int upgradeIndicies)
    {
        if (UpgradeAffordabilityCheck(upgradeIndicies))
        {
            UpgradeCost[] prices = upgradeCosts[upgradeIndicies.x].upgrades[upgradeIndicies.y].upgradeTiers[upgradeIndicies.z].upgradeCosts;
            for (int i = 0; i < prices.Length; i++)
            {
                GameManager.Instance.trashCounts[prices[i].type.ToString()] -= prices[i].cost;
            }
            GameManager.Instance.upgrades[upgradeIndicies.x][upgradeIndicies.y] ++;
            RefreshReadouts();
            GameObject.Find("BongleIsland").GetComponent<BongleIsland>().RefreshUpgrades();
            audioManager.PlaySFX("Twinkle");
            RefreshExamplePanel(upgradeIndicies + new Vector3Int(0, 0, 1));
        }
    }

    private bool UpgradeAffordabilityCheck(Vector3Int upgradeIndicies)
    {
        if (upgradeIndicies.z < upgradeCosts[upgradeIndicies.x].upgrades[upgradeIndicies.y].upgradeTiers.Length)
        {
            UpgradeCost[] upgradeCost = upgradeCosts[upgradeIndicies.x].upgrades[upgradeIndicies.y].upgradeTiers[upgradeIndicies.z].upgradeCosts;
            bool canAffordUpgrade = true;
            for (int i = 0; i < upgradeCost.Length; i++)
            {
                if (GameManager.Instance.trashCounts[upgradeCost[i].type.ToString()] < upgradeCost[i].cost)
                {
                    canAffordUpgrade = false;
                }
            }
            return canAffordUpgrade;
        }
        else
        {
            return false;
        }
    }

    public void StoryUpgradeButton()
    {
        bool affordable = true;
        UpgradeCost[] prices = storyUpgradeCosts[GameManager.Instance.MaxRegion() - 1].upgradeCosts;
        for (int i = 0; i < prices.Length; i++)
        {
            if (GameManager.Instance.trashCounts[prices[i].type.ToString()] < prices[i].cost)
            {
                affordable = false;
            }
        }
        if (affordable)
        {
            for (int i = 0; i < prices.Length; i++)
            {
                GameManager.Instance.trashCounts[prices[i].type.ToString()] -= prices[i].cost;
            }
            GameManager.Instance.storyPoint++;
            videoManager.CheckCutscene();
            Debug.Log("Unlocked Next Region");
        }
        foreach (Transform bossRegion in GameObject.Find("BossRegions").transform)
        {
            bossRegion.gameObject.GetComponent<Region>().RefreshSprites();
        }
    }

    public void ExamplePanelHovered(bool isHovered, Vector3Int buttonIndicies)
    {
        if (currentExampleCoroutine != null)
        {
            StopCoroutine(currentExampleCoroutine);
        }
        currentExampleCoroutine = StartCoroutine(ScaleExamplePanel((isHovered ? 1 : -1), buttonIndicies));
    }

    private IEnumerator ScaleExamplePanel(int direction, Vector3Int upgradeIndicies)
    {
        RefreshExamplePanel(upgradeIndicies);
        if (direction == -1 && upgradeExamplePanel.localScale.y == 1)
        {
            float duration = 0;
            while (duration < examplePanelPauseDuration)
            {
                duration += Time.deltaTime;
                yield return null;
            }
        }
        else if (direction == 1 && upgradeExamplePanel.localScale.y == 1)
        {
            yield break;
        }
        while (upgradeExamplePanel.localScale.y >= 0 && upgradeExamplePanel.localScale.y <= 1)
        {
            upgradeExamplePanel.localScale = new Vector3(upgradeExamplePanel.localScale.x, upgradeExamplePanel.localScale.y + direction * examplePanelScaleSpeed * Time.unscaledDeltaTime, upgradeExamplePanel.localScale.z);
            yield return null;
        }
        if (upgradeExamplePanel.localScale.y > 1)
        {
            upgradeExamplePanel.localScale = Vector3.one;
        }
        else if (upgradeExamplePanel.localScale.y < 0)
        {
            upgradeExamplePanel.localScale = new Vector3(1, 0, 1);
        }
    }

    public void RefreshExamplePanel(Vector3Int upgradeIndicies)
    {
        upgradeExamplePanel.GetChild(0).GetComponent<Animator>().SetInteger("Minigame", upgradeIndicies.x + 1);
        upgradeExamplePanel.GetChild(0).GetComponent<Animator>().SetInteger("Upgrade", upgradeIndicies.y + 1);
        upgradeExamplePanel.GetChild(0).GetComponent<Animator>().SetInteger("Tier", upgradeIndicies.z + 1);
    }
    private IEnumerator StoryButtonBlink()
    {
        float duration = 0;
        while (true)
        {
            duration += Time.deltaTime;
            upgradeButton.gameObject.GetComponent<Image>().color = (duration % 2 * storyUpgradeBlinkDuration > storyUpgradeBlinkDuration) ? Color.grey : Color.white;
            yield return null;
        }
    }

    private void ToggleGamepadIcons()
    {
        bool isGamepad = InputManager.Instance.playerInput.currentControlScheme == "Gamepad";
        foreach (Transform item in bumperButtons)
        {
            item.gameObject.SetActive(isGamepad);
            item.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = GameManager.Instance.playstationLayout ? item.name == "LB" ? "L1" : "R1" : item.name == "LB" ? "LB" : "RB";
        }
        gamepadPrompt.SetActive(isGamepad);
        gamepadPrompt.GetComponent<Image>().sprite = GameManager.Instance.playstationLayout ? playstationButton : gamepadButton;
    }

    private void OnDisable()
    {
        InputManager.Instance.UpgradeMenu -= FlipLerpDir;
        InputManager.Instance.Shoulder -= CheckShoulderTabSwitch;
        InputManager.Instance.SwitchControlScheme -= ToggleGamepadIcons;
    }

    private void EditorUpdate()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard.uKey.wasPressedThisFrame)
        {
            for (int i = 0; i < upgradeCosts.Length; i++)
            {
                for (int j = 0; j < upgradeCosts[i].upgrades.Length; j++)
                {
                    GameManager.Instance.upgrades[i][j] = 0;
                }
            }
            Debug.Log("Upgrades Reset");
            RefreshReadouts();
        }
    }
}

[System.Serializable]
public struct UpgradeCost
{
    public TrashType type;
    public int cost;
}

[System.Serializable]
public struct UpgradeTier
{
    public Sprite upgradeTierImage;
    public UpgradeCost[] upgradeCosts;
}

[System.Serializable]
public struct Upgrade
{
    public UpgradeTier[] upgradeTiers;
}

[System.Serializable]
public struct UpgradePanel
{
    public Upgrade[] upgrades;
}

[System.Serializable]
public struct StoryUpgradeCosts
{
    public UpgradeCost[] upgradeCosts;
}