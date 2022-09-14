using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class LevelManager_Map : LevelManager
{
    public NameSpriteArray[] trashSprites;
    public int maxRandomTrash, secondsPerTrash, birdAmount, castleStoryPoint;
    public GameObject map, randomTrashPrefab, birdPrefab, castle;
    public float trashClearBorder;
    public CapsuleCollider2D castleSmallCollider, castleBigCollider;
    public Key[] keyboardDrawLineCheat;
    public GamepadButton[] gamepadDrawLineCheat;

    [HideInInspector]
    public GameObject upgradesUI;
    [HideInInspector]
    public Coroutine arrowCoroutine;

    [SerializeField] private GameObject cloudCover, bongleIsland, pauseUI;

    private Transform randomTrashContainer;
    private Vector3 bongleIslandPosition;
    private FloatingObjects floatingObjectsScript;
    private Rect mapArea;
    private int drawUnlockProgress = 0, remainingTrash;

    protected override void Awake()
    {
        base.Awake();
        floatingObjectsScript = GetComponent<FloatingObjects>();
    }

    protected override void Start()
    {
        randomTrashContainer = GameObject.Find("MapObjects").transform.Find("RandomTrash");
        SpriteRenderer sprite = map.GetComponent<SpriteRenderer>();
        mapArea = new Rect(sprite.bounds.min + Vector3.one * trashClearBorder, sprite.bounds.size - 2 * trashClearBorder * Vector3.one);
        GameObject.FindGameObjectWithTag("Player").transform.position = GameManager.Instance.bongleIslandPosition;
        if (GameManager.Instance.gameStarted == true)
        {
            GameObject.Find("CloudCover").SetActive(false);
            GameObject.Find("MainMenu").SetActive(false);
            GameObject.Find("BongleIsland").GetComponent<BongleIsland>().isInputEnabled = true;
        }
        else
        {
            upgradesUI.SetActive(false);
            pauseUI.SetActive(false);
        }
        UpdateRegionsUnlocked();
        RespawnTrash();
        if (GameManager.Instance.gameStarted)
        {
            GameObject.Find("UI/StoryVideo").GetComponent<VideoManager>().CheckCutscene();
        }
        StartCoroutine(CheckDrawCheat());
        for (int i = 0; i < birdAmount; i++)
        {
            SpawnBird();
        }
        base.Start();
    }

    public override void StartGame()
    {
        foreach (Transform popup in GameObject.Find("UI/PopupsContainer").transform)
        {
            popup.gameObject.SetActive(true);
        }
        upgradesUI.SetActive(true);
        pauseUI.SetActive(true);
        cloudCover.SetActive(false);
        UpdateRegionsUnlocked();
        upgradesUI.GetComponent<UpgradeMenu>().RefreshReadouts();
        GameObject.Find("UI/StoryVideo").GetComponent<VideoManager>().CheckCutscene();
        Time.timeScale = 1;
        StartCoroutine(Camera.main.GetComponent<MapCamera>().ZoomToMap());
        base.StartGame();
    }

    protected override IEnumerator CheckLoaded()
    {
        bool isVideoPlaying = false;
        foreach (VideoManager.Cutscene scene in GameObject.Find("UI/StoryVideo").GetComponent<VideoManager>().cutScenes)
        {
            if (scene.storyPoint == GameManager.Instance.storyPoint)
            {
                isVideoPlaying = true;
                break;
            }
        }
        if (!isVideoPlaying || !GameManager.Instance.gameStarted)
        {
            yield return base.CheckLoaded();
        }
        yield break;
    }

    public void UpdateRegionsUnlocked()
    {
        upgradesUI.GetComponent<UpgradeMenu>().RefreshReadouts();
        foreach (Transform regionTransform in GameObject.Find("BossRegions").transform)
        {
            Region regionScript = regionTransform.gameObject.GetComponent<Region>();
            regionScript.Unlock(GameManager.Instance.MaxRegion() > regionScript.regionOrder);
        }
        int storyPoint = GameManager.Instance.storyPoint;
        castle.GetComponent<Animator>().SetBool("Castle", storyPoint >= castleStoryPoint);
        castleSmallCollider.enabled = storyPoint < castleStoryPoint;
        castleBigCollider.enabled = storyPoint >= castleStoryPoint;
        for (int i = 0; i < GameObject.Find("BossRegions").transform.childCount; i++)
        {
            Region region = GameObject.Find("BossRegions").transform.GetChild(i).GetComponent<Region>();
            region.arrowCoroutine = StartCoroutine(region.CheckPrompt());
        }
    }

    public void RespawnTrash()
    {
        floatingObjectsScript.RemoveAll();
        int trashToSpawn = Mathf.FloorToInt(Mathf.Clamp(Mathf.Floor((float)((GameManager.Instance.SystemSeconds - GameManager.Instance.lastTrashSpawn) / secondsPerTrash)) + GameManager.Instance.remainingTrash, 0, maxRandomTrash));
        Debug.Log("Spawned " + trashToSpawn + " random trash, including " + GameManager.Instance.remainingTrash + " restored from last map unload.");
        for (int i = 0; i < trashToSpawn; i++)
        {
            SpawnRandomTrash();
        }
        GameManager.Instance.SetLastSpawnTime();
    }

    public void SpawnRandomTrash()
    {
        GameObject newTrash = Instantiate(randomTrashPrefab, new Vector3(Random.Range(mapArea.min.x, mapArea.max.x), Random.Range(mapArea.min.y, mapArea.max.y), 0), Quaternion.identity, randomTrashContainer);
        int trashType = Mathf.Clamp(Random.Range(0, GameManager.Instance.MaxRegion()), 0, 2);
        RandomTrash trashScript = newTrash.GetComponent<RandomTrash>();
        int trashIndex = Random.Range(0, trashSprites[trashType].sprites.Length);
        trashScript.sprite = trashSprites[trashType].sprites[trashIndex];
        trashScript.spriteIndex = trashIndex;
        trashScript.trashType = trashSprites[trashType].name;
        trashScript.floatingObjectsScript = floatingObjectsScript;
        floatingObjectsScript.objectsToAdd.Add(newTrash);
    }

    public void SpawnBird()
    {
        GameObject newBird = Instantiate(birdPrefab, new Vector3(Random.Range(mapArea.min.x, mapArea.max.x), Random.Range(mapArea.min.y, mapArea.max.y), 0), Quaternion.identity, GameObject.Find("MapObjects/Birds").transform);
        newBird.GetComponent<SpriteRenderer>().sortingOrder = 0;
        newBird.GetComponent<SpriteRenderer>().flipX = Random.value > 0.5f;
        newBird.transform.rotation = Quaternion.Euler(-60f, 0, 0);
    }

    protected override void Update()
    {
        bongleIslandPosition = bongleIsland.transform.position;
        remainingTrash = randomTrashContainer.childCount;
        if (Application.isEditor)
        {
            EditorOnly();
        }
        base.Update();
    }

    protected override void SendSaveData()
    {
        GameManager.Instance.bongleIslandPosition = bongleIslandPosition;
        GameManager.Instance.remainingTrash = remainingTrash;
        Debug.Log(remainingTrash + " remaining trash reported to Game Manager.");
        base.SendSaveData();
    }

    private IEnumerator CheckDrawCheat()
    {
        while (drawUnlockProgress < keyboardDrawLineCheat.Length)
        {
            if (Keyboard.current != null && Keyboard.current[keyboardDrawLineCheat[drawUnlockProgress]].wasPressedThisFrame || Gamepad.current != null && Gamepad.current[gamepadDrawLineCheat[drawUnlockProgress]].wasPressedThisFrame)
            {
                drawUnlockProgress += 1;
            }
            yield return null;
        }
        bongleIsland.GetComponent<BongleIsland>().pathLength = 200;
        bongleIsland.GetComponent<BongleIsland>().pathSeparation = 0.3f;
        bongleIsland.GetComponent<BongleIsland>().isDrawLineCheat = true;
    }

    private void EditorOnly()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard.equalsKey.wasPressedThisFrame || keyboard.minusKey.wasPressedThisFrame)
        {
            if (keyboard.equalsKey.wasPressedThisFrame)
            {
                GameManager.Instance.storyPoint = Mathf.Clamp(GameManager.Instance.storyPoint + 1, 1, 3);
            }
            else if (keyboard.minusKey.wasPressedThisFrame)
            {
                GameManager.Instance.storyPoint = Mathf.Clamp(GameManager.Instance.storyPoint - 1, 1, 3);
            }
            Debug.Log("Max Region : " + GameManager.Instance.MaxRegion());
            UpdateRegionsUnlocked();
            RespawnTrash();
        }
        if (keyboard.pKey.wasPressedThisFrame)
        {
            Debug.Log(GameManager.Instance.storyPoint);
        }
    }
}
