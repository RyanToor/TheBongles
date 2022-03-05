using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glass : MonoBehaviour
{
    public GlassType type;
    public Sprite[] sprites;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = sprites[(int)type];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public enum GlassType
{
    bottle,
    brokenBottle,
    jar,
    bulb
}
