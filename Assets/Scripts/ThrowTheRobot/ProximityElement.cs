using System.Collections;
using UnityEngine;

public class ProximityElement : MonoBehaviour
{
    public ProximityElementType type;
    public float bubbleFrequency, bubbleSpeed;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            switch (type)
            {
                case ProximityElementType.bubbleBlaster:
                    StartCoroutine(BlastBubbles());
                    break;
                case ProximityElementType.turtle:
                    break;
                default:
                    break;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StopAllCoroutines();
        }
    }

    private IEnumerator BlastBubbles()
    {
        GameObject bubblePrefab = GameObject.Find("Level").GetComponent<LevelBuilder>().bubblePrefab;
        float duration = 0;
        while (true)
        {
            Bubble newBubble = Instantiate(bubblePrefab, transform.position, Quaternion.identity, GameObject.Find("Bubbles").transform).GetComponent<Bubble>();
            newBubble.floatSpeed = bubbleSpeed;
            while (duration < bubbleFrequency)
            {
                duration += Time.deltaTime;
                yield return null;
            }
            duration = 0;
        }
    }

    [System.Serializable]
    public enum ProximityElementType
    {
        bubbleBlaster,
        turtle
    }
}
