using UnityEngine;

public class Map : MonoBehaviour
{
    public NameSpriteArray[] trashSprites;
    public int randomTrashAmount;
    public GameObject randomTrashContainer, randomTrash;
    public float trashClearBorder;

    Rect mapArea;
    private FloatingObjects floatingObjectsScript;

    // Start is called before the first frame update
    void Start()
    {
        floatingObjectsScript = GetComponent<FloatingObjects>();
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        mapArea = new Rect(sprite.bounds.min + Vector3.one * trashClearBorder, sprite.bounds.size - Vector3.one * trashClearBorder * 2);
        UpdateRegionsUnlocked(PlayerPrefs.GetInt("maxRegion", 1));
        RespawnTrash();
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
        foreach (Transform randomTrash in randomTrashContainer.transform)
        {
            Destroy(randomTrash.gameObject);
        }
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

    private void UpdateRegionsUnlocked(int region)
    {
        if (region > PlayerPrefs.GetInt("maxRegion", 1))
        {
            PlayerPrefs.SetInt("maxRegion", region);
        }
    }

    private void EditorOnly()
    {
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Minus))
        {
            if (Input.GetKeyDown(KeyCode.Equals))
            {
                PlayerPrefs.SetInt("maxRegion", Mathf.Clamp(PlayerPrefs.GetInt("maxRegion", 1) + 1, 1, 3));
            }
            else if (Input.GetKeyDown(KeyCode.Minus))
            {
                PlayerPrefs.SetInt("maxRegion", Mathf.Clamp(PlayerPrefs.GetInt("maxRegion", 1) - 1, 1, 3));
            }
            print(PlayerPrefs.GetInt("maxRegion", 1));
            floatingObjectsScript.RemoveAll();
            RespawnTrash();
        }
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