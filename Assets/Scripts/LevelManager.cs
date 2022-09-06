using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public bool isTrashTrigger, isTrashRandom;
    public float mouseVisibleDuration = 2f;
    public TrashType levelTrashType;
    public Color collectionIndicatorColor;

    [HideInInspector] public int buildIndex, plastic = 0, metal = 0, glass = 0;
    [HideInInspector] public Coroutine mouseVisibleCoroutine;
    [HideInInspector] public bool isLoaded;
    [HideInInspector] public PromptManager promptManager;

    protected virtual void Awake()
    {
        promptManager = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Prompts").GetComponent<PromptManager>();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        StartCoroutine(CheckLoaded());
        GameManager.Instance.StartGameEvent += StartGame;
        Debug.Log(SceneManager.GetActiveScene().name + ", loaded at story point : " + GameManager.Instance.storyPoint);
    }

    protected virtual IEnumerator CheckLoaded()
    {
        buildIndex = SceneManager.GetActiveScene().buildIndex;
        while (Time.timeSinceLevelLoad < GameManager.Instance.minimumLoadDuration)
        {
            yield return null;
        }
        PlayLevelMusic();
        Destroy(GameObject.Find("LoadingCanvas(Clone)"));
        if (buildIndex != 0)
        {
            if (!GameManager.Instance.levelsPrompted[SceneManager.GetActiveScene().buildIndex])
            {
                GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Pause").GetComponent<PauseMenu>().StartPrompt();
                GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Pause/UpgradeBook").gameObject.SetActive(true);
                GameManager.Instance.PauseGame(true);
                GameManager.Instance.levelsPrompted[buildIndex] = true;
            }
        }
        isLoaded = true;
    }

    public virtual void StartGame()
    {
        
    }

    public virtual void PlayLevelMusic()
    {
        AudioManager.Instance.PlayMusic(buildIndex == 0 ? "Map" : buildIndex < 2 ? "Trash Hunt" : buildIndex < 3 ? "Throw the Robot Bubba" : "Scrap Grabber");
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        Mouse mouse = Mouse.current;
        if ((mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame) && SceneManager.GetActiveScene().name != "Map" && !Cursor.visible)
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
        InputManager.Instance.EnableCursor();
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
        InputManager.Instance.EnableCursor();
        float duration = 0;
        while (duration < mouseVisibleDuration)
        {
            duration += Time.deltaTime;
            yield return null;
        }
        Cursor.visible = false;
	}

    private void OnDestroy()
    {
        SendSaveData();
    }
}
