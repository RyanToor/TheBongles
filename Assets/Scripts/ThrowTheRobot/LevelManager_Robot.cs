using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager_Robot : LevelManager
{
    public ThrowInputs throwParameters;
    public int pies;
    public FloatingObjects floatingDecorations, floatingTrash;

    [HideInInspector]
    LevelState state;

    private LevelBuilder levelbuilder;

    // Start is called before the first frame update
    protected override void Start()
    {
        levelbuilder = GameObject.Find("Level").GetComponent<LevelBuilder>();
        StartCoroutine(CheckLoaded());
        throwParameters.ResetDials();
        ChangeState(0);
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
        AudioManager.instance.PlayMusic("Trash Hunt");
    }

    public void ChangeState(LevelState newState)
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<Robot>().SetState(newState);
        switch (newState)
        {
            case LevelState.launch:
                if (pies > 0)
                {
                    pies--;
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
                break;
            default:
                break;
        }
    }

    private void EndLevel()
    {
        pies = 3;
        ChangeState(LevelState.launch);
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
