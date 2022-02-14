using System.Collections;
using UnityEngine;

public class ProximityElement : MonoBehaviour
{
    public ProximityElementType type;
    public float bubbleFrequency, speed;

    private bool randomChanceRunning = false, shyClosed = false, isFlipping = false;
    private Transform anchor1, anchor2;

    private void Awake()
    {
        if (type == ProximityElementType.turtle)
        {
            anchor1 = transform.parent.Find("Anchor1");
            anchor2 = transform.parent.Find("Anchor2");
        }
    }

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
                    StartCoroutine(Turtle());
                    break;
                case ProximityElementType.randomAnim:
                    StartCoroutine(RandomChance());
                    break;
                case ProximityElementType.shy:
                    Shy(true);
                    break;
                case ProximityElementType.mussel:
                    if (randomChanceRunning)
                    {
                        Shy(true);
                    }
                    else
                    {
                        StartCoroutine(RandomChance());
                    }
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
            if (shyClosed)
            {
                Shy(false);
            }
            else if (randomChanceRunning)
            {
                StopCoroutine(nameof(RandomChance));
                GetComponent<Animator>().SetFloat("RandomChance", 0);
                randomChanceRunning = false;
            }
            else
            {
                StopAllCoroutines();
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
            newBubble.floatSpeed = speed;
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
        randomChanceRunning = true;
        while (true)
        {
            GetComponent<Animator>().SetFloat("RandomChance", Random.Range(0, 100));
            yield return null;
        }
    }

    private void Shy(bool isNear)
    {
        GetComponent<Animator>().SetBool("Near", isNear);
        shyClosed = isNear;
    }

    private IEnumerator Turtle()
    {
        float pathLength = (anchor2.position - anchor1.position).magnitude;
        float lapDistance = Time.realtimeSinceStartup * speed % (2 * pathLength);
        bool isFirstLeg = lapDistance < pathLength;
        bool isPrevFrameFlipped = true;
        if (isFirstLeg ^ anchor1.position.x < anchor2.position.x)
        {
            isPrevFrameFlipped = false;
        }
        GetComponent<SpriteRenderer>().flipX = isPrevFrameFlipped;
        while (true)
        {
            lapDistance = Time.realtimeSinceStartup * speed % (2 * pathLength);
            isFirstLeg = lapDistance < pathLength;
            bool isFrameFlipped = !(isFirstLeg ^ anchor1.position.x < anchor2.position.x);
            if (isFrameFlipped != isPrevFrameFlipped && !isFlipping)
            {
                GetComponent<Animator>().SetTrigger("Flip");
                isFlipping = true;
            }
            isPrevFrameFlipped = isFrameFlipped;
            if (isFirstLeg)
            {
                transform.position = Vector2.Lerp(anchor1.position, anchor2.position, lapDistance / pathLength);
            }
            else
            {
                transform.position = Vector2.Lerp(anchor2.position, anchor1.position, (lapDistance - pathLength) / pathLength);
            }
            yield return new WaitForFixedUpdate();
        }
    }

    public void Flip()
    {
        GetComponent<SpriteRenderer>().flipX = !GetComponent<SpriteRenderer>().flipX;
        isFlipping = false;
    }

    [System.Serializable]
    public enum ProximityElementType
    {
        bubbleBlaster,
        turtle,
        randomAnim,
        shy,
        mussel
    }
}
