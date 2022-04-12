using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public bool isTrashTrigger, isTrashRandom, isCursorVisible;
    public float mouseVisibleDuration = 2f;
    public TrashType levelTrashType;
    public Color collectionIndicatorColor;

    [HideInInspector]
    public int plastic = 0, metal = 0, glass = 0;
    public Coroutine mouseVisibleCoroutine;
    public bool isLoaded;

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
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && SceneManager.GetActiveScene().name != "Map" && !isCursorVisible)
        {
            mouseVisibleCoroutine = StartCoroutine(MouseVisible());
        }
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

    public void StopMouseVisibleCoroutine()
    {
        if (mouseVisibleCoroutine != null)
        {
            StopCoroutine(mouseVisibleCoroutine);
        }
    }

    private IEnumerator MouseVisible()
	{
        Cursor.visible = true;
        isCursorVisible = true;
        float duration = 0;
        while (duration < mouseVisibleDuration)
        {
            duration += Time.deltaTime;
            yield return null;
        }
        Cursor.visible = false;
        isCursorVisible = false;
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
