using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour
{
    public bool doubleJump;
    public LayerMask groundLayerMask, legLayerMask;
    public int legRays;
    public float moveForce, jumpForce, airControlMultiplier, groundCastOffset, decelerationMultiplier, 
        boostForce, skimMinVelocity, skimVelocityMultiplier, waterHitVelocityMultiplier, musselForce, jellyfishBoost,
        legMaxLength, legLerpSpeed, rotationSpeed;
    [Range(0, 90)]
    public float skimMaxAngle, musselAngle, maxJellyfishAngle, legMaxAngle;
    public GameObject waterSurface, splashPrefab;

    [HideInInspector]
    public bool isLanded = false;
    [HideInInspector]
    public int rightmostChunk = int.MaxValue;

    private Vector3 startPos;
    private Transform legL, legR;
    private Vector2 legStartL, legStartR;
    private Rigidbody2D rb;
    private LevelManager_Robot levelManager;
    private bool isGrounded, isJumping, isDoubleJumping, canDoubleJump = false;
    private float colliderWidth, colliderHeight;
    private List<Animator> clouds = new List<Animator>();
    private LineRenderer legLine;

    private void Awake()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Robot>();
        colliderWidth = GetComponent<CapsuleCollider2D>().bounds.extents.x;
        colliderHeight = GetComponent<CapsuleCollider2D>().bounds.extents.y;
        legL = transform.Find("Wheel_L");
        legR = transform.Find("Wheel_R");
        legStartL = legL.localPosition;
        legStartR = legR.localPosition;
        legLine = GetComponent<LineRenderer>();
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
            RaycastHit2D groundHit = Physics2D.CircleCast((Vector2)transform.position + GetComponent<CapsuleCollider2D>().offset, colliderWidth, -transform.up, colliderHeight - colliderWidth + groundCastOffset);
            isGrounded = groundHit.collider != null;
            Debug.DrawLine((Vector2)transform.position + GetComponent<CapsuleCollider2D>().offset, (Vector2)transform.position + GetComponent<CapsuleCollider2D>().offset - (Vector2)transform.up * (colliderHeight + groundCastOffset), Color.red);
            rb.AddForce((isGrounded? 1 : airControlMultiplier) * Input.GetAxis("Horizontal") * moveForce * (Mathf.Sign(rb.velocity.x) != Mathf.Sign(Input.GetAxisRaw("Horizontal"))? decelerationMultiplier : 1) * Vector2.right, ForceMode2D.Force);
            if (doubleJump && !isGrounded && !isDoubleJumping && Input.GetAxisRaw("Jump") == 0)
            {
                canDoubleJump = true;
            }
            if (isGrounded)
            {
                if (levelManager.isLaunched && ! isLanded)
                {
                    isLanded = true;
                }
                isJumping = false;
                isDoubleJumping = false;
                canDoubleJump = false;
                rb.gravityScale = 1 - Mathf.Abs(Input.GetAxisRaw("Horizontal"));
                if (Input.GetAxisRaw("Jump") == 1 && !isJumping)
                {
                    rb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
                    isJumping = true;
                }
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.up, groundHit.normal, Vector3.forward)), rotationSpeed);
            }
            else
            {
                if (canDoubleJump && !isDoubleJumping && Input.GetAxisRaw("Jump") == 1)
                {
                    rb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
                    isDoubleJumping = true;
                }
                else if (clouds.Count > 0 && Input.GetAxisRaw("Jump") == 1)
                {
                    Boost();
                }
                else
                {
                    rb.gravityScale = 1;
                }
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rb.velocity.magnitude > 1f ? Quaternion.Euler(0, 0, Mathf.Clamp(Vector2.SignedAngle(Vector2.right, new Vector2(Mathf.Abs(rb.velocity.x), Mathf.Sign(rb.velocity.x) * rb.velocity.y)), -20, 20)) : Quaternion.identity, rotationSpeed);
            }
        }
        legL.localPosition = Vector2.Lerp(legL.localPosition, Legs(legStartL), legLerpSpeed);
        legR.localPosition = Vector2.Lerp(legR.localPosition, Legs(legStartR), legLerpSpeed);
        waterSurface.transform.position = Vector3.right * transform.position.x;
        if (Application.isEditor)
        {
            EditorUpdate();
        }
    }

    private void LateUpdate()
    {
        DrawLegs();
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

    private Vector2 Legs(Vector3 legOrigin)
    {
        int dir = -1;
        float legAngleMultiplier = 0;
        int sideRays = Mathf.FloorToInt(legRays / 2);
        for (int i = 0; i < legRays; i++)
        {
            Debug.DrawLine(transform.position + Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z) * legOrigin, transform.position + Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z) * legOrigin + Quaternion.Euler(0, 0, dir * legMaxAngle * legAngleMultiplier / sideRays) * -transform.up * legMaxLength / Mathf.Cos(Mathf.Deg2Rad * legAngleMultiplier * (legMaxAngle / sideRays)));
            RaycastHit2D hit = Physics2D.Raycast(transform.position + Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z) * legOrigin, Quaternion.Euler(0, 0, dir * legMaxAngle * legAngleMultiplier / sideRays)  * Vector2.down, legMaxLength / Mathf.Cos(Mathf.Deg2Rad * legAngleMultiplier * (legMaxAngle / sideRays)), legLayerMask);
            if (hit.collider != null)
            {
                return transform.InverseTransformPoint(hit.point);
            }
            if (dir == -1)
            {
                legAngleMultiplier += 1;
            }
            dir *= -1;
        }
        return legOrigin - Quaternion.Euler(0, 0, -transform.rotation.eulerAngles.z) * transform.up * legMaxLength;
    }

    private void DrawLegs()
    {
        legLine.SetPositions(new Vector3[5]
        {
            legL.position + legL.transform.up * 0.3f,
            transform.TransformPoint(legStartL),
            transform.position,
            transform.TransformPoint(legStartR),
            legR.position + legL.transform.up * 0.3f
        });
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
            isLanded = false;
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