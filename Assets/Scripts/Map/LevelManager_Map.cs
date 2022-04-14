using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager_Map : LevelManager
{
    public NameSpriteArray[] trashSprites;
    public int randomTrashAmount, birdAmount, castleStoryPoint;
    public GameObject map, randomTrashContainer, randomTrashPrefab, birdPrefab, castle;
    public float trashClearBorder;
    public CapsuleCollider2D castleSmallCollider, castleBigCollider;
    public KeyCode[] drawLineCheat;

    [HideInInspector]
    public GameObject upgradesUI;
    [HideInInspector]
    public Coroutine arrowCoroutine;

    [SerializeField] private GameObject cloudCover, bongleIsland, pauseUI;

    private Vector3 bongleIslandPosition;
    private FloatingObjects floatingObjectsScript;
    private Rect mapArea;
    private int drawUnlockProgress = 0;
    private Coroutine[] promptCoroutines;

    protected override void Awake()
    {
        base.Awake();
        floatingObjectsScript = GetComponent<FloatingObjects>();
    }

    protected override void Start()
    {
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
        Destroy(GameObject.Find("LoadingCanvas(Clone)"));
        promptCoroutines = new Coroutine[GameObject.Find("BossRegions").transform.childCount];
        UpdateRegionsUnlocked();
        RespawnTrash();
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
            GameObject.Find("SoundManager").GetComponent<AudioManager>().PlayMusic("Map");
        }
        if (GameManager.Instance.gameStarted)
        {
            GameObject.Find("UI/StoryVideo").GetComponent<VideoManager>().CheckCutscene();
        }
        StartCoroutine(CheckDrawCheat());
        SpawnBirds();
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
        RespawnTrash();
        GameObject.Find("UI/StoryVideo").GetComponent<VideoManager>().CheckCutscene();
        Time.timeScale = 1;
        StartCoroutine(Camera.main.GetComponent<MapCamera>().ZoomToMap());
        base.StartGame();
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
            if (promptCoroutines[i] != null)
            {
                StopCoroutine(promptCoroutines[i]);
            }
            arrowCoroutine = StartCoroutine(GameObject.Find("BossRegions").transform.GetChild(i).GetComponent<Region>().CheckPrompt());
        }
    }

    public void RespawnTrash()
    {
        floatingObjectsScript.RemoveAll();
        for (int i = 0; i < randomTrashAmount; i++)
        {
            SpawnRandomTrash();
        }
    }

    public void SpawnRandomTrash()
    {
        GameObject newTrash = Instantiate(randomTrashPrefab, new Vector3(Random.Range(mapArea.min.x, mapArea.max.x), Random.Range(mapArea.min.y, mapArea.max.y), 0), Quaternion.identity, randomTrashContainer.transform);
        int trashType = Mathf.Clamp(Random.Range(0, GameManager.Instance.MaxRegion()), 0, 2);
        RandomTrash trashScript = newTrash.GetComponent<RandomTrash>();
        int trashIndex = Random.Range(0, trashSprites[trashType].sprites.Length);
        trashScript.sprite = trashSprites[trashType].sprites[trashIndex];
        trashScript.spriteIndex = trashIndex;
        trashScript.trashType = trashSprites[trashType].name;
        trashScript.floatingObjectsScript = floatingObjectsScript;
        floatingObjectsScript.objectsToAdd.Add(newTrash);
    }

    private void SpawnBirds()
    {
        for (int i = 0; i < birdAmount; i++)
        {
            GameObject newBird = Instantiate(birdPrefab, new Vector3(Random.Range(mapArea.min.x, mapArea.max.x), Random.Range(mapArea.min.y, mapArea.max.y), 0), Quaternion.identity, GameObject.Find("MapObjects").transform);
            newBird.GetComponent<SpriteRenderer>().sortingOrder = 0;
            newBird.GetComponent<SpriteRenderer>().flipX = Random.value > 0.5f;
            newBird.transform.rotation = Quaternion.Euler(-60f, 0, 0);
        }
    }

    protected override void Update()
    {
        bongleIslandPosition = bongleIsland.transform.position;
        if (Application.isEditor)
        {
            EditorOnly();
        }
        base.Update();
    }

    protected override void SendSaveData()
    {
        GameManager.Instance.bongleIslandPosition = bongleIslandPosition;
        base.SendSaveData();
    }

    private IEnumerator CheckDrawCheat()
    {
        while (drawUnlockProgress < drawLineCheat.Length)
        {
            if (Input.GetKeyDown(drawLineCheat[drawUnlockProgress]))
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
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Minus))
        {
            if (Input.GetKeyDown(KeyCode.Equals))
            {
                GameManager.Instance.storyPoint = Mathf.Clamp(GameManager.Instance.storyPoint + 1, 1, 3);
            }
            else if (Input.GetKeyDown(KeyCode.Minus))
            {
                GameManager.Instance.storyPoint = Mathf.Clamp(GameManager.Instance.storyPoint - 1, 1, 3);
            }
            print("Max Region : " + GameManager.Instance.MaxRegion());
            UpdateRegionsUnlocked();
            RespawnTrash();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            print(GameManager.Instance.storyPoint);
        }
    }
}
