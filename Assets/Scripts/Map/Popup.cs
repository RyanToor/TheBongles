using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    public GameObject minigameMarker;
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
                transform.Find("Title").GetComponent<Text>().text = info.title;
                transform.Find("Logo").GetComponent<Image>().sprite = info.logo;
                transform.Find("Logo").GetComponent<Image>().color = Color.white;
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Camera.main.WorldToScreenPoint(minigameMarker.transform.position);
    }

    public void LaunchMinigame()
    {
        GameManager.Instance.LoadMinigame(trashType);
    }

    public void OnClick()
    {
        GameObject.Find("SoundInterface").GetComponent<AudioInterface>().OnClick();
    }

    public void OnHover()
    {
        GameObject.Find("SoundInterface").GetComponent<AudioInterface>().OnHover();
    }

    [System.Serializable]
    public struct PopupInfo
    {
        public TrashType trashType;
        public string title;
        public Sprite logo;
    }
}
