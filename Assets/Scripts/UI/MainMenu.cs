using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public VideoManager videoManager;

    // Start is called before the first frame update
    public void StartGame()
    {
        if (PlayerPrefs.GetInt("storyPoint", 0) == 0)
        {
            videoManager.PlayCutscene(0);
            PlayerPrefs.SetInt("storyPoint", 1);
            print("Story Point : " + PlayerPrefs.GetInt("storyPoint", 0));
        }
        else
        {
            GameObject.Find("CloudCover").SetActive(false);
        }
        PlayerPrefs.SetInt("isLoaded", 0);
        GameObject.Find("BongleIsland").GetComponent<BongleIsland>().isInputEnabled = true;
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
