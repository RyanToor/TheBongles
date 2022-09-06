using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Robot : MonoBehaviour
{
    public bool stopRightMovement, doubleJump, jellyfishStraightBounce;
    public ContactFilter2D groundContactFilter, magnetContactFilter, legContactFilter;
    public int legRays, onScreenLinePoints;
    public float[] launchForceUpgrades;
    public float reelSpeed, moveForce, jumpForce, airControlMultiplier, groundCastOffset, decelerationMultiplier, 
        cloudBoostForce, boostForce, skimMinVelocity, skimVelocityMultiplier, waterHitVelocityMultiplier, musselForce, jellyfishBoost, slipperyDrag,
        legMaxLength, legLerpSpeed, rotationSpeed, maxLineSag, hookOffset,
        cloudBoostFuel, maxBoostFuel, boostFuel, boostCostPerSecond,
        magnetScanWidth, magnetScanInterval, magnetScanPeriod, magnetArmSpeed, magnetCooldown,
        windCooldown;
    [Range(0, 90)]
    public float skimMaxAngle, musselAngle, maxJellyfishAngle, legMaxAngle;
    public AnimationCurve doubleJumpMultiplier;
    public GameObject waterSurface, splashPrefab, skimPrefab, jumpCloudPrefab, magnetPrefab, magnetPanel;
    public SpriteRenderer wheelL, wheelR;
    public Transform fishingPole, hookPoint, ballista;
    public FuelBarUpgrade[] fuelUpgrades;
    public Image magnetImage, magnetCooldownPanel;
    public Animator magnetFrameAnimator;
    public GameObject[] windParticles;
    [Header("Vibration Data")]
    [SerializeField] float skimIntensity;
    [SerializeField]
    float skimDuration, splashIntensity, splashDuration,
        clamIntensity, clamDuration, jellyfishIntensity, jellyfishDuration, cloudIntensity, cloudDuration,
        randomTrashIntensity, randomTrashDuration,
        windIntensity, windDuration, boostIntensity, lineIntensity;
    [SerializeField] AnimationCurve jellyfishCurve, randomTrashCurve, windCurve;

    [HideInInspector]
    public float rightMostPoint = float.MaxValue;
    [HideInInspector]
    public Transform startParent;

    private Vector3 startPos;
    private Transform legL, legR, hook;
    private Vector2 legStartL, legStartR;
    private Rigidbody2D rb;
    private LevelManager_Robot levelManager;
    private bool isGrounded, canDoubleJump, magnetEnabled, isMagnetAvailable = true;
    private float colliderWidth, colliderHeight, startDrag, launchForce = 1, storedLinearDrag;
    private List<Animator> clouds = new List<Animator>();
    private LineRenderer legLine;
    private Animator animator;
    private LineRenderer returnLine;
    private bool isBoosting, isMagnetCooling, isWindSpawning;
    private Color magnetDisabledColour;
    private RaycastHit2D[] groundHits = new RaycastHit2D[1];
    private GameObject splash;
    private InputManager.VibrationData lineVibration;

    private void Awake()
    {
        InputManager.Instance.PrimaryAbility += Magnet;
        InputManager.Instance.Jump += Jump;
        startParent = transform.parent;
        magnetDisabledColour = magnetImage.color;
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
        magnetFrameAnimator.SetBool("Available", false);
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
        for (int i = 0; i < 3; i++)
        {
            if (transform.Find("Upgrade" + (i + 1) + "-" + GameManager.Instance.upgrades[1][i]) != null)
            {
                transform.Find("Upgrade" + (i + 1) + "-" + GameManager.Instance.upgrades[1][i]).gameObject.SetActive(true);
            }
            Upgrade(new Vector2Int(i + 1, GameManager.Instance.upgrades[1][i]));
        }
    }

    void FixedUpdate()
    {
        groundHits[0] = new RaycastHit2D();
        isGrounded = Physics2D.CircleCast((Vector2)transform.position + GetComponent<CapsuleCollider2D>().offset, colliderWidth, Vector2.down, groundContactFilter, groundHits, colliderHeight - colliderWidth + groundCastOffset) > 0;
        if (levelManager.State == LevelState.fly || levelManager.State == LevelState.move)
        {
            Vector2 moveDir = InputManager.Instance.move;
            Debug.DrawLine((Vector2)transform.position + GetComponent<CapsuleCollider2D>().offset, (Vector2)transform.position + GetComponent<CapsuleCollider2D>().offset - (Vector2)transform.up * (colliderHeight + groundCastOffset), Color.red);
            rb.AddForce((isGrounded? 1 : airControlMultiplier) * moveDir.x * moveForce * (Mathf.Sign(rb.velocity.x) != Mathf.Sign(moveDir.x)? decelerationMultiplier : 1) * Vector2.right, ForceMode2D.Force);
            if (levelManager.State == LevelState.fly)
            {
                if (isGrounded)
                {
                    if (!groundHits[0].collider.CompareTag("Minigame"))
                    {
                        levelManager.State = LevelState.move;
                        levelManager.totalThrowDistance += Mathf.Clamp(transform.position.x, 0, float.MaxValue);
                        rightMostPoint = transform.position.x + 19.2f;
                    }
                }
                else
                {
                    if (boostFuel > 0 && InputManager.Instance.jump)
                    {
                        Boost();
                    }
                }
            }
            if (isGrounded)
            {
                if (doubleJump)
                {
                    canDoubleJump = true;
                }
                rb.gravityScale = moveDir.x == 0? 1 : 0;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, Mathf.Clamp(Vector3.SignedAngle(Vector3.up, groundHits[0].normal, Vector3.forward), -20, 20)), rotationSpeed);
            }
            else
            {
                rb.gravityScale = 1;
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

    private void Jump()
    {
        if (isGrounded || groundHits[0].collider != null && groundHits[0].collider.CompareTag("Minigame"))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
            animator.SetTrigger("Jump");
            AudioManager.Instance.PlaySFX("Jump");
        }
        else if (canDoubleJump && levelManager.State == LevelState.move)
        {
            rb.AddForce(Vector3.up * jumpForce * doubleJumpMultiplier.Evaluate(rb.velocity.y), ForceMode2D.Impulse);
            Instantiate(jumpCloudPrefab, transform.position, Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.up, groundHits[0].normal, Vector3.forward)));
            canDoubleJump = false;
            AudioManager.Instance.PlaySFX("DoubleJump");
        }
    }

    public void Launch(Vector2 force)
    {
        rb.gravityScale = 1;
        rb.AddForce(force * launchForce);
    }

    private void Boost()
    {
        rb.AddForce(10 * boostForce * Time.deltaTime * Vector3.up, ForceMode2D.Impulse);
        boostFuel = Mathf.Clamp(boostFuel - Time.deltaTime, 0, maxBoostFuel);
        foreach (Animator cloud in clouds)
        {
            cloud.SetTrigger("Bounce");
        }
        clouds.Clear();

        if (!isBoosting)
        {
            StartCoroutine(BoostSound());
        }
    }

    private void Skim()
    {
        Vector2 skimVector = new Vector2(Mathf.Abs(rb.velocity.x), rb.velocity.y);
        float skimAngle = Vector2.Angle(Vector2.right, skimVector);
        if (skimAngle <= skimMaxAngle && skimVector.magnitude >= skimMinVelocity)
        {
            rb.velocity *= new Vector2(1, -1) * skimVelocityMultiplier;
            AudioManager.Instance.PlaySFXAtLocation("Skim", transform.position, 15);
            Instantiate(skimPrefab, Vector3.Scale(transform.position, Vector3.right), Quaternion.identity);
            InputManager.Instance.Vibrate(skimIntensity, skimDuration);
        }
    }

    private void Splash()
    {
        rb.velocity *= waterHitVelocityMultiplier;
        splash = Instantiate(splashPrefab, Vector3.Scale(transform.position, Vector3.right), Quaternion.identity);
        AudioManager.Instance.PlaySFXAtLocation("Splash", transform.position, 15);
        AudioManager.Instance.PlaySFXAtLocation("BubblingWater", transform.position, 15);
        InputManager.Instance.Vibrate(splashIntensity, splashDuration);
    }

    private Vector2 Legs(Vector3 legOrigin)
    {
        int dir = -1;
        float legAngleMultiplier = 0;
        int sideRays = Mathf.FloorToInt(legRays / 2);
        for (int i = 0; i < legRays; i++)
        {
            Debug.DrawLine(transform.position + Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z) * legOrigin, transform.position + Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z) * legOrigin + Quaternion.Euler(0, 0, dir * legMaxAngle * legAngleMultiplier / sideRays) * -transform.up * legMaxLength / Mathf.Cos(Mathf.Deg2Rad * legAngleMultiplier * (legMaxAngle / sideRays)));
            RaycastHit2D[] hits = new RaycastHit2D[1];
            Physics2D.Raycast(transform.position + Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z) * legOrigin, Quaternion.Euler(0, 0, dir * legMaxAngle * legAngleMultiplier / sideRays)  * Vector2.down, legContactFilter, hits, legMaxLength / Mathf.Cos(Mathf.Deg2Rad * legAngleMultiplier * (legMaxAngle / sideRays)));
            if (hits[0].collider != null)
            {
                return transform.InverseTransformPoint(hits[0].point);
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
        AnimatorStateInfo animState = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
        legLine.SetPositions(new Vector3[5]
        {
            legL.localPosition + Vector3.up * 0.3f,
            legStartL + (animState.IsName("Idle") || animState.IsName("Move_Right") || animState.IsName("MoveLeft")? Vector2.zero : Vector2.up * 0.3f),
            Vector3.zero,
            legStartR + (animState.IsName("Idle") || animState.IsName("Move_Right") || animState.IsName("MoveLeft")? Vector2.zero : Vector2.up * 0.3f),
            legR.localPosition + Vector3.up * 0.3f
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
                SetPhysics(0, 0);
                animator.SetBool("isGrounded", true);
                rightMostPoint = float.MaxValue;
                isMagnetAvailable = false;
                magnetFrameAnimator.SetBool("Available", false);
                break;
            case LevelState.fly:
                SetPhysics(1, 0);
                isMagnetAvailable = false;
                magnetFrameAnimator.SetBool("Available", false);
                break;
            case LevelState.move:
                SetPhysics(1, startDrag);
                if (!isMagnetCooling)
                {
                    isMagnetAvailable = true;
                    magnetFrameAnimator.SetBool("Available", true);
                }
                levelManager.promptManager.Prompt(1);
                break;
            case LevelState.reel:
                SetPhysics(0, 0);
                rb.velocity = Vector3.zero;
                transform.rotation = Quaternion.identity;
                animator.SetBool("isGrounded", true);
                isMagnetAvailable = false;
                magnetFrameAnimator.SetBool("Available", false);
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

    private void Magnet()
    {
        if (levelManager.State == LevelState.move && isMagnetAvailable && magnetEnabled)
        {
            StartCoroutine(MagnetActive());
        }
    }

    private IEnumerator MagnetActive()
    {
        isMagnetAvailable = false;
        magnetFrameAnimator.SetBool("Available", false);
        transform.GetChild(0).gameObject.SetActive(false);
        float magnetDuration = 0;
        List<GameObject> foundTrash = new List<GameObject>();
        while (magnetDuration < magnetScanPeriod)
        {
            float intervalDuration = 0;
            RaycastHit2D[] rayHits = Physics2D.RaycastAll(transform.position, Vector3.down);
            List<Collider2D> newTrash = new List<Collider2D>();
            if (rayHits.Length > 0)
            {
                Vector3 boxBottom = rayHits[rayHits.Length - 1].point + Vector2.down * 5;
                Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y > 0 ? (transform.position.y + boxBottom.y) / 2 : boxBottom.y / 2), new Vector2(magnetScanWidth, transform.position.y > 0 ? transform.position.y + Mathf.Abs(boxBottom.y) : Mathf.Abs(boxBottom.y)), 0f, magnetContactFilter, newTrash);
            }
            foreach (Collider2D collider in newTrash)
            {
                if (collider.CompareTag("RandomTrash") && !foundTrash.Contains(collider.gameObject))
                {
                    foundTrash.Add(collider.gameObject);
                    StartCoroutine(MagnetArm(collider.gameObject));
                }
            }
            while (intervalDuration < magnetScanInterval)
            {
                magnetDuration += Time.deltaTime;
                intervalDuration += Time.deltaTime;
                magnetCooldownPanel.transform.localScale = Vector2.Lerp(Vector2.right, Vector2.one, magnetDuration / magnetScanPeriod);
                yield return null;
            }
        }
        magnetImage.color = magnetDisabledColour;
        transform.GetChild(0).gameObject.SetActive(true);
        isMagnetCooling = true;
        float duration = magnetCooldown;
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            magnetCooldownPanel.transform.localScale = Vector2.Lerp(Vector2.right, Vector2.one, duration / magnetCooldown);
            yield return null;
        }
        magnetCooldownPanel.transform.localScale = Vector2.right;
        isMagnetAvailable = true;
        magnetFrameAnimator.SetBool("Available", true);
        isMagnetCooling = false;
        magnetImage.color = Color.white;
        AudioManager.Instance.PlaySFX("AbilityReady");
    }

    private IEnumerator MagnetArm(GameObject target)
    {
        target.GetComponent<Collider2D>().enabled = false;
        GameObject magnet = Instantiate(magnetPrefab, transform.position, Quaternion.identity, transform);
        LineRenderer line = magnet.GetComponent<LineRenderer>();
        float armProgress = 0;
        while (target != null && armProgress < Vector2.Distance(transform.position, target.transform.position))
        {
            armProgress += magnetArmSpeed * Time.deltaTime;
            Vector2 targetLocalPos = transform.InverseTransformPoint(target.transform.position);
            magnet.transform.localPosition = targetLocalPos * (armProgress / targetLocalPos.magnitude);
            magnet.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, target.transform.position - transform.position));
            line.SetPosition(1, magnet.transform.InverseTransformPoint(magnet.transform.parent.position));
            yield return null;
        }
        target.transform.parent = magnet.transform;
        target.transform.localPosition = Vector3.zero;
        levelManager.floatingTrash.objectsToRemove.Add(target);
        levelManager.floatingTrash.doNotDestroy.Add(target);
        Vector3 magnetCatchPos = magnet.transform.position;
        while (armProgress > 0)
        {
            armProgress -= magnetArmSpeed * Time.deltaTime;
            Vector2 localMagnetCatchPos = transform.InverseTransformPoint(magnetCatchPos);
            magnet.transform.localPosition = localMagnetCatchPos * (armProgress / localMagnetCatchPos.magnitude);
            magnet.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, magnetCatchPos - transform.position));
            line.SetPosition(1, magnet.transform.InverseTransformPoint(magnet.transform.parent.position));
            yield return null;
        }
        foreach (Transform trash in magnet.transform)
        {
            levelManager.metal++;
            GameManager.Instance.SpawnCollectionIndicator(trash.position, levelManager.collectionIndicatorColor);
            Destroy(trash.gameObject);
        }
        Destroy(magnet);
    }

    private void CalculateReturnLine()
    {
        float lineSag = levelManager.State == LevelState.fly? 0 : Mathf.Clamp(maxLineSag * (rightMostPoint - transform.position.x) / 9.6f, 0.1f, maxLineSag) * Mathf.Clamp((transform.position.x - fishingPole.position.x - 9.6f) / 50f, 0, 1);
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
        hook.transform.rotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.down, hookPoint.position - hook.position, Vector3.forward));
        returnLine.positionCount = returnLinePoints.Length;
        returnLine.SetPositions(returnLinePoints);
        Debug.DrawLine(hookPoint.TransformPoint(firstPoint), hookPoint.TransformPoint(midPoint));
        Debug.DrawLine(hookPoint.TransformPoint(midPoint), hookPoint.TransformPoint(lastPoint));
        Debug.DrawLine(hookPoint.TransformPoint(firstPoint), hookPoint.TransformPoint(lastPoint));
    }

    private void Upgrade(Vector2Int upgradeIndicies)
    {
        switch (upgradeIndicies.x)
        {
            case 1:
                if (upgradeIndicies.y > 0)
                {
                    doubleJump = true;
                    if (upgradeIndicies.y > 1)
                    {
                        maxBoostFuel = fuelUpgrades[upgradeIndicies.y - 2].maxFuel;
                        boostFuel = fuelUpgrades[upgradeIndicies.y - 2].startFuel;
                    }
                }
                break;
            case 2:
                launchForce = launchForceUpgrades[upgradeIndicies.y];
                levelManager.ThrowPowerLevel = upgradeIndicies.y;
                break;
            case 3:
                if (upgradeIndicies.y > 0)
                {
                    magnetEnabled = true;
                    magnetCooldownPanel.transform.localScale = Vector2.right;
                    magnetImage.color = Color.white;
                }
                else
                {
                    magnetEnabled = false;
                    magnetPanel.SetActive(false);
                }
                break;
            default:
                break;
        }
    }

    private IEnumerator Wind(Vector3 pos)
    {
        isWindSpawning = true;
        float duration = 0;
        InputManager.Instance.Vibrate(windIntensity, windDuration, windCurve);
        for (int i = 0; i < windParticles.Length; i++)
        {
            GameObject newWind = Instantiate(windParticles[i], pos + Vector3.right, Quaternion.Euler(0, 0, 180), GameManager.Instance.transform);
            newWind.transform.localScale = new Vector3(1, UnityEngine.Random.value < 0.5 ? -1 : 1, 1);
        }
        while (duration < windCooldown)
        {
            duration += Time.deltaTime;
            yield return null;
        }
        isWindSpawning = false;
    }
    private IEnumerator BoostSound()
    {
        InputManager.VibrationData boostVibration = InputManager.Instance.Vibrate();
        isBoosting = true;
        AudioSource boostSource = AudioManager.Instance.PlayAudioAtObject("Boost", gameObject, 20, true);
        while (InputManager.Instance.jump && boostFuel > 0)
        {
            boostVibration.Intensity = Mathf.Lerp(0.4f, 1f, boostFuel / maxBoostFuel) * boostIntensity;
            yield return null;
        }
        Destroy(boostSource);
        InputManager.Instance.vibrations.Remove(boostVibration);
        isBoosting = false;
    }

    public IEnumerator ReleaseSling()
    {
        LineRenderer line = ballista.parent.GetComponent<LineRenderer>();
        while (transform.position.x < ballista.position.x)
        {
            line.SetPosition(1, ballista.parent.InverseTransformPoint(transform.position));
            yield return null;
        }
        GetComponent<Animator>().SetBool("Ballista", false);
        line.SetPosition(1, line.GetPosition(0));
    }

    void EditorUpdate()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard.enterKey.wasPressedThisFrame)
        {
            rb.velocity = Vector3.zero;
            transform.position = startPos;
            levelManager.State = LevelState.launch;
        }
        if (keyboard.mKey.wasPressedThisFrame)
        {
            rb.velocity = Vector3.zero;
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
            AudioManager.Instance.PlaySFXAtLocation("Cloud", transform.position, 15);
            rb.AddForce(cloudBoostForce * Vector2.up, ForceMode2D.Impulse);
            boostFuel = Mathf.Clamp(boostFuel + cloudBoostFuel, 0, maxBoostFuel);
            InputManager.Instance.Vibrate(cloudIntensity, cloudDuration, jellyfishCurve);
        }
        else if (collision.gameObject.CompareTag("RandomTrash"))
        {
            levelManager.metal++;
            levelManager.floatingTrash.objectsToRemove.Add(collision.gameObject);
            AudioManager.Instance.PlaySFXAtLocation("Metal", transform.position, 15);
            GameManager.Instance.SpawnCollectionIndicator(collision.transform.position, levelManager.collectionIndicatorColor);
            InputManager.Instance.Vibrate(randomTrashIntensity, randomTrashDuration, randomTrashCurve);
        }
        else if (collision.CompareTag("Boss"))
        {
            levelManager.Pies++;
            Destroy(collision.gameObject);
            AudioManager.Instance.PlaySFXAtLocation("PieGrab", transform.position, 20);
            InputManager.Instance.Vibrate(randomTrashIntensity, randomTrashDuration, randomTrashCurve);
        }
        else if (collision.gameObject.name == "SandCollectionTile")
        {
            levelManager.State = LevelState.reel;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Cloud"))
        {
            clouds.Remove(collision.gameObject.GetComponent<Animator>());
        }
        else if (collision.CompareTag("Region"))
        {
            if (rb.velocity.y < 0 && splash == null)
            {
                Splash();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string[] name = collision.gameObject.name.Split(' ');
        if (collision.gameObject.name == "Lid")
        {
            rb.AddForce(new Vector3(-Mathf.Sign(collision.gameObject.transform.lossyScale.x) * musselForce * Mathf.Cos(Mathf.Deg2Rad * musselAngle), musselForce * Mathf.Sin(Mathf.Deg2Rad * musselAngle)), ForceMode2D.Impulse);
            collision.gameObject.transform.parent.gameObject.GetComponent<Animator>().SetTrigger("Open");
            AudioManager.Instance.PlaySFXAtLocation("Clam", transform.position, 20);
            InputManager.Instance.Vibrate(clamIntensity, clamDuration);

        }
        else if (collision.gameObject.name == "Turtle")
        {
            transform.parent = collision.transform;
        }
        else if (name[0] == "Jellyfish")
        {
            if (Vector3.Angle(collision.transform.position - transform.position, Vector3.down) < maxJellyfishAngle && levelManager.State == LevelState.move)
            {
                rb.velocity = jellyfishBoost * Vector3.Reflect(rb.velocity, collision.contacts[0].normal);
                if (jellyfishStraightBounce && Vector3.Angle(Vector2.up, rb.velocity) > maxJellyfishAngle)
                {
                    rb.velocity = Quaternion.AngleAxis(Mathf.Clamp(Vector3.SignedAngle(Vector3.up, rb.velocity, Vector3.forward), 0, maxJellyfishAngle), Vector3.forward) * Vector3.up * rb.velocity.magnitude;
                }
                collision.gameObject.GetComponent<Animator>().SetTrigger("Bounce");
                AudioManager.Instance.PlaySFXAtLocation("Jellyfish", transform.position, 20);
                InputManager.Instance.Vibrate(jellyfishIntensity, jellyfishDuration, jellyfishCurve);
            }
        }
        else if (collision.gameObject.CompareTag("MainCamera"))
        {
            if (transform.position.y > 1.5)
            {
                if (!isWindSpawning)
                {
                    StartCoroutine(Wind(collision.GetContact(0).point));
                }
            }
            else if (lineVibration == null)
            {
                lineVibration = InputManager.Instance.Vibrate(lineIntensity, 0, 0);
            }
        }
        else if (name[0] == "Platform_Crystal_Oil" || name[0] == "OilRock")
        {
            storedLinearDrag = rb.drag;
            rb.drag = slipperyDrag;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        string[] name = collision.gameObject.name.Split(' ');
        if (collision.gameObject.name == "Turtle")
        {
            rb.velocity += (collision.gameObject.GetComponent<SpriteRenderer>().flipX? 1 : -1) * collision.gameObject.GetComponent<ProximityElement>().speed * Vector2.right;
            transform.parent = null;
        }
        else if (name[0] == "Platform_Crystal_Oil" || name[0] == "OilRock")
        {
            rb.drag = storedLinearDrag;
        }
        else if (collision.gameObject.CompareTag("MainCamera"))
        {
            if (lineVibration != null)
            {
                InputManager.Instance.vibrations.Remove(lineVibration);
                lineVibration = null;
            }
        }
    }

    private void OnDisable()
    {
        Application.onBeforeRender -= CalculateReturnLine;
        InputManager.Instance.PrimaryAbility -= Magnet;
        InputManager.Instance.Jump -= Jump;
    }

    [System.Serializable]
    public struct FuelBarUpgrade
    {
        public float startFuel;
        public float maxFuel;
    }
}