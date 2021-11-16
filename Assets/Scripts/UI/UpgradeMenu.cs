using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeMenu : MonoBehaviour
{
    public List<Sprite> sprites;
    public List<Text> trashReadouts;

    // Start is called before the first frame update
    void Start()
    {
        UpdateReadouts();
    }

    public void UpdateReadouts()
    {
        int trashToRefresh = 0;
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
    }
}
