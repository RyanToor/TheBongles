using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour
{
    public LayerMask jumpLayers;
    public float moveForce, jumpForce, airControlMultiplier, groundCastOffset, decelerationMultiplier, 
        boostForce, skimMinVelocity, skimVelocityMultiplier, waterHitVelocityMultiplier, musselForce, jellyfishBoost;
    [Range(0, 90)]
    public float skimMaxAngle, musselAngle, maxJellyfishAngle;
    public GameObject waterSurface, splashPrefab;

    [HideInInspector]
    public bool isLaunched;

    private Vector3 startPos;
    private Rigidbody2D rb;
    private LevelManager_Robot levelManager;
    private bool isGrounded, isJumping;
    private float colliderWidth, colliderHeight;
    private List<Animator> clouds = new List<Animator>();

    private void Awake()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Robot>();
        colliderWidth = GetComponent<CapsuleCollider2D>().bounds.extents.x;
        colliderHeight = GetComponent<CapsuleCollider2D>().bounds.extents.y;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
    }

    void FixedUpdate()
    {
        if (levelManager.isLaunched)
        {
            isGrounded = Physics2D.CircleCast((Vector2)transform.position + GetComponent<CapsuleCollider2D>().offset, colliderWidth, -transform.up, colliderHeight - colliderWidth + groundCastOffset);
            Debug.DrawLine((Vector2)transform.position + GetComponent<CapsuleCollider2D>().offset, (Vector2)transform.position + GetComponent<CapsuleCollider2D>().offset - (Vector2)transform.up * (colliderHeight + groundCastOffset), Color.red);
            rb.AddForce((isGrounded? 1 : airControlMultiplier) * Input.GetAxis("Horizontal") * moveForce * (Mathf.Sign(rb.velocity.x) != Mathf.Sign(Input.GetAxisRaw("Horizontal"))? decelerationMultiplier : 1) * Vector2.right, ForceMode2D.Force);
            if (isGrounded)
            {
                isJumping = false;
                rb.gravityScale = 1 - Mathf.Abs(Input.GetAxisRaw("Horizontal"));
                if (Input.GetAxisRaw("Jump") == 1 && !isJumping)
                {
                    rb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
                    isJumping = true;
                }
            }
            else if (clouds.Count > 0)
            {
                if (Input.GetAxisRaw("Jump") == 1)
                {
                    Boost();
                }
            }
            else
            {
                rb.gravityScale = 1;
            }
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rb.velocity.magnitude > 1f? Quaternion.Euler(0, 0, Mathf.Clamp(Vector2.SignedAngle(Vector2.right, new Vector2(Mathf.Abs( rb.velocity.x), Mathf.Sign(rb.velocity.x) * rb.velocity.y)), -20, 20)) : Quaternion.identity, 5f);
        }
        waterSurface.transform.position = Vector3.right * transform.position.x;
        if (Application.isEditor)
        {
            EditorUpdate();
        }
    }

    public void Launch(Vector2 force)
    {
        rb.gravityScale = 1;
        rb.AddForce(force);
    }

    private void Boost()
    {
        //Temp Code
        rb.AddForce(Vector3.up * boostForce, ForceMode2D.Impulse);
        foreach (Animator cloud in clouds)
        {
            cloud.SetTrigger("Bounce");
        }
        clouds.Clear();
    }

    private void Skim()
    {
        Vector2 skimVector = new Vector2(Mathf.Abs(rb.velocity.x), rb.velocity.y);
        float skimAngle = Vector2.Angle(Vector2.right, skimVector);
        if (skimAngle <= skimMaxAngle && skimVector.magnitude >= skimMinVelocity)
        {
            rb.velocity *= new Vector2(1, -1) * skimVelocityMultiplier;
        }
        else
        {
            Splash();
        }
    }

    private void Splash()
    {
        rb.velocity *= waterHitVelocityMultiplier;
        Instantiate(splashPrefab, Vector3.Scale(transform.position, Vector3.right), Quaternion.identity);
    }

    void EditorUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            transform.position = new Vector3(-4, 1, 0);
            rb.gravityScale = 0;
            rb.velocity = Vector3.zero;
            rb.drag = 0;
            transform.position = startPos;
            levelManager.isLaunched = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Region"))
        {
            if (rb.velocity.y < 0)
            {
                Skim();
            }
        }
        else if (collision.CompareTag("Cloud"))
        {
            clouds.Add(collision.gameObject.GetComponent<Animator>());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Cloud"))
        {
            clouds.Remove(collision.gameObject.GetComponent<Animator>());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Lid")
        {
            rb.AddForce(new Vector3(-Mathf.Sign(collision.gameObject.transform.lossyScale.x) * musselForce * Mathf.Cos(Mathf.Deg2Rad * musselAngle), musselForce * Mathf.Sin(Mathf.Deg2Rad * musselAngle)), ForceMode2D.Impulse);
            collision.gameObject.transform.parent.gameObject.GetComponent<Animator>().SetTrigger("Open");
        }
        else if (collision.gameObject.name == "Turtle")
        {
            transform.parent = collision.transform;
        }
        else if (collision.gameObject.name == "Jellyfish")
        {
            if (Vector3.Angle(collision.transform.position - transform.position, Vector3.down) < maxJellyfishAngle)
            {
                rb.velocity = jellyfishBoost * Vector3.Reflect(rb.velocity, collision.contacts[0].normal);
                collision.gameObject.GetComponent<Animator>().SetTrigger("Bounce");
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Turtle")
        {
            transform.parent = null;
        }
    }
}