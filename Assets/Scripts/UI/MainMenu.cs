using UnityEngine;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public void StartGame()
    {
        PlayerPrefs.SetInt("isLoaded", 0);
        GameObject.Find("CloudCover").SetActive(false);
        BongleIsland bongleIsland = GameObject.Find("BongleIsland").GetComponent<BongleIsland>();
        bongleIsland.isInputEnabled = true;
        foreach  (Transform popup in GameObject.Find("UI/PopupsContainer").transform)
        {
            popup.gameObject.SetActive(true);
        }
        foreach (GameObject uI in GameObject.Find("Map").GetComponent<Map>().inGameUI)
        {
            uI.SetActive(true);
        }
        gameObject.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
        PlayerPrefs.SetInt("isLoaded", 1);
    }
}
