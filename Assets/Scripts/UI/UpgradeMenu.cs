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
        }
        else if (PlayerPrefs.GetInt("Metal", 0) > 0)
        {
            trashToRefresh = 1;
        }
        else
        {
            trashToRefresh = 0;
        }
        transform.Find("LeftPanel/TrashCount").GetComponent<Image>().sprite = sprites[trashToRefresh];
        for (int i = 0; i <= trashToRefresh; i++)
        {
            trashReadouts[i].text = PlayerPrefs.GetInt(trashReadouts[i].gameObject.name, 0).ToString();
        }
    }
}
