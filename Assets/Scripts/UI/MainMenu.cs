using UnityEngine;
using UnityEngine.Audio;

public class MainMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public VideoManager videoManager;

    // Start is called before the first frame update
    public void StartGame()
    {
        if (PlayerPrefs.GetInt("storyPoint", 0) == 0)
        {
            videoManager.PlayVideo(videoManager.storyVideos[0]);
            PlayerPrefs.SetInt("storyPoint", 1);
            print("Story Point : " + PlayerPrefs.GetInt("storyPoint", 0));
        }
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
    public void SetVolume (float volume)
    {
        audioMixer.SetFloat("masterVolume", volume);
    }
}
