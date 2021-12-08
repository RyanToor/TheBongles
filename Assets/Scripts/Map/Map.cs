using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Map : MonoBehaviour
{
    public NameSpriteArray[] trashSprites;
    public int randomTrashAmount;
    public GameObject randomTrashContainer, randomTrash;
    public float trashClearBorder;
    public List<GameObject> inGameUI;

    Rect mapArea;
    private FloatingObjects floatingObjectsScript;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt("isLoaded", 1) == 0)
        {
            GameObject.Find("CloudCover").SetActive(false);
            GameObject.Find("MainMenu").SetActive(false);
            GameObject.Find("BongleIsland").GetComponent<BongleIsland>().isInputEnabled = true;
        }
        else
        {
            foreach (GameObject uI in inGameUI)
            {
                uI.SetActive(false);
            }
        }
        floatingObjectsScript = GetComponent<FloatingObjects>();
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        mapArea = new Rect(sprite.bounds.min + Vector3.one * trashClearBorder, sprite.bounds.size - Vector3.one * trashClearBorder * 2);
        UpdateRegionsUnlocked(PlayerPrefs.GetInt("maxRegion", 1));
        RespawnTrash();
        Destroy(GameObject.Find("LoadingCanvas(Clone)"));
        Time.timeScale = 1;
        GameObject.Find("SoundManager").GetComponent<AudioManager>().PlayMusic("Map");
    }

    private void Update()
    {
        if (Application.isEditor)
        {
            EditorOnly();
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
        GameObject newTrash = Instantiate(randomTrash, new Vector3(Random.Range(mapArea.min.x, mapArea.max.x), Random.Range(mapArea.min.y, mapArea.max.y), 0), Quaternion.identity, randomTrashContainer.transform);
        int trashType = Random.Range(0, PlayerPrefs.GetInt("maxRegion", 0));
        RandomTrash trashScript = newTrash.GetComponent<RandomTrash>();
        trashScript.sprite = trashSprites[trashType].sprites[Random.Range(0, trashSprites[trashType].sprites.Length)];
        trashScript.trashType = trashSprites[trashType].name;
        trashScript.floatingObjectsScript = floatingObjectsScript;
        floatingObjectsScript.objectsToAdd.Add(newTrash);
    }

    public void UpdateRegionsUnlocked(int region)
    {
        print("Max Region : " + region);
        if (region > PlayerPrefs.GetInt("maxRegion", 1))
        {
            PlayerPrefs.SetInt("maxRegion", region);
            GameObject.Find("UI/Upgrades").GetComponent<UpgradeMenu>().RefreshReadouts();
        }
        foreach (Transform regionTransform in GameObject.Find("BossRegions").transform)
        {
            Region regionScript = regionTransform.gameObject.GetComponent<Region>();
            regionScript.Unlock(PlayerPrefs.GetInt("maxRegion", 1) >= regionScript.regionOrder);
        }
    }

    private void EditorOnly()
    {
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Minus))
        {
            int currentRegion = PlayerPrefs.GetInt("maxRegion", 1);
            if (Input.GetKeyDown(KeyCode.Equals))
            {
                PlayerPrefs.SetInt("maxRegion", Mathf.Clamp(currentRegion + 1, 1, 3));
            }
            else if (Input.GetKeyDown(KeyCode.Minus))
            {
                PlayerPrefs.SetInt("maxRegion", Mathf.Clamp(currentRegion - 1, 1, 3));
            }
            print("Max Region : " + PlayerPrefs.GetInt("maxRegion", 1));
            UpdateRegionsUnlocked(PlayerPrefs.GetInt("maxRegion", 1));
            GameObject.Find("UI/Upgrades").GetComponent<UpgradeMenu>().RefreshReadouts();
            RespawnTrash();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            PlayerPrefs.SetInt("StoryStarted", Mathf.Abs(PlayerPrefs.GetInt("StoryStarted", 1) - 1));
            if (PlayerPrefs.GetInt("StoryStarted", 0) == 1)
            {
                print("Story Started");
            }
            else
            {
                print("Story Not Started");
            }
        }
    }

    public void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("isLoaded", 1);
    }
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