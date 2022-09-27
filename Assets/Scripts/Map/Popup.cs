using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    public GameObject minigameMarker, spacePrompt, buttonPrompt;
    public TrashType trashType;
    public GameObject loadScreen;
    public GameObject[] stars;
    public Color disabledStarColour;
    public PopupInfo[] popupInfo;

    // Start is called before the first frame update
    void Start()
    {
        if (!GameManager.Instance.gameStarted)
        {
            gameObject.SetActive(false);
        }

        for (int i = 1; i <= stars.Length; i++)
         {
            stars[i - 1].GetComponent<Image>().color = (GameManager.Instance.highscoreStars[(int)trashType] >= i) ? Color.white : disabledStarColour;
         }

        foreach (PopupInfo info in popupInfo)
        {
            if (info.trashType == trashType)
            {
                transform.Find("Title").GetComponent<TMPro.TextMeshProUGUI>().text = info.title;
                transform.Find("Logo").GetComponent<Image>().sprite = info.logo;
                transform.Find("Logo").GetComponent<Image>().color = Color.white;
                break;
            }
        }

        InputManager.Instance.SwitchControlScheme += SwitchPrompts;
        SwitchPrompts();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Camera.main.WorldToScreenPoint(minigameMarker.transform.position);
    }

    public void LaunchMinigame()
    {
        if (GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>().isLoaded && GameManager.Instance.gameStarted)
        {
            GameManager.Instance.LoadMinigame(trashType);
        }
    }

    private void SwitchPrompts()
    {
        spacePrompt.SetActive(InputManager.Instance.inputMethod == "Keyboard&Mouse");
        buttonPrompt.SetActive(InputManager.Instance.inputMethod != "Keyboard&Mouse");
        buttonPrompt.GetComponent<Image>().sprite = InputManager.Instance.abilityPrompts[0].abilityPrompts[GameManager.Instance.playstationLayout ? 2 : 1].sprite;
    }

    public void OnClick()
    {
        GameObject.Find("SoundInterface").GetComponent<AudioInterface>().OnClick();
    }

    public void OnHover()
    {
        GameObject.Find("SoundInterface").GetComponent<AudioInterface>().OnHover();
    }

    private void OnDisable()
    {
        InputManager.Instance.SwitchControlScheme -= SwitchPrompts;
    }

    [System.Serializable]
    public struct PopupInfo
    {
        public TrashType trashType;
        public string title;
        public Sprite logo;
    }
}
