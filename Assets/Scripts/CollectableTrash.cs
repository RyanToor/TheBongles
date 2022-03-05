using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableTrash : MonoBehaviour
{
    [HideInInspector]
    public TrashType type;
    [HideInInspector]
    public string trashName;
    public TrashTypeSprites[] sprites;

    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>().isTrashRandom)
        {
            GetComponent<SpriteRenderer>().sprite = sprites[1].sprites[Random.Range(0, sprites[1].sprites.Length)].sprite;
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

    // Update is called once per frame
    void Update()
    {
        
    }
}

[System.Serializable]
public struct TrashTypeSprites
{
    public TrashType type;
    public NameSprite[] sprites;
}