using System.Collections.Generic;
using UnityEngine;

public class Region : MonoBehaviour
{
    public bool isUnlocked;
    public int minigameSpawnCount;
    public GameObject minigameMarker;
    public trashTypes trashType;
    public bossTypes bossEnum;
    public List<NameController> bossControllers = new List<NameController>();

    private Collider2D regionCollider;
    private List<Transform> minigameSpawnPoints = new List<Transform>();
    private bool isStoryCleared;
    private string boss;
    private FloatingObjects floatingObjectScript;

    private void Start()
    {
        floatingObjectScript = GameObject.Find("Map").GetComponent<FloatingObjects>();
        int minigameSpawnsCount = transform.Find("MinigameSpawns").childCount;
        if (minigameSpawnCount > minigameSpawnsCount)
        {
            minigameSpawnCount = minigameSpawnsCount;
        }
        regionCollider = GetComponent<Collider2D>();
        Unlock(isUnlocked);
        isStoryCleared = PlayerPrefs.GetInt("StoryCleared", 0) == 1;
        boss = bossEnum.ToString();
        foreach (NameController bossType in bossControllers)
        {
            if (bossType.name == boss)
            {
                transform.Find("BossIsland/SpriteBoss").GetComponent<Animator>().runtimeAnimatorController = bossType.bossController;
                //transform.Find("BossIsland/SpriteRipple").GetComponent<Animator>().runtimeAnimatorController = bossType.rippleController;
            }
        }
    }

    private void Update()
    {
        if (Application.isEditor)
        {
            EditorUpdate();
        }        
    }

    public void Unlock(bool state)
    {
        isUnlocked = state;
        regionCollider.enabled = !state;
        if (isUnlocked)
        {
            SpawnMinigames();
        }
        else
        {
            foreach (Transform minigame in GameObject.Find("Minigames").transform)
            {
                floatingObjectScript.objectsToRemove.Add(minigame.gameObject);
                Destroy(minigame.gameObject);
            }
        }
    }

    private void SpawnMinigames()
    {
        minigameSpawnPoints.Clear();
        foreach (Transform spawnPoint in transform.Find("MinigameSpawns"))
        {
            minigameSpawnPoints.Add(spawnPoint);
        }
        for (int i = 0; i < minigameSpawnCount; i++)
        {
            int j = Random.Range(0, minigameSpawnPoints.Count);
            MinigameMarker newMarker = Instantiate(minigameMarker, minigameSpawnPoints[j].position, Quaternion.identity, GameObject.Find("Minigames").transform).GetComponent<MinigameMarker>();
            newMarker.trashType = trashType.ToString();
            floatingObjectScript.objectsToAdd.Add(newMarker.gameObject);
            minigameSpawnPoints.RemoveAt(j);
        }
    }

    private void EditorUpdate()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Unlock(!isUnlocked);
            print("Regions Unlocked : " + isUnlocked);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            isStoryCleared = !isStoryCleared;
            print("Story Cleared :" + boss + " = " + isStoryCleared);
        }
    }

    public enum trashTypes
    {
        Plastic,
        Metal,
        Glass
    }

    public enum bossTypes
    {
        Eel,
        Crab,
        Whale
    }
}