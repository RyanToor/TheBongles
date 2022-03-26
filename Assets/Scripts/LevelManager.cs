using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public bool isTrashTrigger, isTrashRandom;
    public TrashType levelTrashType;
    public int plastic = 0, metal = 0, glass = 0;

    protected virtual void Awake()
    {
        
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        Debug.Log("Story Point : " + GameManager.Instance.storyPoint);
    }

    public virtual void StartGame()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {

    }

    protected virtual void SendSaveData()
    {
        GameManager.Instance.trashCounts["Plastic"] += plastic;
        GameManager.Instance.trashCounts["Metal"] += metal;
        GameManager.Instance.trashCounts["Glass"] += glass;
        GameManager.Instance.SaveGame();
    }

    public void LoadMap()
    {
        Time.timeScale = 1f;
        GameObject newLoadScreen = Instantiate(GameManager.Instance.loadScreenPrefab, new Vector3(960, 540, 0), Quaternion.identity);
        DontDestroyOnLoad(newLoadScreen);
        Cursor.visible = true;
        SceneManager.LoadScene("Map");
    }

    private void OnDestroy()
    {
        SendSaveData();
    }

    private void OnApplicationQuit()
    {
        SendSaveData();
    }
}
