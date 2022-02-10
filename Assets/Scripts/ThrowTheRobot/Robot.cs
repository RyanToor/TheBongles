using UnityEngine;

public class Robot : MonoBehaviour
{
    public float moveForce, jumpForce;
    public float fireForce, underwaterDrag, waterHitVelocityMultiplier, maxDraw;
    public CapsuleCollider2D groundCheck;

    [HideInInspector]
    public bool isLaunched;

    Vector3 pos1, startPos;
    Rigidbody2D rb;
    private bool inWater;
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
        if (levelManager.isLaunched && inWater)
        {
            rb.AddForce(Vector2.right * moveForce * Input.GetAxis("Horizontal"), ForceMode2D.Force);
        }
        if (groundContacts > 0 && Input.GetAxisRaw("Jump") == 1)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
        }
        if (Application.isEditor)
        {
            EditorUpdate();
        }
    }

    public void Launch()
    {
        //rb.AddForce()
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
            inWater = false;
            transform.position = startPos;
            levelManager.isLaunched = false;
        }
    }
}