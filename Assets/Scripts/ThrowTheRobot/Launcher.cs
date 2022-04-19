using System.Collections;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    public float reelSpeed, lineWidth, armSpeed, postThrowPause;
    public Transform robotHoldPoint, ballistaArm, cannonBarrel;
    public ThrowInputs throwScript;
    public GameObject thrownHand;
    public SpriteRenderer piesSprite;
    public Sprite[] pieSprites;

    [HideInInspector]
    public bool isReeling, isAngleSet;
    [HideInInspector]
    public Vector3 throwVector;

    private LevelManager_Robot levelManager;
    private Transform reelBottom, reelTop, arm;
    private GameObject robot;
    private bool reelStarted, isEnded;
    private float armStartAngle;
    private Animator animator, robotAnimator;
    private Coroutine pauseCoroutine;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Robot>();
        robot = GameObject.FindGameObjectWithTag("Player");
        robotAnimator = robot.GetComponent<Animator>();
        reelBottom = transform.Find("LineBottom");
        reelTop = transform.Find("LineStop");
        arm = transform.Find("AngleArm");
        armStartAngle = arm.rotation.eulerAngles.z;
        isReeling = true;
        UpdatePies();
    }

    private void Update()
    {
        if (Application.isEditor)
        {
            EditorUpdate();
        }
    }

    public IEnumerator Angle(float angle, bool isReturning = false)
    {
        switch (levelManager.throwPowerLevel)
        {
            case 0:
                if (!isReturning)
                {
                    animator.SetTrigger("CollectingPower");
                    Hold(0);
                }
                float progress = 0f, startAngle = isReturning ? arm.rotation.eulerAngles.z : armStartAngle, endAngle = isReturning ? armStartAngle : angle;
                arm.gameObject.SetActive(true);
                while (progress < 1)
                {
                    progress = Mathf.Clamp(progress + armSpeed * Time.deltaTime, 0, 1);
                    arm.rotation = Quaternion.Slerp(Quaternion.Euler(0, 0, startAngle), Quaternion.Euler(0, 0, endAngle), progress);
                    yield return null;
                }
                if (isReturning)
                {
                    arm.gameObject.SetActive(false);
                }
                break;
            case 1:
                progress = 0;  startAngle = isReturning ? ballistaArm.rotation.eulerAngles.z : 11f; endAngle = isReturning ? 11f : Mathf.Clamp(angle, 11f, 70f);
                while (progress < 1)
                {
                    progress = Mathf.Clamp(progress + armSpeed * Time.deltaTime, 0, 1);
                    ballistaArm.rotation = Quaternion.Slerp(Quaternion.Euler(0, 0, startAngle), Quaternion.Euler(0, 0, endAngle), progress);
                    yield return null;
                }
                break;
            case 2:
                progress = 0; startAngle = isReturning ? cannonBarrel.rotation.eulerAngles.z : 0f; endAngle = isReturning ? 0f : -90f + angle;
                while (progress < 1)
                {
                    progress = Mathf.Clamp(progress + armSpeed * Time.deltaTime, 0, 1);
                    cannonBarrel.rotation = Quaternion.Slerp(Quaternion.Euler(0, 0, startAngle), Quaternion.Euler(0, 0, endAngle), progress);
                    yield return null;
                }
                break;
            default:
                break;
        }
        isAngleSet = true;
    }

    public void PowerCollected()
    {
        if (levelManager.throwPowerLevel > 0)
        {
            animator.SetTrigger("CollectingPower");
        }
    }

    public void UpdatePies()
    {
        piesSprite.sprite = levelManager.Pies > 0 ? pieSprites[Mathf.Clamp(levelManager.pies - 1, 0, pieSprites.Length - 1)] : null;
    }

    public void Reel()
    {
        if (!reelStarted)
        {
            if (pauseCoroutine != null)
            {
                StopCoroutine(pauseCoroutine);
                animator.speed = 1;
            }
            StartCoroutine(ReelIn());
            reelStarted = true;
        }
    }

    public IEnumerator ReelIn()
    {
        Vector3 robotStartPos = robot.transform.position;
        float progress = 0;
        animator.SetTrigger("Reel");
        robot.transform.position = robotStartPos;
        reelBottom.position = new Vector3(reelBottom.position.x, robot.transform.position.y, reelBottom.position.z);
        AudioSource reelSound = AudioManager.Instance.PlayAudioAtObject("Reel", gameObject, 20, true);
        while (progress != 1)
        {
            if (isReeling)
            {
                progress += reelSpeed * Time.deltaTime;
                progress = Mathf.Clamp(progress, 0, 1);
                Vector3 desiredPosition = Vector3.Lerp(reelBottom.position, reelTop.position, progress);
                robot.transform.position = Vector3.Lerp(robotStartPos, desiredPosition, progress);
                yield return new WaitForFixedUpdate();
            }
        }
        Destroy(reelSound);
        animator.SetInteger("Pies", levelManager.Pies);
        animator.SetTrigger("Eat");
        reelStarted = false;
    }

    public void Grab()
    {
        robot.transform.SetParent(robot.GetComponent<Robot>().startParent);
        robot.transform.localPosition = Vector3.zero;
    }

    public void Eat()
    {
        levelManager.Pies--;
        piesSprite.sprite = levelManager.Pies > 0 ? pieSprites[Mathf.Clamp(levelManager.Pies - 1, 0, pieSprites.Length - 1)] : null;
    }

    public void Hold(int isHeld)
    {
        robotAnimator.SetBool("Held", isHeld == 1);
    }

    public void Load()
    {
        throwScript.isLoaded = true;
        if (levelManager.throwPowerLevel == 1)
        {
            robot.transform.parent = ballistaArm;
        }
    }

    public void Arms(int enabled)
    {
        robot.transform.Find("Arms").gameObject.SetActive(enabled == 1);
    }

    public void Launch()
    {
        levelManager.State = LevelState.launch;
    }

    public void Release()
    {
        levelManager.State = LevelState.fly;
        robot.transform.SetParent(null, true);
        robot.transform.rotation = Quaternion.identity;
        robot.GetComponent<Robot>().Launch(throwVector);
        pauseCoroutine = StartCoroutine(LaunchPause());
    }

    public void End()
    {
        if (!isEnded)
        {
            isEnded = true;
            levelManager.EndLevel();
        }
    }

    private IEnumerator LaunchPause()
    {
        float duration = 0;
        if (levelManager.throwPowerLevel == 0)
        {
            thrownHand.SetActive(true);
            animator.speed = 0;
        }
        while (duration < postThrowPause)
        {
            duration += Time.deltaTime;
            yield return null;
        }
        animator.SetTrigger("Idle");
        animator.speed = 1;
        if (levelManager.throwPowerLevel == 0)
        {
            thrownHand.SetActive(false);
        }
        StartCoroutine(Angle(levelManager.throwPowerLevel == 1 ? 11f : 0f, true));
    }

    private void EditorUpdate()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reel();
        }
    }
}
