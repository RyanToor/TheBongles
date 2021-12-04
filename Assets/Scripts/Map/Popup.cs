using UnityEngine;
using UnityEngine.SceneManagement;

public class Popup : MonoBehaviour
{
    public GameObject minigameMarker;
    public string trashType;
    public GameObject loadScreen;
    public AudioClip TrashHuntMusic;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt("isLoaded", 1) == 1)
        {
            gameObject.SetActive(false);
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
}
