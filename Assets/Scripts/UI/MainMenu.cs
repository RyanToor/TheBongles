using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public VideoManager videoManager;

    // Start is called before the first frame update
    public void StartGame()
    {
        gameObject.SetActive(false);
        if (GameManager.Instance.storyPoint == 1)
        {
            GameObject.Find("UI/Prompts").GetComponent<InputPrompts>().StartPrompt();
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
