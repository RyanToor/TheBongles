using System;
using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour
{
    public bool stopRightMovement, doubleJump;
    public LayerMask groundLayerMask, legLayerMask;
    public int legRays, onScreenLinePoints;
    public float reelSpeed, moveForce, jumpForce, airControlMultiplier, groundCastOffset, decelerationMultiplier, 
        boostForce, skimMinVelocity, skimVelocityMultiplier, waterHitVelocityMultiplier, musselForce, jellyfishBoost,
        legMaxLength, legLerpSpeed, rotationSpeed, maxLineSag, hookOffset;
    [Range(0, 90)]
    public float skimMaxAngle, musselAngle, maxJellyfishAngle, legMaxAngle;
    public GameObject waterSurface, splashPrefab, jumpCloudPrefab;
    public SpriteRenderer wheelL, wheelR;
    public Transform fishingPole, hookPoint;

    [HideInInspector]
    public float rightMostPoint = float.MaxValue;

    private Vector3 startPos;
    private Transform legL, legR, hook;
    private Vector2 legStartL, legStartR;
    private Rigidbody2D rb;
    private LevelManager_Robot levelManager;
    private bool isGrounded, isJumping, isDoubleJumping, canDoubleJump = false;
    private float colliderWidth, colliderHeight, startDrag;
    private List<Animator> clouds = new List<Animator>();
    private LineRenderer legLine;
    private Animator animator;
    private LineRenderer returnLine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        startDrag = rb.drag;
        animator = GetComponent<Animator>();
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Robot>();
        colliderWidth = GetComponent<CapsuleCollider2D>().bounds.extents.x;
        colliderHeight = GetComponent<CapsuleCollider2D>().bounds.extents.y;
        legL = transform.Find("Wheel_L");
        legR = transform.Find("Wheel_R");
        legStartL = legL.localPosition;
        legStartR = legR.localPosition;
        legLine = GetComponent<LineRenderer>();
        hook = transform.Find("Hook");
        hookPoint = transform.Find("HookPoint");
        returnLine = hookPoint.gameObject.GetComponent<LineRenderer>();
        if (!Application.isEditor)
        {
            stopRightMovement = true;
        }
    }

    private void OnEnable()
    {
        Application.onBeforeRender += CalculateReturnLine;
    }

    private void Start()
    {

    }

    void FixedUpdate()
    {
        if (levelManager.State == LevelState.fly || levelManager.State == LevelState.move)
        {
            RaycastHit2D groundHit = Physics2D.CircleCast((Vector2)transform.position + GetComponent<CapsuleCollider2D>().offset, colliderWidth, -transform.up, colliderHeight - colliderWidth + groundCastOffset, groundLayerMask);
            isGrounded = groundHit.collider != null;
            Debug.DrawLine((Vector2)transform.position + GetComponent<CapsuleCollider2D>().offset, (Vector2)transform.position + GetComponent<CapsuleCollider2D>().offset - (Vector2)transform.up * (colliderHeight + groundCastOffset), Color.red);
            rb.AddForce((isGrounded? 1 : airControlMultiplier) * Input.GetAxis("Horizontal") * moveForce * (Mathf.Sign(rb.velocity.x) != Mathf.Sign(Input.GetAxisRaw("Horizontal"))? decelerationMultiplier : 1) * Vector2.right, ForceMode2D.Force);
            if (doubleJump && !isGrounded && !isDoubleJumping && Input.GetAxisRaw("Jump") == 0)
            {
                canDoubleJump = true;
            }
            if (isGrounded)
            {
                if (levelManager.State == LevelState.fly)
                {
                    levelManager.State = LevelState.move;
                    levelManager.totalThrowDistance += transform.position.x;
                    rightMostPoint = transform.position.x + 19.2f;
                }
                isJumping = false;
                isDoubleJumping = false;
                canDoubleJump = false;
                rb.gravityScale = 1 - Mathf.Abs(Input.GetAxisRaw("Horizontal"));
                if (Input.GetAxisRaw("Jump") == 1 && !isJumping)
                {
                    rb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
                    animator.SetTrigger("Jump");
                    isJumping = true;
                }
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.up, groundHit.normal, Vector3.forward)), rotationSpeed);
            }
            else
            {
                if (canDoubleJump && !isDoubleJumping && Input.GetAxisRaw("Jump") == 1)
                {
                    rb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
                    Instantiate(jumpCloudPrefab, transform.position, Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.up, groundHit.normal, Vector3.forward)));
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
        if (levelManager.State == LevelState.move)
        {
            if (rightMostPoint > 10.8 && stopRightMovement)
            {
                rightMostPoint -= reelSpeed * Time.deltaTime;
            }
            Debug.DrawLine(Vector3.right * rightMostPoint, Vector3.right * rightMostPoint + Vector3.down * 50f);
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
        Animate();
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
            transform.TransformPoint(legStartL + (isGrounded? Vector2.zero : Vector2.up * 0.3f)),
            transform.position,
            transform.TransformPoint(legStartR + (isGrounded? Vector2.zero : Vector2.up * 0.3f)),
            legR.position + legL.transform.up * 0.3f
        });
    }

    private void Animate()
    {
        animator.SetFloat("HorizontalSpeed", rb.velocity.x);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("VerticalSpeed", rb.velocity.y);
        animator.speed = Mathf.Clamp(rb.velocity.magnitude / 10f, 1, float.MaxValue);
        wheelL.flipX = rb.velocity.x < 0;
        wheelR.flipX = rb.velocity.x < 0;
    }

    public void SetState(LevelState state)
    {
        switch (state)
        {
            case LevelState.launch:
                transform.parent = null;
                transform.position = startPos;
                SetPhysics(0, 0);
                animator.SetBool("isGrounded", true);
                rightMostPoint = float.MaxValue;
                break;
            case LevelState.fly:
                SetPhysics(1, 0);
                break;
            case LevelState.move:
                SetPhysics(1, startDrag);
                break;
            case LevelState.reel:
                SetPhysics(0, 0);
                rb.velocity = Vector3.zero;
                transform.rotation = Quaternion.identity;
                animator.SetBool("isGrounded", true);
                break;
            default:
                break;
        }
    }

    private void SetPhysics(float gravity, float drag)
    {
        rb.gravityScale = gravity;
        rb.drag = drag;
    }

    private void CalculateReturnLine()
    {
        float lineSag = levelManager.State == LevelState.fly? 0 : Mathf.Clamp(maxLineSag * (rightMostPoint - transform.position.x) / 9.6f, 0.1f, maxLineSag) * Mathf.Clamp((transform.position.x - fishingPole.position.x - 9.6f) / 20f, 0, 1);
        Vector3[] tempReturnLinePoints = new Vector3[onScreenLinePoints + 1];
        Vector3 firstPoint = hookPoint.transform.InverseTransformPoint(fishingPole.position);
        Vector3 midPoint = hookPoint.InverseTransformPoint(fishingPole.position) / 2 + hookPoint.InverseTransformDirection(Vector3.down) * lineSag;
        Vector3 lastPoint = Vector3.zero;
        for (int i = 0; i < onScreenLinePoints + 1; i++)
        {
            float fac = (float) i / onScreenLinePoints;
            Vector3 aB = Vector3.Lerp(firstPoint, midPoint, fac);
            Vector3 bC = Vector3.Lerp(midPoint, lastPoint, fac);
            tempReturnLinePoints[i] = Vector3.Lerp(aB, bC, fac);
        }
        Vector3[] returnLinePoints = tempReturnLinePoints;
        for (int i = onScreenLinePoints - 1; i > 1; i--)
        {
            if (tempReturnLinePoints[i].magnitude > hookOffset)
            {
                tempReturnLinePoints[i + 1] += (tempReturnLinePoints[i + 1] - tempReturnLinePoints[i]).normalized * (tempReturnLinePoints[i + 1].magnitude - hookOffset);
                returnLinePoints = new Vector3[i + 2];
                Array.Copy(tempReturnLinePoints, 0, returnLinePoints, 0, i + 2);
                break;
            }
        }
        hook.position = hookPoint.TransformPoint(returnLinePoints[returnLinePoints.Length - 1]);
        hook.transform.rotation = Quaternion.Euler(0, 0, Vector3.Angle(Vector3.down, hookPoint.position - hook.position));
        returnLine.positionCount = returnLinePoints.Length;
        returnLine.SetPositions(returnLinePoints);
        Debug.DrawLine(hookPoint.TransformPoint(firstPoint), hookPoint.TransformPoint(midPoint));
        Debug.DrawLine(hookPoint.TransformPoint(midPoint), hookPoint.TransformPoint(lastPoint));
        Debug.DrawLine(hookPoint.TransformPoint(firstPoint), hookPoint.TransformPoint(lastPoint));
    }

    void EditorUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            rb.velocity = Vector3.zero;
            transform.position = startPos;
            levelManager.State = LevelState.launch;
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
            collision.gameObject.GetComponent<Animator>().SetTrigger("Bounce");
            rb.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);
        }
        else if (collision.gameObject.CompareTag("RandomTrash"))
        {
            levelManager.metal++;
            levelManager.floatingTrash.objectsToRemove.Add(collision.gameObject);
        }
        else if (collision.CompareTag("Boss"))
        {
            levelManager.Pies++;
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.name == "SandCollectionTile")
        {
            transform.parent = collision.transform;
            levelManager.State = LevelState.reel;
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

    private void OnDisable()
    {
        Application.onBeforeRender -= CalculateReturnLine;
    }
}