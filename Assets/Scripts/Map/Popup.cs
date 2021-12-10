using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    public GameObject minigameMarker;
    public string trashType;
    public GameObject loadScreen;
    public AudioClip TrashHuntMusic;
    public List<Stars> StarNum;

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

       /* for (int i = 1; 1 <= Stars.Length; i++)
        {
            Stars[i - 1].SetActive(((PlayerPrefs.Get("Star1", 0) >= i) ? true : false)
        }*/
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
    public class Stars
    {
        //Initialise name and audioclip and source
        public string StarNum;
        public Image Star;
    }
}
