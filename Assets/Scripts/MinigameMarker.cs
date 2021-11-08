using System.Collections.Generic;
using UnityEngine;

public class MinigameMarker : MonoBehaviour
{
    public List<NameSprite> sprites;
    public string trashType;

    // Start is called before the first frame update
    void Start()
    {
        foreach (NameSprite minigameMarkerSprite in sprites)
        {
            if (minigameMarkerSprite.name == trashType)
            {
                transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = minigameMarkerSprite.sprite;
                break;
            }
        }
    }
}
