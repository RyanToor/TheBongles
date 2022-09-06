using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager_Robot : LevelManager
{
    public ThrowInputs throwParameters;
    public int pies;
    public FloatingObjects floatingDecorations, floatingTrash, floatingSurface;
    public GameObject ballista, cannon, dock;

    [HideInInspector]
    public float totalThrowDistance;
    [HideInInspector]
    public int throwPowerLevel;

    private LevelState state;
    private LevelBuilder levelBuilder;
    private ThrowTheRobotUI uI;

    protected override void Awake()
    {
        base.Awake();
        if (!Application.isEditor)
        {
            floatingDecorations.enabled = true;
            floatingTrash.enabled = true;
            floatingSurface.enabled = true;
        }
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        uI = GameObject.Find("Canvas").GetComponent<ThrowTheRobotUI>();
        levelBuilder = GameObject.Find("Level").GetComponent<LevelBuilder>();
        throwParameters.ResetDials();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (Application.isEditor)
        {
            EditorUpdate();
        }
    }

    public int Pies
    {
        get
        {
            return pies;
        }
        set
        {
            pies = value;
            GameObject.Find("Bubba").GetComponent<Launcher>().UpdatePies();
        }
    }

    protected override IEnumerator CheckLoaded()
    {
        while (levelBuilder == null || !levelBuilder.isLevelBuilt)
        {
            yield return null;
        }
        ChangeState(0);
        GameObject.Find("Bubba").GetComponent<Animator>().enabled = true;
        StartCoroutine(base.CheckLoaded());
    }

    private void ChangeState(LevelState newState)
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<Robot>().SetState(newState);
        switch (newState)
        {
            case LevelState.launch:
                throwParameters.StopAllCoroutines();
                StartCoroutine(throwParameters.Throw());
                break;
            case LevelState.fly:
                AudioManager.Instance.PlayMusicWithFade("Throw the Robot Fly");
                break;
            case LevelState.move:
                AudioManager.Instance.PlayMusicWithFade("Throw the Robot Platform");
                break;
            case LevelState.reel:
                GameObject.Find("Bubba").GetComponent<Launcher>().Reel();
                AudioManager.Instance.PlayMusicWithFade("Throw the Robot Bubba");
                break;
            default:
                break;
        }
    }

    public void EndLevel()
    {
        uI.EndGame();
        AudioManager.Instance.PlaySFX("GameOver-ThrowTheRobot");
    }

    public LevelState State
    {
        get
        {
            return state;
        }
        set
        {
            if (state == value)
            {
                return;
            }
            state = value;
            ChangeState(value);
        }
    }

    public int ThrowPowerLevel
    {
        get
        {
            return throwPowerLevel;
        }
        set
        {
            throwPowerLevel = value;
            dock.SetActive(value > 0);
            GameObject.Find("Bubba").transform.Find("Lever").gameObject.SetActive(value > 0);
            ballista.SetActive(value == 1);
            cannon.SetActive(value > 1);
            GameObject.Find("Bubba").GetComponent<Animator>().SetInteger("LaunchUpgradeLevel", value);
        }
    }

    private void EditorUpdate()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard.enterKey.wasPressedThisFrame)
        {
            ChangeState(LevelState.launch);
        }
    }
}

public enum LevelState
{
    launch,
    fly,
    move,
    reel
}
