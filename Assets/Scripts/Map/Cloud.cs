using System.Collections;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    public float fadeSpeed;
    public Sprite[] cloudSprites;
    public SpriteRenderer sprite;
    public float currentAlpha;
    public bool originalCloud = true;

    private int currentColliders;

    // Start is called before the first frame update
    void Start()
    {
        if (originalCloud)
        {
            int randInt = Random.Range(0, cloudSprites.Length);
            sprite.sprite = cloudSprites[randInt];
        }
    }

    //Add new collider to the count of colliders and fade the cloud in if this is it's first collider.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        currentColliders++;
        if (currentColliders == 1 && originalCloud)
        {
            StopAllCoroutines();
            StartCoroutine(Fade(1));
        }
    }

    //Ramove collider from the count and fade it out if there are none left.
    private void OnTriggerExit2D(Collider2D collision)
    {
        Mathf.Clamp(currentColliders--, 0, Mathf.Infinity);
        if (currentColliders == 0 && originalCloud)
        {
            StopAllCoroutines();
            StartCoroutine(Fade(-1));
        }
    }

    //Increment the sprite's alpha until it reaches max opacity (true), or full transparency(false)
    public IEnumerator Fade(int fadeDir)
    {
        Color col = sprite.color;
        while (0 <= currentAlpha && currentAlpha <= 1)
        {
            currentAlpha = Mathf.Clamp01(currentAlpha + fadeDir * fadeSpeed * Time.deltaTime);
            col.a = currentAlpha;
            sprite.color = col;
            if (sprite.color.a == Mathf.Clamp01(fadeDir))
            {
                break;
            }
            yield return null;
        }
    }
}