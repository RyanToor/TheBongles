using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public SpriteRenderer[] spriteRenderers;
    public int maxHealth;
    public float chasmTopperForce, moveForce, maxSpeed, airControlMultiplier, waterDrag, airDrag, knockbackForce, maxBagDistance, damagePulseCount, damagePulseSpeed;
    public GameObject splash, twin;
    public GameObject gameOver;
    public GameObject pauseMenu;
    public GameObject plastic;
    public float tailSegmentLength = 0.1f, tailWidth = 0.1f, tailGravity = 9.8f, underwaterTailGravity = 0.1f, jointLerpSpeed;

    [HideInInspector]
    public int collectedPlastic;
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
    private float[] dryTime, jointLerpSpeeds;
    private Transform tailOffset;
    private bool isFlipping;
    private Vector3 correctedTailOffset;
    //private Vector3 twinPrevPos;

    // Start is called before the first frame update
    void Start()
    {
        tailOffset = transform.Find("TailOffset");
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
            if (PlayerPrefs.GetInt("upgrade0" + i, 0) == 1)
            {
                if (transform.Find("Upgrade" + (i + 1)) != null)
                {
                    transform.Find("Upgrade" + (i + 1)).gameObject.SetActive(true);
                    Upgrade(i + 1);
                }
            }
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Application.isEditor)
        {
            EditorUpdate();
        }
        float moveUp = 0;
        if (isloaded)
        {
            CheckPhysics();
            moveRight = Input.GetAxis("Horizontal");
            moveUp = Input.GetAxis("Vertical");
            if (moveRight < 0)
            {
                flipDir = -1;
            }
            else if (moveRight > 0)
            {
                flipDir = 1;
            }
            moveDir = new Vector3(moveRight, moveUp).normalized;
            rb.AddForce(rb.mass * moveDir * moveForce * controlMultiplier * Time.deltaTime, ForceMode2D.Force);
            rb.velocity = rb.velocity.normalized * Mathf.Clamp(rb.velocity.magnitude, 0, maxSpeed);
            Animate();
            UpdateTail();
        }
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
    }

    public void Flip()
    {
        spriteRenderer.flipX = (flipDir != 1);
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
        jointLerpSpeeds = new float[joints.Length];
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
        joints[0] = transform.position + transform.TransformVector(Vector3.Scale(tailOffset.localPosition, new Vector3(flipDir, 1, 1)));
        lerpJoints[0] = joints[0];
        for (int i = 1; i < joints.Length; i++)
        {
            joints[i] += (tailSegmentLength - (joints[i] - joints[i - 1]).magnitude) * (joints[i] - joints[i - 1]).normalized;
            lerpJoints[i] = Vector3.Lerp(lerpJoints[i], joints[i], Mathf.Lerp(100, jointLerpSpeed, (float)i / jointLerpSpeeds.Length) * Time.deltaTime);
        }
        lineRenderer.SetPositions(lerpJoints);
        twin.transform.position = lerpJoints[lerpJoints.Length - 1];
        twin.transform.rotation = /*Quaternion.Slerp(Quaternion.identity, */Quaternion.LookRotation(Vector3.forward, lerpJoints[lerpJoints.Length - 1] - lerpJoints[lerpJoints.Length - 2])/*, (lerpJoints[lerpJoints.Length - 1] - twinPrevPos).magnitude / 0.01f)*/;
    }

    private void Upgrade(int upgradeNumber)
    {
        switch (upgradeNumber)
        {
            case 1:
                health += 2;
                break;
            case 2:
                health += 1;
                break;
            default:
                break;
        }
    }

    private void RemoveArmour(int armourIndex)
    {
        if (transform.Find("Upgrade" + (armourIndex)) != null)
        {
            transform.Find("Upgrade" + (armourIndex)).gameObject.SetActive(false);
        }
    }

    IEnumerator DamagePulse()
    {
        float duration = 0;
        while (duration < damagePulseCount * 2 * Mathf.PI)
        {
            spriteRenderer.color = Color.Lerp(Color.white, Color.red, (Mathf.Sin(duration) + 0.5f) / 2);
            duration += Time.deltaTime * damagePulseSpeed;
            yield return null;
        }
        spriteRenderer.color = Color.white;
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
                    RemoveArmour(2);
                    if (health < maxHealth + 1)
                    {
                        RemoveArmour(1);
                    }
                }
                audioManager.PlaySFXAtLocation("Hit1", collision.transform.position);
                rb.AddForce((transform.position - collision.transform.position).normalized * knockbackForce, ForceMode2D.Impulse);
                trashManager.objectsToRemove.Add(new Unity.Mathematics.int2(chunkIndex, objectIndex));
                StopAllCoroutines();
                StartCoroutine(DamagePulse());
            }
        }
        if (collision.CompareTag("Region"))
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
            PlayerPrefs.SetInt("storyPoint", 2);
            rb.bodyType = RigidbodyType2D.Static;
            GetComponent<Collider2D>().enabled = false;
            twin.GetComponent<Collider2D>().enabled = false;
            isloaded = false;
            pauseMenu.SetActive(false);
            plastic.SetActive(false);
            SceneManager.LoadScene("Map");
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
            collectedPlastic += 20;
        }
    }

    private void OnDestroy()
    {
        int plastic = PlayerPrefs.GetInt("Plastic", 0);
        plastic += collectedPlastic;
        PlayerPrefs.SetInt("Plastic", plastic);
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
        gameOver.transform.Find("EndScreen/Score/Plastic").GetComponent<Text>().text = collectedPlastic.ToString();
        gameOver.transform.Find("EndScreen/Score/Depth").GetComponent<Text>().text = Mathf.Abs(Mathf.Ceil(transform.position.y)).ToString();
        uI.EndGame();
    }
}