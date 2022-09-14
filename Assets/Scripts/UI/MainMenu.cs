using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenu : MonoBehaviour
{
    public VideoManager videoManager;

    [SerializeField] private string backgroundUrl;

    private void Start()
    {
        transform.Find("Settings/Music_Sound").GetComponent<Slider>().value = GameManager.Instance.musicVolume;
        transform.Find("Settings/SFX_Sound").GetComponent<Slider>().value = GameManager.Instance.sFXVolume;
        transform.Find("Settings/Music/Cross").gameObject.SetActive(GameManager.Instance.musicMuted == 0);
        transform.Find("Settings/SFX/Cross").gameObject.SetActive(GameManager.Instance.sFXMuted == 0);
        VideoPlayer videoPlayer = transform.Find("StartBackground").GetComponent<VideoPlayer>();
        if (GameManager.Instance.isWeb)
        {
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = backgroundUrl;
        }
        videoPlayer.Play();
    }

    public void StartGame()
    {
        InputManager.Instance.CloseMenu(transform);
        gameObject.SetActive(false);
        if (GameManager.Instance.storyPoint == 1)
        {
            GameObject.Find("CloudCover").SetActive(false);
        }
        GameManager.Instance.StartGame();
    }

    public void ResetGame()
    {
        GameManager.Instance.ResetGame();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
