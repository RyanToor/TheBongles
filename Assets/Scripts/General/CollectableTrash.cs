using UnityEngine;

public class CollectableTrash : MonoBehaviour
{
    public bool allowRandomisation = true;
    [HideInInspector]
    public TrashType type;
    [HideInInspector]
    public string trashName;
    public TrashTypeSprites[] sprites;

    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>().isTrashRandom && allowRandomisation)
        {
            TrashType levelTrashType = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>().levelTrashType;
            int trashIndex = Random.Range(0, sprites[(int)levelTrashType].sprites.Length);
            GetComponent<SpriteRenderer>().sprite = sprites[(int)levelTrashType].sprites[trashIndex].sprite;
            trashName = sprites[(int)levelTrashType].sprites[trashIndex].name;
        }
        else
        {
            foreach (NameSprite nameSprite in sprites[(int)type].sprites)
            {
                if (nameSprite.name == trashName)
                {
                    GetComponent<SpriteRenderer>().sprite = nameSprite.sprite;
                }
            }
        }
        PolygonCollider2D newCollider = gameObject.AddComponent<PolygonCollider2D>();
        newCollider.isTrigger = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>().isTrashTrigger;
    }
}

[System.Serializable]
public struct TrashTypeSprites
{
    public TrashType type;
    public NameSprite[] sprites;
}