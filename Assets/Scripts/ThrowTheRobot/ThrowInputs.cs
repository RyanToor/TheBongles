using System.Collections;
using UnityEngine;

public class ThrowInputs : MonoBehaviour
{
    public GameObject powerWheel, powerPointer, anglePointer;
    public float powerMin, powerMax, angleMin, angleMax, powerSpeedMin, powerSpeedMax, angleSpeedMin, angleSpeedMax;

    private bool valueCollected;
    public float power, powerAngle, angle;

    public void ResetDials()
    {
        powerWheel.transform.localRotation = Quaternion.Euler(Vector3.forward * Random.Range(0, 360));
        powerPointer.transform.localRotation = Quaternion.Euler(Vector3.forward * Random.Range(0, 360));
        anglePointer.transform.localRotation = Quaternion.Euler(Vector3.forward * Random.Range(-45, 45));
    }

    public void GenerateThrowData()
    {
        StartCoroutine(GetThrowInputs());
    }

    public IEnumerator GetThrowInputs()
    {
        valueCollected = false;
        StartCoroutine(Spin(anglePointer, Random.Range(powerSpeedMin, powerSpeedMax)));
        while (!valueCollected)
        {
            yield return null;
        }
        valueCollected = false;
        while (Input.GetAxis("Jump") == 1)
        {
            yield return null;
        }
        StartCoroutine(Spin(powerPointer, Random.Range(angleSpeedMin, angleSpeedMax)));
        while (!valueCollected)
        {
            yield return null;
        }
    }

    private IEnumerator Spin(GameObject pointer, float speed)
    {
        int spinDir = 1;
        while (!valueCollected)
        {
            if (Input.GetAxis("Jump") == 1)
            {
                if (pointer == powerPointer)
                {
                    print(Mathf.DeltaAngle(pointer.transform.rotation.eulerAngles.z, powerWheel.transform.rotation.eulerAngles.z));
                    power = Mathf.Lerp(powerMax, powerMin, Mathf.Abs(Mathf.DeltaAngle(pointer.transform.rotation.eulerAngles.z, powerWheel.transform.rotation.eulerAngles.z)) / 180);
                }
                else
                {
                    if (pointer.transform.rotation.eulerAngles.z < 180)
                    {
                        angle = 45 + Mathf.Clamp(pointer.transform.rotation.eulerAngles.z, 0, 45);
                    }
                    else
                    {
                        angle = 45 - Mathf.Clamp(360 - pointer.transform.rotation.eulerAngles.z, 0, 45);
                    }
                }
                valueCollected = true;
                break;
            }
            pointer.transform.Rotate(speed * spinDir * Vector3.back);
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
