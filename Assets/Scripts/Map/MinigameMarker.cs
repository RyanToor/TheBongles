using System.Collections.Generic;
using UnityEngine;

public class MinigameMarker : MonoBehaviour
{
    public List<NameSprite> sprites;
    public TrashType trashType;

    // Start is called before the first frame update
    void Start()
    {
        foreach (NameSprite minigameMarkerSprite in sprites)
        {
            if (minigameMarkerSprite.name == trashType.ToString())
            {
                transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = minigameMarkerSprite.sprite;
                break;
            }
        }
    }
}
