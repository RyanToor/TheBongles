using System.Collections;
using UnityEngine;

public class ProximityElement : MonoBehaviour
{
    public ProximityElementType type;
    public float bubbleFrequency, bubbleSpeed;

    private bool randomChanceRunning = false, shyClosed = false;
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
                case ProximityElementType.randomAnim:
                    StartCoroutine(RandomChance());
                    break;
                case ProximityElementType.shy:
                    Shy(true);
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
            if (randomChanceRunning)
            {
                GetComponent<Animator>().SetFloat("RandomChance", 0);
                randomChanceRunning = false;
            }
            else if (shyClosed)
            {
                Shy(false);
                shyClosed = false;
            }
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

    private IEnumerator RandomChance()
    {
        while (true)
        {
            GetComponent<Animator>().SetFloat("RandomChance", Random.Range(0, 100));
            yield return null;
        }
    }

    private void Shy(bool isNear)
    {
        GetComponent<Animator>().SetBool("Near", isNear);
        shyClosed = true;
    }

    [System.Serializable]
    public enum ProximityElementType
    {
        bubbleBlaster,
        turtle,
        randomAnim,
        shy
    }
}
