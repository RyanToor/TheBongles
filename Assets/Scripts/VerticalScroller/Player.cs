using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public int health;
    public float moveForce, maxSpeed, airControlMultiplier, waterDrag, airDrag, knockbackForce, maxBagDistance, damagePulseCount, damagePulseSpeed;
    public GameObject splash, bag;
    public GameObject gameOver;
    public GameObject pauseMenu;
    public GameObject plastic;

    [HideInInspector]
    public int collectedPlastic;
    [HideInInspector]
    public float gravity;
    [HideInInspector]
    public bool isloaded;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float controlMultiplier, moveRight = 1, prevFlipDir = 1, flipDir = 1, spriteDir = 1;
    private Vector2 moveDir;
    private Animator animator, bagAnimator;
    private TrashManager trashManager;
    private AudioManager audioManager;
    private VerticalScrollerUI uI;
    // Start is called before the first frame update
    void Start()
    {
        uI = GameObject.Find("Canvas").GetComponent<VerticalScrollerUI>();
        audioManager = GameObject.Find("SoundManager").GetComponent<AudioManager>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        gravity = rb.gravityScale;
        animator = GetComponent<Animator>();
        bagAnimator = GameObject.Find("Bag").GetComponent<Animator>();
        trashManager = GameObject.Find("TrashContainer").GetComponent<TrashManager>();
        rb.gravityScale = 0;
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
            if (moveRight < 0)
            {
                flipDir = -1;
            }
            else if (moveRight > 0)
            {
                flipDir = 1;
            }
            moveUp = Input.GetAxis("Vertical");
        }
        moveDir = new Vector3(moveRight, moveUp).normalized;
        rb.AddForce(rb.mass * moveDir * moveForce * controlMultiplier * Time.deltaTime, ForceMode2D.Force);
        rb.velocity = rb.velocity.normalized * Mathf.Clamp(rb.velocity.magnitude, 0, maxSpeed);
        Animate();
        MoveBag();
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

    private void MoveBag()
    {
        if ((bag.transform.position - transform.position).magnitude != maxBagDistance)
        {
            Vector3 bagOffset = (bag.transform.position - transform.position).normalized * maxBagDistance;
            bag.transform.position = transform.position + bagOffset;
            bag.transform.rotation = Quaternion.LookRotation(Vector3.forward, bagOffset);
        }
    }

    private void Animate()
    {
        transform.rotation = Quaternion.LerpUnclamped(Quaternion.identity, Quaternion.AngleAxis(90, Vector3.forward), spriteDir * rb.velocity.y / maxSpeed);
        animator.SetFloat("Speed", rb.velocity.magnitude);
        float currentSpeed = Mathf.Clamp(rb.velocity.magnitude / maxSpeed, 0.5f, 1);
        animator.speed = currentSpeed;
        bagAnimator.speed = currentSpeed;
        if (prevFlipDir != flipDir)
        {
            animator.SetTrigger("Flip");
        }
        prevFlipDir = flipDir;
    }

    public void Flip()
    {
        spriteRenderer.flipX = !spriteRenderer.flipX;
        if (spriteRenderer.flipX)
        {
            spriteDir = -1;
        }
        else
        {
            spriteDir = 1;
        }
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

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Region"))
        {
            Instantiate(splash, new Vector3(transform.position.x, 0.75f, 0), Quaternion.identity);
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
        isloaded = false;
        gameOver.SetActive(true);
        pauseMenu.SetActive(false);
        plastic.SetActive(false);
        gameOver.transform.Find("EndScreen/Score/Plastic").GetComponent<Text>().text = collectedPlastic.ToString();
        gameOver.transform.Find("EndScreen/Score/Depth").GetComponent<Text>().text = Mathf.Abs(Mathf.Ceil(transform.position.y)).ToString();
        uI.EndGame();
    }
}