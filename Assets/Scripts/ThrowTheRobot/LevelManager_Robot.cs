using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager_Robot : LevelManager
{
    public ThrowInputs throwParameters;
    public int pies;
    public FloatingObjects floatingDecorations, floatingTrash, floatingSurface;

    [HideInInspector]
    public float totalThrowDistance;
    [HideInInspector]
    public int throwPowerLevel;

    private LevelState state;
    private LevelBuilder levelbuilder;
    private ThrowTheRobotUI uI;

    protected override void Awake()
    {
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
        uI = GameObject.Find("Canvas").GetComponent<ThrowTheRobotUI>();
        levelbuilder = GameObject.Find("Level").GetComponent<LevelBuilder>();
        StartCoroutine(CheckLoaded());
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

    private IEnumerator CheckLoaded()
    {
        while (!levelbuilder.isLevelBuilt)
        {
            yield return null;
        }
        Destroy(GameObject.Find("LoadingCanvas(Clone)"));
        ChangeState(0);
        AudioManager.Instance.PlayMusic("Throw the Robot");
    }

    private void ChangeState(LevelState newState)
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<Robot>().SetState(newState);
        switch (newState)
        {
            case LevelState.launch:
                if (pies >= 0)
                {
                    throwParameters.StopAllCoroutines();
                    StartCoroutine(throwParameters.Throw());
                }
                else
                {
                    EndLevel();
                }
                break;
            case LevelState.fly:
                break;
            case LevelState.move:
                break;
            case LevelState.reel:
                GameObject.Find("Bubba").GetComponent<Launcher>().Reel();
                break;
            default:
                break;
        }
    }

    private void EndLevel()
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

    private void EditorUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Return))
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
