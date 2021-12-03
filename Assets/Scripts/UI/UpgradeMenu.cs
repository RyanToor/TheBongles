using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeMenu : MonoBehaviour
{
    public float lerpSpeed, lerpDir = -1;
    public Vector3 closedPos, openPos;
    public Color upgradeActiveColour;
    public List<Sprite> sprites;
    public List<Text> trashReadouts;
    public Button upgradeButton;
    public GameObject[] tabs, storyCostContainers;
    public UpgradeCostArray[] upgradeCosts;
    public Sprite[] StorySprites;
    public UpgradeCostArray[] storyUpgradeCosts;

    [HideInInspector]
    public int currentlyVisibleUpgrades;

    private float lerpPos = 0;
    private string[] trashNames = new string[] { "Plastic", "Metal", "Glass" };

    // Start is called before the first frame update
    void Start()
    {
        UpdateReadouts();
        UpdateStoryPanel();
        foreach (UpgradeCostArray costArray in upgradeCosts)
        {
            foreach (UpgradeCost cost in costArray.upgradeCosts)
            {
                cost.text.text = cost.cost.ToString();
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
    }

    public void UpdateReadouts()
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
    }

    public void UpdateStoryPanel()
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
        if (PlayerPrefs.GetInt("StoryStarted", 0) == 1)
        {
            print("Upgrade Menu Dir Flipped");
            lerpDir *= -1;
        }
    }

    public void SwitchTab(GameObject newTab)
    {
        if (System.Array.IndexOf(tabs, newTab) < PlayerPrefs.GetInt("maxRegion", 1))
        {
            foreach (GameObject tab in tabs)
            {
                tab.SetActive(tab == newTab);
            }
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
    public struct UpgradeCostArray
    {
        public UpgradeCost[] upgradeCosts;
    }

    public enum TrashType
    {
        Plastic,
        Metal,
        Glass
    }
}
