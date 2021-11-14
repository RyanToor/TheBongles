using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public AudioSource SFX;
    public AudioClip OnClick;

    public static bool GameIsPaused = false;
    [Range(0, 1)]
    public float lerpSpeed;
    public Vector3 closedPos, openPos;

    public Animator rightPanel;

    public float lerpPos = 0, lerpDir = -1;
    private RectTransform rightPaneTransform;

    // Start is called before the first frame update
    void Start()
    {
        rightPaneTransform = transform.Find("RightPanel").GetComponent<RectTransform>();
        rightPaneTransform.localPosition = closedPos;
    }

    // Update is called once per frame
     void Update()
      {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    
        if (lerpPos != Mathf.Clamp(lerpDir, 0, 1))
        {
            lerpPos = Mathf.Clamp(Mathf.Lerp(0, 1, lerpPos + lerpDir * lerpSpeed), 0, 1);
            rightPaneTransform.localPosition = Vector3.Lerp(closedPos, openPos, lerpPos);
        }
    }
      public void Resume()
      {
        OpenCloseMenu();
        Time.timeScale = 1f;
        GameIsPaused = false;
      }

      void Pause()
      {
        OpenCloseMenu();
        Time.timeScale = 0f;
          GameIsPaused = true;
      }

    public void pauseGame()
    {
        if (GameIsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

public void OpenCloseMenu()
    {
        lerpDir *= -1;
        SFX.PlayOneShot(OnClick);
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}