using System.Collections;
using UnityEngine;

public class ThrowInputs : MonoBehaviour
{
    public GameObject powerWheel, powerPointer, anglePointer;
    public float powerMin, powerMax, angleMin, angleMax, powerSpeedMin, powerSpeedMax, angleSpeedMin, angleSpeedMax;

    public float power, angle;

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
            pointer.transform.Rotate(speed * spinDir * Vector3.back * Time.deltaTime);
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