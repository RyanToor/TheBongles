using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager_Robot : LevelManager
{
    public bool isLaunched;
    public ThrowInputs throwParameters;

    // Start is called before the first frame update
    protected override void Start()
    {
        throwParameters.ResetDials();
        throwParameters.GenerateThrowData();
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            throwParameters.GenerateThrowData();
        }
    }
}
