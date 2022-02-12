using UnityEngine;

public class Robot : MonoBehaviour
{
    public float moveForce, jumpForce, airControlMultiplier, groundCastOffset;
    public float waterHitVelocityMultiplier;

    [HideInInspector]
    public bool isLaunched;

    private Vector3 startPos;
    private Rigidbody2D rb;
    private LevelManager_Robot levelManager;
    private bool isGrounded;
    private float colliderWidth, colliderHeight;

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
            rb.AddForce((isGrounded? 1 : airControlMultiplier) * Input.GetAxis("Horizontal") * moveForce * Vector2.right, ForceMode2D.Force);
            if (isGrounded)
            {
                rb.gravityScale = 1 - Mathf.Abs(Input.GetAxisRaw("Horizontal"));
                if (Input.GetAxisRaw("Jump") == 1)
                {
                    rb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
                }
            }
            else
            {
                rb.gravityScale = 1;
            }
        }
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
}