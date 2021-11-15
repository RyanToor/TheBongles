using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveForce, maxSpeed, airControlMultiplier, waterDrag, airDrag;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float gravity, controlMultiplier, moveRight = 1, prevFlipDir = 1, flipDir = 1, spriteDir = 1;
    private Vector2 moveDir;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        gravity = rb.gravityScale;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void LateUpdate()
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
        moveDir = new Vector3(moveRight, Input.GetAxis("Vertical")).normalized;
        rb.AddForce(rb.mass * moveDir * moveForce * controlMultiplier * Time.deltaTime, ForceMode2D.Force);
        rb.velocity = rb.velocity.normalized * Mathf.Clamp(rb.velocity.magnitude, 0, maxSpeed);
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
        transform.rotation = Quaternion.LerpUnclamped(Quaternion.identity, Quaternion.AngleAxis(90, Vector3.forward), spriteDir * rb.velocity.y / maxSpeed);
        animator.SetFloat("Speed", rb.velocity.magnitude);
        animator.speed = Mathf.Clamp(rb.velocity.magnitude / maxSpeed, 0.5f, 1);
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
}