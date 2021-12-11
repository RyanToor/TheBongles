using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    public GameObject minigameMarker;
    public string trashType;
    public GameObject loadScreen;
    public GameObject[] stars;
    public Color disabledStarColour;
    public PopupInfo[] popupInfo;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt("isLoaded", 1) == 1)
        {
            gameObject.SetActive(false);
        }

        for (int i = 1; i <= stars.Length; i++)
         {
            stars[i - 1].GetComponent<Image>().color = (PlayerPrefs.GetInt("Stars1", 0) >= i) ? Color.white : disabledStarColour;
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
        switch (trashType)
        {
            case "Plastic":
                GameObject newLoadScreen = Instantiate(loadScreen, new Vector3(960, 540, 0), Quaternion.identity);
                DontDestroyOnLoad(newLoadScreen);
                SceneManager.LoadScene("VerticalScroller");
                break;
            default:
                break;
        }
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
        public string trashType;
        public string title;
        public Sprite logo;
    }
}
