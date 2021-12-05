using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeMenu : MonoBehaviour
{
    public VideoManager videoManager;
    public float lerpSpeed, lerpDir = -1;
    public Vector3 closedPos, openPos;
    public Color upgradeAvailableColour, upgradeActiveColour;
    public List<Sprite> sprites;
    public List<Text> trashReadouts;
    public Button upgradeButton;
    public GameObject[] tabs, storyCostContainers;
    public UpgradePanel[] upgradeCosts;
    public Sprite[] StorySprites;
    public Upgrade[] storyUpgradeCosts;
    public GameObject[][] upgradeButtons;

    [HideInInspector]
    public int currentlyVisibleUpgrades;

    private AudioManager audioManager;
    private float lerpPos = 0;
    private string[] trashNames = new string[] { "Plastic", "Metal", "Glass" };
    private int currentTabIndex;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = GameObject.Find("SoundManager").GetComponent<AudioManager>();
        currentTabIndex = 0;
        RefreshReadouts();
        RefreshStoryPanel();
        foreach (UpgradePanel panel in upgradeCosts)
        {
            foreach (Upgrade upgrade in panel.upgrades)
            {
                foreach (UpgradeCost cost in upgrade.upgradeCosts)
                {
                    cost.text.text = cost.cost.ToString();
                }
            }
        }
        for (int i = 0; i < upgradeCosts.Length; i++)
        {
            for (int j = 0; j < upgradeCosts[i].upgrades.Length; j++)
            {
                upgradeCosts[i].upgrades[j].button.alphaHitTestMinimumThreshold = 0.5f;
            }
        }
    }

    private void Update()
    {
        if (lerpPos != Mathf.Clamp(lerpDir, 0, 1))
        {
            lerpPos = Mathf.Clamp(Mathf.Lerp(0, 1, lerpPos + lerpDir * lerpSpeed), 0, 1);
            transform.localPosition = Vector3.Lerp(closedPos, openPos, lerpPos);
        }
        if (Application.isEditor)
        {
            EditorUpdate();
        }
    }

    public void RefreshReadouts()
    {
        int trashToRefresh;
        if (PlayerPrefs.GetInt("Glass", 0) > 0)
        {
            trashToRefresh = 2;
            foreach (Text readout in trashReadouts)
            {
                readout.enabled = true;
            }
        }
        else if (PlayerPrefs.GetInt("Metal", 0) > 0)
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
            trashReadouts[i].text = PlayerPrefs.GetInt(trashReadouts[i].gameObject.name, 0).ToString();
        }
        transform.Find("LeftPanel/TrashCount").GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
        for (int i = 0; i < upgradeCosts[currentTabIndex].upgrades.Length; i++)
        {
            if (UpgradeAffordabilityCheck(new int[2] { currentTabIndex, i }) && PlayerPrefs.GetInt("upgrade" + currentTabIndex + i, 0) == 0)
            {
                upgradeCosts[currentTabIndex].upgrades[i].button.color = upgradeAvailableColour;
            }
            else if (PlayerPrefs.GetInt("upgrade" + currentTabIndex + i, 0) == 1)
            {
                upgradeCosts[currentTabIndex].upgrades[i].button.color = upgradeActiveColour;
                foreach (Transform child in upgradeCosts[currentTabIndex].upgrades[i].button.gameObject.transform)
                {
                    if (child.gameObject != upgradeCosts[currentTabIndex].upgrades[i].image)
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                upgradeCosts[currentTabIndex].upgrades[i].button.color = Color.white;
            }
        }
    }

    public void RefreshStoryPanel()
    {
        int currentLevel = PlayerPrefs.GetInt("maxRegion", 1);
        Vector3[] barFills = new Vector3[currentLevel];
        bool upgradeReady = true;
        for (int i = 0; i < storyCostContainers.Length; i++)
        {
            if (i < currentLevel)
            {
                storyCostContainers[i].SetActive(true);
                storyCostContainers[i].transform.Find("Text").GetComponent<Text>().text = storyUpgradeCosts[currentLevel - 1].upgradeCosts[i].cost.ToString();
                barFills[i] = new Vector3(Mathf.Clamp((float)PlayerPrefs.GetInt(trashNames[i], 0) / storyUpgradeCosts[currentLevel - 1].upgradeCosts[i].cost, 0, 1), 1, 1);
                storyCostContainers[i].transform.Find("BarFill").localScale = barFills[i];
                if (barFills[i].x < 1)
                {
                    upgradeReady = false;
                }
            }
            else
            {
                storyCostContainers[i].SetActive(false);
            }
        }
        upgradeButton.interactable = upgradeReady;
    }

    public void FlipLerpDir()
    {
        if (PlayerPrefs.GetInt("storyPoint", 0) > 1)
        {
            print("Upgrade Menu Dir Flipped");
            lerpDir *= -1;
        }
    }

    public void SwitchTab(GameObject newTab)
    {
        int newTabIndex = System.Array.IndexOf(tabs, newTab);
        if (newTabIndex < PlayerPrefs.GetInt("maxRegion", 1))
        {
            currentTabIndex = newTabIndex;
            foreach (GameObject tab in tabs)
            {
                tab.SetActive(tab == newTab);
            }
            RefreshReadouts();
        }
    }

    public void UpgradeButton(string upgradeLocation)
    {
        string[] splitUpgradeLocation = upgradeLocation.Split('-');
        int[] upgradeIndicies = new int[splitUpgradeLocation.Length];
        for (int i = 0; i < splitUpgradeLocation.Length; i++)
        {
            int.TryParse(splitUpgradeLocation[i], out upgradeIndicies[i]);
        }
        for (int i = 0; i < upgradeIndicies.Length; i++)
        {
            upgradeIndicies[i]--;
        }
        if (UpgradeAffordabilityCheck(upgradeIndicies))
        {
            UpgradeCost[] prices = upgradeCosts[upgradeIndicies[0]].upgrades[upgradeIndicies[1]].upgradeCosts;
            for (int i = 0; i < prices.Length; i++)
            {
                PlayerPrefs.SetInt(prices[i].type.ToString(), PlayerPrefs.GetInt(prices[i].type.ToString(), 0) - prices[i].cost);
            }
            PlayerPrefs.SetInt("upgrade" + upgradeIndicies[0] + upgradeIndicies[1], 1);
            RefreshReadouts();
            audioManager.PlaySFX("Twinkle");
        }
    }

    private bool UpgradeAffordabilityCheck(int[] upgradeIndicies)
    {
        UpgradeCost[] upgradeCost = upgradeCosts[upgradeIndicies[0]].upgrades[upgradeIndicies[1]].upgradeCosts;
        bool canAffordUpgrade = true;
        for (int i = 0; i < upgradeCost.Length; i++)
        {
            //print(upgradeCost[i].type.ToString() + " : " + PlayerPrefs.GetInt(upgradeCost[i].type.ToString(), 0) + " / " + upgradeCost[i].cost);
            if (PlayerPrefs.GetInt(upgradeCost[i].type.ToString(), 0) < upgradeCost[i].cost)
            {
                canAffordUpgrade = false;
            }
        }
        //print(canAffordUpgrade);
        return canAffordUpgrade;
    }

    public void StoryUpgradeButton()
    {
        bool affordable = true;
        UpgradeCost[] prices = storyUpgradeCosts[PlayerPrefs.GetInt("maxRegion", 1) - 1].upgradeCosts;
        for (int i = 0; i < prices.Length; i++)
        {
            print(prices[i].type.ToString() + " : " + PlayerPrefs.GetInt(prices[i].type.ToString(), 0) + " / " + prices[i].cost);
            if (PlayerPrefs.GetInt(prices[i].type.ToString(), 0) < prices[i].cost)
            {
                affordable = false;
            }
        }
        if (affordable)
        {
            for (int i = 0; i < prices.Length; i++)
            {
                PlayerPrefs.SetInt(prices[i].type.ToString(), PlayerPrefs.GetInt(prices[i].type.ToString(), 0) - prices[i].cost);
            }
            audioManager.PlaySFX("Twinkle");
            videoManager.PlayVideo(videoManager.storyVideos[PlayerPrefs.GetInt("maxRegion", 1) * 2]);
            PlayerPrefs.SetInt("maxRegion", Mathf.Clamp(PlayerPrefs.GetInt("maxRegion", 1) + 1, 1, 3));
            GameObject.Find("Map").GetComponent<Map>().UpdateRegionsUnlocked(PlayerPrefs.GetInt("maxRegion", 1));
            RefreshStoryPanel();
            RefreshReadouts();
            print("Unlocked Next Region");
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
                    PlayerPrefs.SetInt("upgrade" + i + j, 0);
                    foreach (Transform child in upgradeCosts[i].upgrades[j].button.gameObject.transform)
                    {
                        child.gameObject.SetActive(true);
                    }
                }
            }
            print("Upgrades Reset");
            RefreshReadouts();
        }
    }

    [System.Serializable]
    public class UpgradeCost
    {
        public Text text;
        public TrashType type;
        public int cost;
    }

    [System.Serializable]
    public struct Upgrade
    {
        public Image button;
        public GameObject image;
        public UpgradeCost[] upgradeCosts;
    }

    [System.Serializable]
    public struct UpgradePanel
    {
        public Upgrade[] upgrades;
    }

    public enum TrashType
    {
        Plastic,
        Metal,
        Glass
    }
}