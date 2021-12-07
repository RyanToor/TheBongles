using System.Collections.Generic;
using UnityEngine;

public class Region : MonoBehaviour
{
    public float regionOrder;
    public bool isUnlocked;
    public int minigameSpawnCount;
    public GameObject minigameMarker;
    public trashTypes trashType;
    public bossTypes bossEnum;
    public List<NameController> bossControllers = new List<NameController>();

    private Collider2D regionCollider;
    private List<Transform> minigameSpawnPoints = new List<Transform>();
    private bool isStoryCleared, isBossMet;
    private string boss;
    private FloatingObjects floatingObjectScript;
    private Animator bossAnimator, rippleAnimator;

    private void Awake()
    {
        floatingObjectScript = GameObject.Find("Map").GetComponent<FloatingObjects>();
        int minigameSpawnsCount = transform.Find("MinigameSpawns").childCount;
        if (minigameSpawnCount > minigameSpawnsCount)
        {
            minigameSpawnCount = minigameSpawnsCount;
        }
        regionCollider = GetComponent<Collider2D>();
        RefreshSprites();
        boss = bossEnum.ToString();
        foreach (NameController bossType in bossControllers)
        {
            if (bossType.name == boss)
            {
                bossAnimator = transform.Find("BossIsland/SpriteBoss").GetComponent<Animator>();
                rippleAnimator = transform.Find("BossIsland/SpriteRipples").GetComponent<Animator>();
                bossAnimator.runtimeAnimatorController = bossType.bossController;
                rippleAnimator.runtimeAnimatorController = bossType.rippleController;
            }
        }        
    }

    private void Update()
    {
        bossAnimator.SetFloat("RandomChance", Random.Range(0f, 100f));
        if (Application.isEditor)
        {
            EditorUpdate();
        }
        bossAnimator.SetBool("isClean", isBossMet);
        rippleAnimator.SetBool("isClean", isStoryCleared);
    }

    public void Unlock(bool state)
    {
        isUnlocked = state;
        regionCollider.enabled = !state;
        print(bossEnum.ToString() + " Story Point : " + PlayerPrefs.GetInt("storyPoint", 0));
        if (isUnlocked && PlayerPrefs.GetInt("storyPoint", 0) / 2 >= regionOrder)
        {
            SpawnMinigames();
        }
        else
        {
            foreach (Transform minigame in transform.Find("Minigames"))
            {
                floatingObjectScript.objectsToRemove.Add(minigame.gameObject);
            }
        }
    }

    private void SpawnMinigames()
    {
        if (transform.Find("Minigames").childCount == 0)
        {
            minigameSpawnPoints.Clear();
            foreach (Transform spawnPoint in transform.Find("MinigameSpawns"))
            {
                minigameSpawnPoints.Add(spawnPoint);
            }
            for (int i = 0; i < minigameSpawnCount; i++)
            {
                int j = Random.Range(0, minigameSpawnPoints.Count);
                MinigameMarker newMarker = Instantiate(minigameMarker, minigameSpawnPoints[j].position, Quaternion.identity, transform.Find("Minigames")).GetComponent<MinigameMarker>();
                newMarker.trashType = trashType.ToString();
                floatingObjectScript.objectsToAdd.Add(newMarker.gameObject);
                minigameSpawnPoints.RemoveAt(j);
                minigameSpawnPoints.TrimExcess();
            }
        }
    }

    public void RefreshSprites()
    {
        isStoryCleared = PlayerPrefs.GetInt("maxRegion", 0) >= regionOrder;
        isBossMet = PlayerPrefs.GetInt("storyPoint", 0) / 2 >= regionOrder;
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