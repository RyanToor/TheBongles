using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    [HideInInspector]
    public Upgrade upgrade;
    [HideInInspector]
    public Vector3Int upgradeIndicies;
    [HideInInspector]
    public UpgradeMenu upgradeMenu;

    public Image image;
    public Image[] requirementImages;
    public Sprite[] requirementSprites;
    public float requirementOffset, requirementSeparation;
    public Color availableColour, unlockedColour, finalTierColour;

    private void Start()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
    }

    public void UpdateContents(int tier, bool available)
    {
        bool finalTier = upgradeMenu.upgradeCosts[upgradeIndicies[0]].upgrades[upgradeIndicies[1]].upgradeTiers.Length == tier;
        image.sprite = upgrade.upgradeTiers[Mathf.Clamp(tier, 0, upgradeMenu.upgradeCosts[upgradeIndicies[0]].upgrades[upgradeIndicies[1]].upgradeTiers.Length - 1)].upgradeTierImage;
        List<TrashType> requiredTrash = new List<TrashType>();
        List<int> requiredAmmounts = new List<int>();
        if (!finalTier)
        {
            foreach (UpgradeCost cost in upgrade.upgradeTiers[tier].upgradeCosts)
            {
                requiredTrash.Add(cost.type);
                requiredAmmounts.Add(cost.cost);
            }
        }
        for (int i = 0; i < requiredTrash.Count; i++)
        {
            requirementImages[i].sprite = requirementSprites[(int)requiredTrash[i]];
        }
        int costCount = requiredTrash.Count;
        for (int i = 0; i < 3; i++)
        {
            GameObject costValue = requirementImages[i].gameObject;
            if (i >= costCount)
            {
                costValue.SetActive(false);
            }
            else
            {
                costValue.SetActive(true);
                costValue.transform.localPosition = new Vector3(costValue.transform.localPosition.x, requirementOffset + requirementSeparation * (costCount - 1) / 2 - requirementSeparation * i, costValue.transform.localPosition.z);
                requirementImages[i].GetComponentInChildren<Text>().text = requiredAmmounts[i].ToString();
            }
        }
        if (available)
        {
            GetComponent<Image>().color = availableColour;
        }
        else if (finalTier)
        {
            GetComponent<Image>().color = finalTierColour;
        }
        else if (tier == 1)
        {
            GetComponent<Image>().color = unlockedColour;
        }
        else if (tier == 0)
        {
            GetComponent<Image>().color = Color.white;
        }
        else
        {
            GetComponent<Image>().color = Color.Lerp(unlockedColour, finalTierColour, (float)tier / upgrade.upgradeTiers.Length);
            if (tier > upgrade.upgradeTiers.Length - 1)
            {
                foreach (Transform child in gameObject.transform)
                {
                    if (child.gameObject.name != "Image")
                    {
                        child.gameObject.SetActive(false);
                    }
                    else
                    {
                        child.gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    public void RequestUpgrade()
    {
        upgradeMenu.Upgrade(upgradeIndicies);
    }

    public void UpgradeExampleHovered(bool isHovered)
    {
        upgradeMenu.ExamplePanelHovered(isHovered, upgradeIndicies);
    }
}
