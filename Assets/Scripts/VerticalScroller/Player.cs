using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveForce, airControl;

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y > 0)
        {
            rb.gravityScale = 1;
            rb.drag = 0.2f;
        }
        else
        {
            rb.gravityScale = 0;
            rb.drag = 1;
        }
        float controlMultiplier;
        if (transform.position.y < 0)
        {
            controlMultiplier = 1;
        }
        else
        {
            controlMultiplier = airControl;
        }
        float moveUp = Input.GetAxis("Vertical");
        float moveRight = Input.GetAxis("Horizontal");
        Vector2 moveDir = new Vector3(moveRight, moveUp).normalized;
        rb.AddForce(moveDir * moveForce * controlMultiplier * Time.deltaTime);
    }
}