using System.Collections;
using UnityEngine;

public class ThrowInputs : MonoBehaviour

{
    public LevelManager_Robot levelManager;
    public GameObject robot, powerWheel, powerPointer, anglePointer;
    public float drawDistance, drawPause, powerMin, powerMax, angleMin, angleMax, powerSpeedMin, powerSpeedMax, angleSpeedMin, angleSpeedMax;

    [HideInInspector]
    public float power, angle;

    private void Awake()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Robot>();
        robot = GameObject.FindGameObjectWithTag("Player");
    }

    public void Throw()
    {
        GetThrowInputs(throwValues =>
        {
            StartCoroutine(DrawAndHold(throwValues));
        });
    }

    private void GetThrowInputs(System.Action<Vector2> callback)
    {
        Vector2 throwValues = Vector2.zero;
        StartCoroutine(Spin("angle", valueCollected =>
        {
            Debug.Log("Angle Collected : " + valueCollected);
            throwValues.x = valueCollected;
            StartCoroutine(Spin("power", valueCollected =>
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
        Vector3 drawPoint = startPoint - powerAngle.y / powerMax * drawDistance * throwVector.normalized;
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
        AudioManager.instance.PlaySFX("Throw");
        robot.GetComponent<Robot>().Launch(throwVector);
        levelManager.State = LevelState.fly;
    }

    public void ResetDials()
    {
        powerWheel.transform.localRotation = Quaternion.Euler(Vector3.forward * Random.Range(0, 360));
        powerPointer.transform.localRotation = Quaternion.Euler(Vector3.forward * Random.Range(0, 360));
        anglePointer.transform.localRotation = Quaternion.Euler(Vector3.forward * Random.Range(-45, 45));
    }

    public IEnumerator Spin(string type, System.Action<float> callback)
    {
        float speed = type == "power" ? Random.Range(powerSpeedMin, powerSpeedMax) : Random.Range(angleSpeedMin, angleSpeedMax);
        GameObject pointer = type == "power" ? powerPointer : anglePointer;
        int spinDir = 1;
        while (true)
        {
            if (Input.GetAxis("Jump") == 1)
            {
                while (Input.GetAxis("Jump") == 1)
                {
                    yield return null;
                }
                if (pointer == powerPointer)
                {
                    callback(Mathf.Lerp(powerMax, powerMin, Mathf.Abs(Mathf.DeltaAngle(pointer.transform.rotation.eulerAngles.z, powerWheel.transform.rotation.eulerAngles.z)) / 180));
                }
                else
                {
                    if (pointer.transform.rotation.eulerAngles.z < 180)
                    {
                        callback(45 + Mathf.Clamp(pointer.transform.rotation.eulerAngles.z, 0, 45));
                    }
                    else
                    {
                        callback(45 - Mathf.Clamp(360 - pointer.transform.rotation.eulerAngles.z, 0, 45));
                    }
                }
                break;
            }
            pointer.transform.Rotate(speed * spinDir * Time.deltaTime * Vector3.back);
            if (pointer == anglePointer)
            {
                if (pointer.transform.rotation.eulerAngles.z < 180 && pointer.transform.rotation.eulerAngles.z > 45 - (90 - angleMax))
                {
                    spinDir = 1;
                }
                else if (pointer.transform.rotation.eulerAngles.z > 180 && pointer.transform.eulerAngles.z < 315 + angleMin)
                {
                    spinDir = -1;
                }
            }
            yield return null;
        }
    }
}