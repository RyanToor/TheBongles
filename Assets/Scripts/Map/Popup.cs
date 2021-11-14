using UnityEngine;
using UnityEngine.SceneManagement;

public class Popup : MonoBehaviour
{
    public GameObject minigameMarker;
    public string trashType;

    // Start is called before the first frame update
    void Start()
    {
        
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
                SceneManager.LoadScene("VerticalScroller");
                break;
            default:
                break;
        }
    }
}
