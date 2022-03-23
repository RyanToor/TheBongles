using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityElement : MonoBehaviour
{
    public ProximityElementType type;
    public float bubbleFrequency, speed, birdMaxAngle, birdKnockbeck, birdSpeed, anchorLerpSpeed, anchorTopOffset;

    [HideInInspector]
    public Vector3 anchorPoint, blockerPoint;

    private bool randomChanceRunning = false, shyClosed = false, isFlipping = false, hit = false;
    private Transform anchor1, anchor2;
    private Vector2 anchorLineStart;
    private Sprite anchor, anchorSeabed;

    private void Awake()
    {
        if (type == ProximityElementType.turtle)
        {
            anchor1 = transform.parent.Find("Anchor1");
            anchor2 = transform.parent.Find("Anchor2");
        }
        else if (type == ProximityElementType.anchor)
        {
            anchorLineStart = GetComponent<EdgeCollider2D>().points[0];
            GetComponent<LineRenderer>().SetPositions(new Vector3[] { anchorLineStart, transform.InverseTransformPoint(blockerPoint) });
            StartCoroutine(Anchor(GameObject.FindGameObjectWithTag("Player").transform));
            anchor = GameObject.Find("Level").GetComponent<LevelBuilder>().anchorSprites[0];
            anchorSeabed = GameObject.Find("Level").GetComponent<LevelBuilder>().anchorSprites[1];
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
                case ProximityElementType.bird:
                    if (!shyClosed)
                    {
                        Shy(true);
                        StartCoroutine(CheckBirdOffscreen());
                        AudioManager.instance.PlaySFXAtLocation("BirdFly", transform.position, 60);
                    }
                    else
                    {
                        hit = true;
                        AudioManager.instance.PlaySFXAtLocation("BirdDead", transform.position, 60);
                        if (Vector3.Angle(Vector3.up, collision.transform.position - transform.position) < birdMaxAngle)
                        {
                            collision.GetComponent<Rigidbody2D>().velocity = Vector3.Reflect(collision.GetComponent<Rigidbody2D>().velocity, Vector3.up);
                        }
                        else
                        {
                            collision.GetComponent<Rigidbody2D>().AddForce(-Mathf.Sign(collision.GetComponent<Rigidbody2D>().velocity.x) * birdKnockbeck * Vector2.right, ForceMode2D.Impulse);
                        }
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
            transform.position = Vector2.Lerp(anchor1.position, anchor2.position, (isFirstLeg? lapDistance : (2 * pathLength - lapDistance)) / pathLength);
            yield return new WaitForFixedUpdate();
        }
    }

    public IEnumerator BirdFly()
    {
        float angle = Random.Range(20, 70);
        GameObject robot = GameObject.FindGameObjectWithTag("Player");
        while (!hit)
        {
            Vector3 offset = transform.position - robot.transform.position;
            float flightMagnitude = Mathf.Sign(offset.x) * birdSpeed * Time.deltaTime;
            transform.position += new Vector3(flightMagnitude * Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Abs(flightMagnitude) * Mathf.Sin(Mathf.Deg2Rad * angle));
            GetComponent<SpriteRenderer>().flipX = offset.x > 0;
            yield return null;
        }
        GetComponent<Animator>().SetTrigger("Hit");
    }

    public IEnumerator CheckBirdOffscreen()
    {
        while (true)
        {
            if ((transform.position - GameObject.FindGameObjectWithTag("Player").transform.position).magnitude > 40)
            {
                Destroy(gameObject);
                break;
            }
            else
            {
                yield return null;
            }
        }
    }

    public void BirdFall()
    {
        gameObject.AddComponent<Rigidbody2D>();
    }

    public void Flip()
    {
        GetComponent<SpriteRenderer>().flipX = !GetComponent<SpriteRenderer>().flipX;
        isFlipping = false;
    }

    private IEnumerator Anchor(Transform player)
    {
        while (true)
        {
            Vector3 desiredPosition = player.position.x < transform.position.x ? anchorPoint : blockerPoint + Vector3.down * anchorTopOffset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, anchorLerpSpeed * Time.deltaTime);
            GetComponent<LineRenderer>().SetPositions(new Vector3[] { anchorLineStart, transform.InverseTransformPoint(blockerPoint) });
            GetComponent<EdgeCollider2D>().SetPoints(new List<Vector2> { anchorLineStart, transform.InverseTransformPoint(blockerPoint) });
            GetComponent<SpriteRenderer>().sprite = (Vector3.Magnitude(transform.position - anchorPoint) < 0.1f) ? anchorSeabed : anchor;
            yield return null;
        }
    }

    [System.Serializable]
    public enum ProximityElementType
    {
        bubbleBlaster,
        turtle,
        randomAnim,
        shy,
        mussel,
        bird,
        anchor
    }
}
