using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager_Map : LevelManager
{
    public NameSpriteArray[] trashSprites;
    public int randomTrashAmount, castleStoryPoint;
    public GameObject map, randomTrashContainer, randomTrashPrefab, castle;
    public float trashClearBorder;
    public Sprite castleBefore, castleAfter;
    public CapsuleCollider2D castleSmallCollider, castleBigCollider;

    [HideInInspector]
    public GameObject upgradesUI;

    [SerializeField] private GameObject cloudCover, bongleIsland, pauseUI;

    private Vector3 bongleIslandPosition;
    private FloatingObjects floatingObjectsScript;
    private Rect mapArea;

    protected override void Awake()
    {
        base.Awake();
        floatingObjectsScript = GetComponent<FloatingObjects>();
    }

    protected override void Start()
    {
        SpriteRenderer sprite = map.GetComponent<SpriteRenderer>();
        mapArea = new Rect(sprite.bounds.min + Vector3.one * trashClearBorder, sprite.bounds.size - Vector3.one * trashClearBorder * 2);
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
        UpdateRegionsUnlocked();
        GameObject.Find("SoundManager").GetComponent<AudioManager>().PlayMusic("Map");
        if (GameManager.Instance.gameStarted)
        {
            GameObject.Find("UI/StoryVideo").GetComponent<VideoManager>().CheckCutscene();
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
        RespawnTrash();
        GameObject.Find("UI/StoryVideo").GetComponent<VideoManager>().CheckCutscene();
        Time.timeScale = 1;
        base.StartGame();
    }

    public void UpdateRegionsUnlocked()
    {
        upgradesUI.GetComponent<UpgradeMenu>().RefreshReadouts();
        foreach (Transform regionTransform in GameObject.Find("BossRegions").transform)
        {
            Region regionScript = regionTransform.gameObject.GetComponent<Region>();
            Debug.Log("Max Region : " + GameManager.Instance.MaxRegion());
            regionScript.Unlock(GameManager.Instance.MaxRegion() > regionScript.regionOrder);
        }
        int storyPoint = GameManager.Instance.storyPoint;
        castle.GetComponent<SpriteRenderer>().sprite = storyPoint > castleStoryPoint? castleAfter: castleBefore;
        castleSmallCollider.enabled = storyPoint <= castleStoryPoint;
        castleBigCollider.enabled = storyPoint > castleStoryPoint;
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
        int trashType = Random.Range(0, GameManager.Instance.MaxRegion());
        RandomTrash trashScript = newTrash.GetComponent<RandomTrash>();
        int trashIndex = Random.Range(0, trashSprites[trashType].sprites.Length);
        trashScript.sprite = trashSprites[trashType].sprites[trashIndex];
        trashScript.spriteIndex = trashIndex;
        trashScript.trashType = trashSprites[trashType].name;
        trashScript.floatingObjectsScript = floatingObjectsScript;
        floatingObjectsScript.objectsToAdd.Add(newTrash);
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
            GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Map>().UpdateRegionsUnlocked();
            RespawnTrash();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            print(GameManager.Instance.storyPoint);
        }
    }
}
