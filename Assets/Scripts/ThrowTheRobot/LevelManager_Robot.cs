using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager_Robot : LevelManager
{
    public bool isLaunched;
    public ThrowInputs throwParameters;
    public float drawDistance, drawPause;

    private GameObject robot;
    private LevelBuilder levelbuilder;

    // Start is called before the first frame update
    protected override void Start()
    {
        robot = GameObject.FindGameObjectWithTag("Player");
        levelbuilder = GameObject.Find("Level").GetComponent<LevelBuilder>();
        StartCoroutine(CheckLoaded());
        throwParameters.ResetDials();
        Throw();
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (Application.isEditor)
        {
            EditorUpdate();
        }
    }

    private void Throw()
    {
        GetThrowInputs(throwValues => 
        {
            StartCoroutine(DrawAndHold(throwValues));
        });
    }

    private void GetThrowInputs(System.Action<Vector2> callback)
    {
        Vector2 throwValues = Vector2.zero;
        StartCoroutine(throwParameters.Spin("angle", valueCollected => 
        {
            Debug.Log("Angle Collected : " + valueCollected);
            throwValues.x = valueCollected;
            StartCoroutine(throwParameters.Spin("power", valueCollected => 
            {
                Debug.Log("Power Collected : " + valueCollected);
                throwValues.y = valueCollected;
                callback(throwValues);
            }));
        }));
    }

    private IEnumerator DrawAndHold(Vector2 powerAngle)
    {
        float duration = 0;
        Vector3 startPoint = robot.transform.position;
        Vector3 throwVector = powerAngle.y * new Vector2(Mathf.Cos(Mathf.Deg2Rad * powerAngle.x), Mathf.Sin(Mathf.Deg2Rad * powerAngle.x));
        Vector3 drawPoint = startPoint - powerAngle.y / throwParameters.powerMax * drawDistance * throwVector.normalized;
        while (robot.transform.position != drawPoint)
        {
            robot.transform.position = Vector3.Lerp(startPoint, drawPoint, duration);
            duration += Time.deltaTime;
            yield return null;
        }
        duration = 0;
        while (duration < drawPause)
        {
            yield return null;
            duration += Time.deltaTime;
        }
        robot.GetComponent<Robot>().Launch(throwVector);
        isLaunched = true;
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

    private void EditorUpdate()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Throw();
        }
    }
}
