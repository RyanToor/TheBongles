using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public SpriteRenderer[] spriteRenderers;
    public int maxHealth;
    public float chasmTopperForce, moveForce, maxSpeed, airControlMultiplier, waterDrag, airDrag, knockbackForce, maxBagDistance, damagePulseCount, damagePulseSpeed;
    public GameObject splash, twin, gameOver, pauseMenu, plastic, endFade;
    public float tailSegmentLength = 0.1f, tailWidth = 0.1f, tailGravity = 9.8f, underwaterTailGravity = 0.1f, minJointLerp, endSequenceLength, endSequenceShakeLength, endSequenceShakeMagnitute, spinStarsDuration;

    [HideInInspector]
    public float gravity, health = 3;
    [HideInInspector]
    public bool isloaded;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float controlMultiplier, moveRight = 1, flipDir = 1, spriteDir = 1;
    private Vector2 moveDir;
    private Animator animator, twinAnimator;
    private TrashManager trashManager;
    private AudioManager audioManager;
    private VerticalScrollerUI uI;
    private Vector3[] joints, lerpJoints;
    private LineRenderer lineRenderer;
    private bool[] dry;
    private float[] dryTime;
    private Transform tailOffset, spinStarsOffset;
    private bool isFlipping;
    private GameObject spinStars;
    //private Vector3 twinPrevPos;

    // Start is called before the first frame update
    void Start()
    {
        spinStars = transform.Find("SpinStars").gameObject;
        spinStars.SetActive(false);
        tailOffset = transform.Find("TailOffset");
        spinStarsOffset = transform.Find("SpinStarsOffset");
        uI = GameObject.Find("Canvas").GetComponent<VerticalScrollerUI>();
        audioManager = GameObject.Find("SoundManager").GetComponent<AudioManager>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        gravity = rb.gravityScale;
        animator = GetComponent<Animator>();
        twinAnimator = GameObject.Find("Twin").GetComponent<Animator>();
        trashManager = GameObject.Find("TrashContainer").GetComponent<TrashManager>();
        rb.gravityScale = 0;
        InitialiseTail();
        health = maxHealth;
        for (int i = 0; i < 3; i++)
        {
            if (GameManager.Instance.upgrades[0][i] > 0)
            {
                if (transform.Find("Upgrade" + (i + 1) + "-" + GameManager.Instance.upgrades[0][i]) != null)
                {
                    transform.Find("Upgrade" + (i + 1) + "-" + GameManager.Instance.upgrades[0][i]).gameObject.SetActive(true);
                    Upgrade(new Vector2Int(i + 1, GameManager.Instance.upgrades[0][i]));
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (isloaded)
        {
            CheckPhysics();
            moveRight = Input.GetAxis("Horizontal");
            float moveUp = Input.GetAxis("Vertical");
            if (moveRight < 0)
            {
                flipDir = -1;
            }
            else if (moveRight > 0)
            {
                flipDir = 1;
            }
            moveDir = new Vector3(moveRight, moveUp).normalized;
            rb.AddForce(controlMultiplier * moveForce * rb.mass * Time.deltaTime * moveDir, ForceMode2D.Force);
            rb.velocity = rb.velocity.normalized * Mathf.Clamp(rb.velocity.magnitude, 0, maxSpeed);
        }
        UpdateTail();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Application.isEditor)
        {
            EditorUpdate();
        }
        Animate();
    }

    private void CheckPhysics()
    {
        if (transform.position.y > 0)
        {
            rb.gravityScale = gravity;
            rb.drag = 0.2f;
        }
        else
        {
            rb.gravityScale = 0;
            rb.drag = 1;
        }
        if (transform.position.y < 0)
        {
            controlMultiplier = 1;
            rb.drag = waterDrag;
        }
        else
        {
            controlMultiplier = airControlMultiplier;
            rb.drag = airDrag;
        }
    }

    private void Animate()
    {
        transform.rotation = Quaternion.SlerpUnclamped(Quaternion.identity, Quaternion.AngleAxis(90, Vector3.forward), spriteDir * rb.velocity.y / maxSpeed);
        animator.SetFloat("Angle", Mathf.Abs((transform.rotation.eulerAngles.z > 180) ? 360 - transform.rotation.eulerAngles.z : transform.rotation.eulerAngles.z));
        animator.SetFloat("Speed", rb.velocity.magnitude);
        twinAnimator.SetFloat("Speed", rb.velocity.magnitude);
        float currentSpeed = Mathf.Clamp(rb.velocity.magnitude / maxSpeed, 0.5f, 1);
        animator.speed = currentSpeed;
        twinAnimator.speed = currentSpeed;
        if (spriteRenderer.flipX != (flipDir != 1) && !isFlipping)
        {
            animator.SetTrigger("Flip");
            isFlipping = true;
        }
        spinStars.transform.localPosition = Vector3.Scale(spinStarsOffset.localPosition, new Vector3(flipDir, 1, 1));
    }

    public void Flip()
    {
        spriteRenderer.flipX = (flipDir != 1);
        spinStars.GetComponent<SpriteRenderer>().flipX = (flipDir != 1);
        foreach (SpriteRenderer upgradeSpriteRenderer in spriteRenderers)
        {
            upgradeSpriteRenderer.flipX = spriteRenderer.flipX;
        }
        if (spriteRenderer.flipX)
        {
            spriteDir = -1;
        }
        else
        {
            spriteDir = 1;
        }
        isFlipping = false;
    }

    private void InitialiseTail()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        joints = new Vector3[Mathf.CeilToInt((twin.transform.position - transform.position).magnitude / tailSegmentLength)];
        joints[0] = transform.position + (tailOffset.position - transform.position) * spriteDir;
        dry = new bool[joints.Length];
        dryTime = new float[joints.Length];
        lineRenderer.positionCount = joints.Length;
        for (int i = 1; i < joints.Length; i++)
        {
            joints[i] = transform.position + (twin.transform.position - transform.position).normalized * (i * tailSegmentLength);
        }
        lineRenderer.SetPositions(joints);
        lineRenderer.startWidth = tailWidth;
        lineRenderer.endWidth = tailWidth;
        lerpJoints = new Vector3[joints.Length];
    }

    private void UpdateTail()
    {
        for (int i = 1; i < joints.Length; i++)
        {
            if (joints[i].y > 0)
            {
                if (!dry[i])
                {
                    dry[i] = true;
                    dryTime[i] = Time.time;
                }
            }
            else
            {
                if (dry[i])
                {
                    dry[i] = false;
                }
            }
        }
        for (int i = 1; i < joints.Length; i++)
        {
            if (dry[i])
            {
                joints[i].y -= tailGravity * (Time.time - dryTime[i]) * Time.deltaTime;
            }
            else
            {
                joints[i].y -= underwaterTailGravity * Time.deltaTime;
            }
        }
        //twinPrevPos = lerpJoints[lerpJoints.Length - 1];
        joints[0] = transform.position + transform.TransformVector(Vector3.Scale(tailOffset.localPosition, new Vector3((spriteRenderer.flipX == true) ? -1 : 1, 1, 1)));
        lerpJoints[0] = joints[0];
        for (int i = 1; i < joints.Length; i++)
        {
            joints[i] += (tailSegmentLength - (joints[i] - joints[i - 1]).magnitude) * (joints[i] - joints[i - 1]).normalized;
            lerpJoints[i] = Vector3.Lerp(lerpJoints[i], joints[i], minJointLerp * (joints.Length - i) * Time.deltaTime);
        }
        lineRenderer.SetPositions(lerpJoints);
        twin.transform.position = lerpJoints[lerpJoints.Length - 1];
        twin.transform.rotation = /*Quaternion.Slerp(Quaternion.identity, */Quaternion.LookRotation(Vector3.forward, lerpJoints[lerpJoints.Length - 1] - lerpJoints[lerpJoints.Length - 2])/*, (lerpJoints[lerpJoints.Length - 1] - twinPrevPos).magnitude / 0.01f)*/;
    }

    private void Upgrade(Vector2Int upgradeNumber)
    {
        switch (upgradeNumber.x)
        {
            case 1:
                if (upgradeNumber.y == 1)
                {
                    health += 2;
                    break;
                }
                else if (upgradeNumber.y == 2)
                {
                    health += 1;
                    break;
                }
                else
                {
                    break;
                }
            case 2:
                health += 1;
                break;
            default:
                break;
        }
    }

    private void RemoveArmour(string armourIndex)
    {
        if (transform.Find("Upgrade" + armourIndex) != null)
        {
            transform.Find("Upgrade" + armourIndex).gameObject.SetActive(false);
        }
    }

    IEnumerator DamagePulse()
    {
        float duration = 0;
        spinStars.SetActive(true);
        while (duration < damagePulseCount * 2 * Mathf.PI / damagePulseSpeed)
        {
            spriteRenderer.color = Color.Lerp(Color.white, Color.red, (Mathf.Sin(duration * damagePulseSpeed) + 0.5f) / 2);
            duration += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = Color.white;
        while (duration < spinStarsDuration)
        {
            duration += Time.deltaTime;
            yield return null;
        }
        if (health > 0)
        {
            spinStars.SetActive(false);
        }
    }

    IEnumerator EndSequence()
    {
        Vector3 cameraStartPosition = Camera.main.transform.position;
        GameObject eel = GameObject.Find("Eel/EelHead");
        Vector3 eelStartPos = eel.transform.localPosition;
        float t = 0;
        while (t < endSequenceLength)
        {
            Camera.main.transform.position = cameraStartPosition + Mathf.Sin(t * Mathf.PI/ endSequenceShakeLength) * endSequenceShakeMagnitute * Vector3.down;
            endFade.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, t / endSequenceLength);
            eel.transform.localPosition = Vector3.Lerp(eelStartPos, -eelStartPos, t / endSequenceLength);
            t += Time.deltaTime;
            yield return null;
        }
        SceneManager.LoadScene("Map");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("RandomTrash"))
        {
            int chunkIndex = Mathf.FloorToInt(-collision.transform.position.y / trashManager.chunkHeight);
            int objectIndex = 0;
            for (int i = 0; i < trashManager.currentTrash[chunkIndex].Count; i++)
            {
                if (trashManager.currentTrash[chunkIndex][i] == collision.gameObject)
                {
                    objectIndex = i;
                    break;
                }
            }
            Trash trashInfo = trashManager.loadedTrash[chunkIndex][objectIndex];
            if (trashInfo.isDangerous)
            {
                health--;
                if (health <= 0)
                {
                    Dead();
                }
                if (health < maxHealth + 3)
                {
                    RemoveArmour("1-2");
                    if (health < maxHealth + 1)
                    {
                        RemoveArmour("1-1");
                    }
                }
                audioManager.PlaySFXAtLocation("Hit1", collision.transform.position);
                rb.AddForce((transform.position - collision.transform.position).normalized * knockbackForce, ForceMode2D.Impulse);
                trashManager.objectsToRemove.Add(new Unity.Mathematics.int2(chunkIndex, objectIndex));
                StopAllCoroutines();
                StartCoroutine(DamagePulse());
                animator.SetTrigger("Hit");
            }
        }
        else if (collision.CompareTag("Region"))
        {
            Instantiate(splash, transform.position + Vector3.up * 1.5f, Quaternion.identity);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Region"))
        {
            Instantiate(splash, new Vector3(transform.position.x, 0.75f, 0), Quaternion.identity);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Boss"))
        {
            GameManager.Instance.storyPoint = 2;
            rb.bodyType = RigidbodyType2D.Static;
            GetComponent<Collider2D>().enabled = false;
            isloaded = false;
            pauseMenu.SetActive(false);
            plastic.SetActive(false);
            StartCoroutine(EndSequence());
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.name == "ChasmTopper_1" || collision.gameObject.name == "ChasmTopper_2")
        {
            rb.AddForce(Vector3.right * ((collision.gameObject.name == "ChasmTopper_1") ? 1 : -1) * chasmTopperForce);
        }
    }

    private void EditorUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Map");
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            health--;
            if (health <= 0)
            {
                Dead();
            }
            StopAllCoroutines();
            StartCoroutine(DamagePulse());
        }
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_VerticalScroller>().plastic++;
        }
    }

    private void Dead()
    {
        rb.bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;
        twin.GetComponent<Collider2D>().enabled = false;
        audioManager.PlaySFX("GameOver");
        isloaded = false;
        gameOver.SetActive(true);
        pauseMenu.SetActive(false);
        plastic.SetActive(false);
        uI.EndGame();
    }
}