using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeMenu : MonoBehaviour
{
    public VideoManager videoManager;
    public float lerpSpeed, lerpDir = -1, examplePanelPauseDuration, examplePanelScaleSpeed, storyUpgradeBlinkDuration;
    public Vector3 closedPos, openPos;
    public Color upgradeAvailableColour, upgradeActiveColour, barUnavailableColour, barAvailableColour;
    public List<Sprite> sprites;
    public List<Text> trashReadouts;
    public Button upgradeButton;
    public GameObject[] tabs, storyCostContainers;
    public UpgradePanel[] upgradeCosts;
    public Sprite[] storySprites;
    public StoryUpgradeCosts[] storyUpgradeCosts;
    public Sprite[] enabledSprites;
    public UpgradeButton[] upgradeButtons;
    public RectTransform upgradeExamplePanel;

    private AudioManager audioManager;
    private float lerpPos = 0;
    private readonly string[] trashNames = new string[] { "Plastic", "Metal", "Glass" };
    private int currentTabIndex;
    private Coroutine currentExampleCoroutine, storyButtonBlink;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = GameObject.Find("SoundManager").GetComponent<AudioManager>();
        currentTabIndex = 0;
    }

    private void Update()
    {
        if (lerpPos != Mathf.Clamp(lerpDir, 0, 1))
        {
            lerpPos = Mathf.Clamp(Mathf.Lerp(0, 1, lerpPos + lerpDir * lerpSpeed), 0, 1);
            transform.localPosition = Vector3.Lerp(closedPos, openPos, lerpPos);
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            FlipLerpDir();
        }
        if (Application.isEditor)
        {
            EditorUpdate();
        }
    }

    public void RefreshReadouts()
    {
        foreach (UpgradeButton button in upgradeButtons)
        {
            button.upgradeMenu = this;
        }
        int trashToRefresh;
        if (GameManager.Instance.trashCounts["Glass"] > 0)
        {
            trashToRefresh = 2;
            foreach (Text readout in trashReadouts)
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
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            int upgradeTier = GameManager.Instance.upgrades[currentTabIndex][i];
            upgradeButtons[i].upgradeIndicies = new Vector3Int(currentTabIndex, i, upgradeTier);
            upgradeButtons[i].upgrade = upgradeCosts[currentTabIndex].upgrades[i];
            upgradeButtons[i].UpdateContents(GameManager.Instance.upgrades[currentTabIndex][i], UpgradeAffordabilityCheck(new Vector3Int(currentTabIndex, i, upgradeTier)));
        }
        int currentLevel = GameManager.Instance.MaxRegion();
        bool upgradeReady = true;
        if (currentLevel < 4 && GameManager.Instance.storyPoint != 5 && GameManager.Instance.storyPoint != 9)
        {
            transform.Find("LeftPanel/UpgradeBackground/Story").gameObject.SetActive(true);
            for (int i = 0; i < storyCostContainers.Length; i++)
            {
                if (i < currentLevel)
                {
                    Vector3 barFill;
                    storyCostContainers[i].SetActive(true);
                    storyCostContainers[i].transform.Find("Text").GetComponent<Text>().text = storyUpgradeCosts[currentLevel - 1].upgradeCosts[i].cost.ToString();
                    barFill = new Vector3(Mathf.Clamp((float)GameManager.Instance.trashCounts[trashNames[i]] / storyUpgradeCosts[currentLevel - 1].upgradeCosts[i].cost, 0, 1), 1, 1);
                    storyCostContainers[i].transform.Find("BarFill").localScale = barFill;
                    storyCostContainers[i].transform.Find("BarFill").GetComponent<Image>().color = (barFill.x == 1) ? barAvailableColour : barUnavailableColour;
                    if (barFill.x < 1)
                    {
                        upgradeReady = false;
                    }
                }
                else
                {
                    storyCostContainers[i].SetActive(false);
                }
            }
            transform.Find("LeftPanel/UpgradeBackground/Story").GetComponent<Image>().sprite = storySprites[currentLevel - 1];
        }
        else
        {
            transform.Find("LeftPanel/UpgradeBackground/Story").gameObject.SetActive(false);
            upgradeReady = false;
        }
        switch (currentLevel)
        {
            case 4:
                transform.Find("LeftPanel/UpgradeBackground/EelTab/ImageCrab").GetComponent<Image>().sprite = enabledSprites[0];
                transform.Find("LeftPanel/UpgradeBackground/EelTab/ImageWhale").GetComponent<Image>().sprite = enabledSprites[1];
                transform.Find("LeftPanel/UpgradeBackground/CrabTab/ImageWhale").GetComponent<Image>().sprite = enabledSprites[1];
                break;
            case 3:
                transform.Find("LeftPanel/UpgradeBackground/EelTab/ImageCrab").GetComponent<Image>().sprite = enabledSprites[0];
                transform.Find("LeftPanel/UpgradeBackground/EelTab/ImageWhale").GetComponent<Image>().sprite = enabledSprites[1];
                transform.Find("LeftPanel/UpgradeBackground/CrabTab/ImageWhale").GetComponent<Image>().sprite = enabledSprites[1];
                break;
            case 2:
                transform.Find("LeftPanel/UpgradeBackground/EelTab/ImageCrab").GetComponent<Image>().sprite = enabledSprites[0];
                break;
            default:
                break;
        }
        upgradeButton.interactable = upgradeReady;
        if (upgradeReady && gameObject.activeSelf)
        {
            if (storyButtonBlink == null)
            {
                storyButtonBlink = StartCoroutine(StoryButtonBlink());
            }
        }
        else
        {
            if (storyButtonBlink != null)
            {
                StopCoroutine(storyButtonBlink);
                storyButtonBlink = null;
            }
        }
    }

    public void FlipLerpDir()
    {
        if (GameManager.Instance.storyPoint > 1)
        {
            print("Upgrade Menu Dir Flipped");
            lerpDir *= -1;
        }
    }

    public void SwitchTab(GameObject newTab)
    {
        int newTabIndex = System.Array.IndexOf(tabs, newTab);
        if (newTabIndex < GameManager.Instance.MaxRegion())
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
                //print(upgradeCost[i].type.ToString() + " : " + GameManager.Instance.trashCounts[upgradeCost[i].type.ToString()] + " / " + upgradeCost[i].cost);
                if (GameManager.Instance.trashCounts[upgradeCost[i].type.ToString()] < upgradeCost[i].cost)
                {
                    canAffordUpgrade = false;
                }
            }
            //print(canAffordUpgrade);
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

    private void EditorUpdate()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            for (int i = 0; i < upgradeCosts.Length; i++)
            {
                for (int j = 0; j < upgradeCosts[i].upgrades.Length; j++)
                {
                    GameManager.Instance.upgrades[i][j] = 0;
                    RefreshReadouts();
                }
            }
            print("Upgrades Reset");
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