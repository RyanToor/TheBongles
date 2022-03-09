using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager_Robot : LevelManager
{
    public ThrowInputs throwParameters;
    public SpriteRenderer piesSprite;
    public Sprite[] pieSprites;
    public int pies;
    public FloatingObjects floatingDecorations, floatingTrash, floatingSurface;

    [HideInInspector]
    public float totalThrowDistance;

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
        ChangeState(0);
        Pies = pies;
    }

    // Update is called once per frame
    protected override void Update()
    {
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
        AudioManager.instance.PlayMusic("Throw the Robot");
    }

    private void ChangeState(LevelState newState)
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<Robot>().SetState(newState);
        switch (newState)
        {
            case LevelState.launch:
                if (pies > 0)
                {
                    Pies--;
                    throwParameters.StopAllCoroutines();
                    throwParameters.Throw();
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
                GameObject.Find("Launcher").GetComponent<Launcher>().Reel();
                break;
            default:
                break;
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
            piesSprite.sprite = pies > 0? pieSprites[Mathf.Clamp(value - 1, 0, pieSprites.Length - 1)] : null;
        }
    }

    private void EndLevel()
    {
        uI.EndGame();
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
