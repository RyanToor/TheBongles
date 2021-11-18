using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public GameObject loadScreen;

    public void Retry() 
    {
        GameObject newLoadScreen = Instantiate(loadScreen, new Vector3(960, 540, 0), Quaternion.identity);
        DontDestroyOnLoad(newLoadScreen);
        SceneManager.LoadScene("VerticalScroller");
    }
    public void EndScreen()
    {

    }
}
