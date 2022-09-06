using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public float minimumLoadDuration;
    public GameObject loadScreenPrefab, collectionIndicatorPrefab;
    public List<GameObject> inGameUI;
    public int[] regionStoryPoints;

    public event Action<bool> EnablePrompts;
    public Dictionary<string, int> trashCounts = new Dictionary<string, int>();
    [HideInInspector]
    public event Action StartGameEvent;

    private static GameManager instance;
    private bool isResetting = false;

    #region SavedFields
    [HideInInspector] public bool gameStarted = false;
    [HideInInspector] public Vector3 bongleIslandPosition;
    [HideInInspector] public double lastTrashSpawn;
    [HideInInspector] public int[][] upgrades;
    [HideInInspector] public int[] highscoreStars;
    [HideInInspector] public int storyPoint, remainingTrash;
    [HideInInspector] public bool[] levelsPrompted;
    #endregion

    #region Settings
    [HideInInspector] public int musicMuted = 1, sFXMuted = 1;
    [HideInInspector] public float musicVolume = 0.25f, sFXVolume = 0.5f;
    [HideInInspector] public bool allowVibration, playstationLayout, promptsEnabled;
    #endregion


    public static GameManager Instance
    {
        get{ return instance; }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        if (SceneManager.GetActiveScene().name == "Map")
        {
            inGameUI.Clear();
            inGameUI.Add(GameObject.Find("UI/Upgrades"));
            inGameUI.Add(GameObject.Find("UI/Pause"));
        }
        SceneManager.sceneLoaded += OnSceneLoad;
        trashCounts.Add("Plastic", 0);
        trashCounts.Add("Metal", 0);
        trashCounts.Add("Glass", 0);
        upgrades = new int[3][];
        for (int i = 0; i < upgrades.Length; i++)
        {
            upgrades[i] = new int[3] { 0, 0, 0};
        }
        highscoreStars = new int[] { 0, 0, 0 };
        LoadGame();
    }

    private void Start()
    {
        if (!gameStarted && SceneManager.GetActiveScene().name == "Map")
        {
            InputManager.Instance.EnableUIInput();
        }
    }

    public void StartGame()
    {
        StartGameEvent?.Invoke();
        gameStarted = true;
    }

    public int MaxRegion()
    {
        int maxRegion = 1;
        for (int i = 0; i < regionStoryPoints.Length; i++)
        {
            if (storyPoint >= regionStoryPoints[i])
            {
                maxRegion = i + 1;
            }
            else
            {
                break;
            }
        }
        return maxRegion;
    }

    public void SaveGame()
    {
        if (!isResetting)
        {
            Save save = new Save()
            {
                position = bongleIslandPosition,
                plastic = trashCounts["Plastic"],
                metal = trashCounts["Metal"],
                glass = trashCounts["Glass"],
                remainingTrash = remainingTrash,
                lastTrashSpawn = lastTrashSpawn,
                upgrades1 = upgrades[0],
                upgrades2 = upgrades[1],
                upgrades3 = upgrades[2],
                storyPoint = storyPoint,
                highscoreStars = highscoreStars,
                levelsPrompted = levelsPrompted
            };
            File.WriteAllText(Application.persistentDataPath + "/saveGame.json", JsonUtility.ToJson(save, true));
        }
    }

    public void SaveSettings()
    {
        Settings settings = new Settings()
        {
            musicMuted = musicMuted,
            sFXMuted = sFXMuted,
            musicVolume = musicVolume,
            sFXVolume = sFXVolume,
            allowVibration = allowVibration,
            playstationLayout = playstationLayout,
            promptsEnabled = promptsEnabled
        };
        File.WriteAllText(Application.persistentDataPath + "/settings.json", JsonUtility.ToJson(settings, true));
    }

    private void LoadGame()
    {
        Save loadedSave = new Save();
        if (File.Exists(Application.persistentDataPath + "/saveGame.json"))
        {
            loadedSave = JsonUtility.FromJson<Save>(File.ReadAllText(Application.persistentDataPath + "/saveGame.json"));
        }
        bongleIslandPosition = loadedSave.position;
        trashCounts["Plastic"] = loadedSave.plastic;
        trashCounts["Metal"] = loadedSave.metal;
        trashCounts["Glass"] = loadedSave.glass;
        remainingTrash = loadedSave.remainingTrash;
        if (loadedSave.lastTrashSpawn != 0)
        {
            lastTrashSpawn = loadedSave.lastTrashSpawn;
        }
        else
        {
            lastTrashSpawn = 0;
        }
        if (loadedSave.upgrades1 != null)
        {
            upgrades[0] = loadedSave.upgrades1;
            upgrades[1] = loadedSave.upgrades2;
            upgrades[2] = loadedSave.upgrades3;
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                upgrades[i] = new int[3] { 0, 0, 0 };
            }
        }
        storyPoint = loadedSave.storyPoint;
        if (loadedSave.highscoreStars != null)
        {
            highscoreStars = loadedSave.highscoreStars;
        }
        else
        {
            highscoreStars = new int[4] { 0, 0, 0, 0 };
        }
        if (loadedSave.levelsPrompted != null && loadedSave.levelsPrompted.Length != 0)
        {
            levelsPrompted = loadedSave.levelsPrompted;
        }
        else
        {
            levelsPrompted = new bool[4] { false, false, false, false };
        }

        if (File.Exists(Application.persistentDataPath + "/settings.json"))
        {
            Settings loadedSettings = JsonUtility.FromJson<Settings>(File.ReadAllText(Application.persistentDataPath + "/settings.json"));
            musicMuted = loadedSettings.musicMuted;
            sFXMuted = loadedSettings.sFXMuted;
            musicVolume = loadedSettings.musicVolume;
            sFXVolume = loadedSettings.sFXVolume;
            allowVibration = loadedSettings.allowVibration;
            playstationLayout = loadedSettings.playstationLayout;
            promptsEnabled = loadedSettings.promptsEnabled;
        }
        else
        {
            musicMuted = 1;
            sFXMuted = 1;
            musicVolume = 0.25f;
            sFXVolume = 0.5f;
            allowVibration = true;
            playstationLayout = false;
            promptsEnabled = true;
            SaveSettings();
        }
        isResetting = false;
    }

    public void ResetGame()
    {
        isResetting = true;
        Debug.Log(Application.persistentDataPath)
;       if (!File.Exists(Application.persistentDataPath + "/saveGame.json"))
        {
            Debug.Log("Save file not found.");
        }
        File.Delete(Application.persistentDataPath + "/saveGame.json");
        SceneManager.LoadScene("Map");
    }

    public void PauseGame(bool isPaused)
    {
        Time.timeScale = isPaused ? 0 : 1;
        if(isPaused || SceneManager.GetActiveScene().name == "Map")
        {
            InputManager.Instance.EnableCursor();
        }
    }

    public void LoadMinigame(TrashType trashType)
    {
        print(GameObject.Find("MapObjects/RandomTrash").transform.childCount);
        SaveGame();
        GameObject newLoadScreen = Instantiate(loadScreenPrefab, new Vector3(960, 540, 0), Quaternion.identity);
        DontDestroyOnLoad(newLoadScreen);
        Cursor.visible = false;
        Time.timeScale = 1;
        switch (trashType)
        {
            case TrashType.Plastic:
                SceneManager.LoadScene("VerticalScroller");
                break;
            case TrashType.Metal:
                SceneManager.LoadScene("ThrowTheRobot");
                break;
            case TrashType.Glass:
                SceneManager.LoadScene("ScrapGrabber");
                break;
            default:
                break;
        }
    }

    public void SpawnCollectionIndicator(Vector3 pos, Color textColor, string text = "", float maxAngle = -1, float maxDistance = -1, float speed = -1)
    {
        CollectionIndicator newIndicator = Instantiate(collectionIndicatorPrefab, Camera.main.WorldToScreenPoint(pos), Quaternion.identity, GameObject.FindGameObjectWithTag("MainCanvas").transform).GetComponent<CollectionIndicator>();
        newIndicator.startPos = pos;
        newIndicator.textColor = textColor;
        if (text != "")
        {
            newIndicator.text = text;
        }
        if (maxAngle > 0)
        {
            newIndicator.maxAngle = maxAngle;
        }
        if (maxDistance > 0)
        {
            newIndicator.maxTravelDistance = maxDistance;
        }
        if (speed > 0)
        {
            newIndicator.speed = speed;
        }
    }

    public void SetLastSpawnTime()
    {
        lastTrashSpawn = SystemSeconds;
    }

    public double SystemSeconds
    {
        get
        {
            return DateTime.Now.Subtract(DateTime.MinValue).TotalSeconds;
        }
    }

    public bool AllowVibration
    {
        get
        {
            return allowVibration;
        }
        set
        {
            allowVibration = value;
            InputManager.Instance.SwitchControls();
            SaveSettings();
        }
    }

    public bool PlaystationLayout
    {
        get
        {
            return playstationLayout;
        }
        set
        {
            playstationLayout = value;
            InputManager.Instance.SwitchControls();
            SaveSettings();
        }
    }

    public bool PromptsEnabled
    {
        get
        {
            return promptsEnabled;
        }
        set
        {
            promptsEnabled = value;
            EnablePrompts?.Invoke(value);
            SaveSettings();
        }
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        LoadGame();
    }

    public void QuitGame()
    {
        if (true)
        {

        }
        Application.Quit();
    }
}

[System.Serializable]
public enum TrashType
{
    Plastic,
    Metal,
    Glass
}

[System.Serializable]
struct Settings
{
    public int musicMuted, sFXMuted;
    public float musicVolume, sFXVolume;
    public bool allowVibration, playstationLayout, promptsEnabled;
}

[System.Serializable]
struct Save
{
    public Vector3 position;
    public int plastic, metal, glass, remainingTrash;
    public double lastTrashSpawn;
    public int[] upgrades1;
    public int[] upgrades2;
    public int[] upgrades3;
    public int storyPoint;
    public int[] highscoreStars;
    public bool[] levelsPrompted;
}

[System.Serializable]
public struct NameSprite
{
    public string name;
    public Sprite sprite;
}

[System.Serializable]
public struct NameSpriteArray
{
    public string name;
    public Sprite[] sprites;
}

[System.Serializable]
public struct NameController
{
    public string name;
    public RuntimeAnimatorController bossController;
    public RuntimeAnimatorController rippleController;
}