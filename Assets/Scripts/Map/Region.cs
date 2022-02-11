using System.Collections.Generic;
using UnityEngine;

public class Region : MonoBehaviour
{
    public bool isUnlocked;
    public int minigameSpawnCount, regionOrder;
    public GameObject minigameMarker, cloudScreen;
    public TrashType trashType;
    public bossTypes bossEnum;
    public int[] storyMeetPoints;
    public List<NameController> bossControllers = new List<NameController>();

    private Collider2D regionCollider;
    private List<Transform> minigameSpawnPoints = new List<Transform>();
    private bool isStoryCleared, isBossMet;
    private string boss;
    private FloatingObjects floatingObjectScript;
    private Animator bossAnimator, rippleAnimator;

    private void Awake()
    {
        boss = bossEnum.ToString();
        floatingObjectScript = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<FloatingObjects>();
        int minigameSpawnsCount = transform.Find("MinigameSpawns").childCount;
        if (minigameSpawnCount > minigameSpawnsCount)
        {
            minigameSpawnCount = minigameSpawnsCount;
        }
        regionCollider = GetComponent<Collider2D>();
        foreach (NameController bossType in bossControllers)
        {
            if (bossType.name == bossEnum.ToString())
            {
                bossAnimator = transform.Find("BossIsland/SpriteBoss").GetComponent<Animator>();
                rippleAnimator = transform.Find("BossIsland/SpriteRipples").GetComponent<Animator>();
                bossAnimator.runtimeAnimatorController = bossType.bossController;
                rippleAnimator.runtimeAnimatorController = bossType.rippleController;
            }
        }        
    }

    private void Start()
    {
        RefreshSprites();
    }

    private void Update()
    {
        bossAnimator.SetFloat("RandomChance", Random.Range(0f, 100f));
        if (Application.isEditor)
        {
            EditorUpdate();
        }
        bossAnimator.SetBool("isMet", isBossMet);
        bossAnimator.SetBool("isClean", isStoryCleared);
        rippleAnimator.SetBool("isClean", isStoryCleared);
    }

    public void Unlock(bool isUnlocked)
    {
        regionCollider.enabled = !isUnlocked;
        if (isUnlocked && GameManager.Instance.storyPoint > GameManager.Instance.regionStoryPoints[regionOrder] && GameManager.Instance.storyPoint != 1)
        {
            SpawnMinigames();
            if (transform.Find("BossIsland/CloudScreen") != null && GameManager.Instance.storyPoint > storyMeetPoints[regionOrder] + 1)
            {
                transform.Find("BossIsland/CloudScreen").gameObject.SetActive(false);
            }
        }
        else
        {
            foreach (Transform minigame in transform.Find("Minigames"))
            {
                floatingObjectScript.objectsToRemove.Add(minigame.gameObject);
            }
        }
        RefreshSprites();
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
                newMarker.trashType = trashType;
                floatingObjectScript.objectsToAdd.Add(newMarker.gameObject);
                minigameSpawnPoints.RemoveAt(j);
                minigameSpawnPoints.TrimExcess();
            }
        }
    }

    public void RefreshSprites()
    {
        isStoryCleared = GameManager.Instance.storyPoint >= GameManager.Instance.regionStoryPoints[Mathf.Clamp(regionOrder, 0, int.MaxValue)];
        isBossMet = GameManager.Instance.storyPoint >= storyMeetPoints[(int)bossEnum];
        bossAnimator.Rebind();
        bossAnimator.Update(0f);
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

    public enum bossTypes
    {
        Eel,
        Crab,
        Whale
    }
}