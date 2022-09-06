using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public VideoManager videoManager;

    private void Start()
    {
        transform.Find("Settings/Music_Sound").GetComponent<Slider>().value = GameManager.Instance.musicVolume;
        transform.Find("Settings/SFX_Sound").GetComponent<Slider>().value = GameManager.Instance.sFXVolume;
        transform.Find("Settings/Music/Cross").gameObject.SetActive(GameManager.Instance.musicMuted == 0);
        transform.Find("Settings/SFX/Cross").gameObject.SetActive(GameManager.Instance.sFXMuted == 0);
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
