using UnityEngine;

public class Robot : MonoBehaviour
{
    public float moveForce, jumpForce, airControlMultiplier;
    public float waterHitVelocityMultiplier;

    [HideInInspector]
    public bool isLaunched;

    private Vector3 startPos;
    private Rigidbody2D rb;
    private int groundContacts;
    private LevelManager_Robot levelManager;

    private void Awake()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Robot>();
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
            rb.AddForce(Input.GetAxis("Horizontal") * moveForce * Vector2.right * (groundContacts > 0? 1 : airControlMultiplier), ForceMode2D.Force);
            if (groundContacts > 0 && Input.GetAxisRaw("Jump") == 1)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        groundContacts++;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        groundContacts--;
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