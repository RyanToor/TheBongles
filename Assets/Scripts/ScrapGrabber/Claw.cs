using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Claw : MonoBehaviour
{
    public float maxAimAngle, aimSpeed, fireSpeed, turnSpeed, linePointSeparation, maxLineLength, reelSpeed, reelRotateSpeed, fuelTime;
    public int trashCatchLimit;
    public GameObject fuelBar, spotLight;
    public GameObject[] lineLengthIndicators;
    public TrashRequests trashRequestScript;

    private ClawState state;
    private LineRenderer lineRenderer;
    private List<Vector3> linePoints = new List<Vector3>();
    private bool isReleasing;
    private LevelManager_ScrapGrabber levelManager;
    private float fuelBarStartLength, lineLength, lineLengthIndicatorPortion, lightOffset;
    private int currentTrash;

    // Start is called before the first frame update
    void Start()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_ScrapGrabber>();
        linePoints.AddRange(new Vector3[2]{ transform.parent.position, transform.position});
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetPositions(linePoints.ToArray());
        fuelBarStartLength = fuelBar.transform.localScale.x;
        lineLength = (transform.position - transform.parent.position).magnitude;
        lineLengthIndicatorPortion = maxLineLength / lineLengthIndicators.Length;
        lightOffset = (spotLight.transform.position - transform.parent.position).magnitude;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (state)
        {
            case ClawState.aim:
                Aim();
                break;
            case ClawState.fire:
                Fire();
                break;
            case ClawState.reel:
                Reel();
                break;
            default:
                break;
        }
        UpdateInstruments();
        UpdateLight();
    }

    private void Aim()
    {
        transform.RotateAround(transform.parent.position, Vector3.forward, Input.GetAxis("Horizontal") * aimSpeed * Time.deltaTime);
        float currentAngle = Vector3.SignedAngle(transform.position - transform.parent.position, Vector3.down, Vector3.forward);
        if (Mathf.Abs(currentAngle) > maxAimAngle)
        {
            transform.RotateAround(transform.parent.position, Mathf.Sign(currentAngle) > 0? Vector3.forward : Vector3.back, Mathf.Abs(currentAngle) - maxAimAngle);
        }
        if (Input.GetAxis("Jump") > 0)
        {
            state = ClawState.fire;
            linePoints.Insert(linePoints.Count - 1, transform.position);
            isReleasing = true;
        }
        linePoints[linePoints.Count - 1] = transform.position;
        lineRenderer.SetPositions(linePoints.ToArray());
    }

    private void Fire()
    {
        if (isReleasing && Input.GetAxisRaw("Jump") == 0)
        {
            isReleasing = false;
        }
        transform.Rotate(Vector3.forward, Input.GetAxisRaw("Horizontal") * turnSpeed * Time.deltaTime);
        float currentAngle = transform.rotation.eulerAngles.z < 180? transform.rotation.eulerAngles.z : -(360 - transform.rotation.eulerAngles.z);
        if (Mathf.Abs(currentAngle) > maxAimAngle)
        {
            transform.Rotate(Mathf.Sign(currentAngle) > 0 ? Vector3.back : Vector3.forward, Mathf.Abs(currentAngle) - maxAimAngle);
        }
        transform.position += fireSpeed * Time.deltaTime * -transform.up;
        linePoints[linePoints.Count - 1] = transform.position;
        if ((linePoints[linePoints.Count - 1] - linePoints[linePoints.Count - 2]).magnitude > linePointSeparation)
        {
            linePoints.Insert(linePoints.Count - 1, transform.position);
            lineLength += (linePoints[linePoints.Count - 2] - linePoints[linePoints.Count - 3]).magnitude;
        }
        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray());
        if (lineRenderer.positionCount * linePointSeparation > maxLineLength || (Input.GetAxis("Jump") > 0 && !isReleasing))
        {
            currentTrash = 0;
            state = ClawState.reel;
            GetComponent<Animator>().SetBool("Closed", true);
        }
    }

    private void Reel()
    {
        float segmentLength = (linePoints[linePoints.Count - 2] - transform.position).magnitude;
        if (segmentLength < reelSpeed * Time.deltaTime)
        {
            lineLength -= (linePoints[linePoints.Count - 2] - linePoints[linePoints.Count - 3]).magnitude;
            if (linePoints.Count == 3)
            {
                transform.position = linePoints[1];
                state = ClawState.aim;
                GetComponent<Animator>().SetBool("Closed", false);
                StoreTrash();
            }
            else
            {
                transform.position = linePoints[linePoints.Count - 2] + (linePoints[linePoints.Count - 3] - linePoints[linePoints.Count - 2]).normalized * (reelSpeed * Time.deltaTime - segmentLength);
            }
            linePoints.RemoveAt(linePoints.Count - 2);
            linePoints.TrimExcess();
        }
        else
        {
            transform.position += reelSpeed * Time.deltaTime * (linePoints[linePoints.Count - 2] - transform.position).normalized;
        }
        linePoints[linePoints.Count - 1] = transform.position;
        Quaternion desiredRotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.up, linePoints[linePoints.Count - 2] - transform.position, Vector3.forward));
        transform.rotation = state == ClawState.aim? Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.up, linePoints[linePoints.Count - 2] - transform.position, Vector3.forward)) : Quaternion.RotateTowards(transform.rotation, desiredRotation, reelRotateSpeed * Time.deltaTime);
        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray());
    }

    private void StoreTrash()
    {
        List<string> collectedTrash = new List<string>();
        foreach (Transform trash in transform.Find("TrashContainer"))
        {
            if (trash.name == "Fuel")
            {
                levelManager.remainingTime = Mathf.Clamp(levelManager.remainingTime + fuelTime, 0, levelManager.maxTime);
            }
            else
            {
                levelManager.glass++;
                collectedTrash.Add(trash.gameObject.GetComponent<CollectableTrash>().trashName);
            }
            Destroy(trash.gameObject);
        }
        trashRequestScript.CheckCatch(collectedTrash);
    }

    private void UpdateInstruments()
    {
        fuelBar.transform.localScale = new Vector3(Mathf.Clamp(levelManager.remainingTime * fuelBarStartLength / levelManager.maxTime, 0, fuelBarStartLength), fuelBar.transform.localScale.y, 1);
        for (int i = 0; i < lineLengthIndicators.Length; i++)
        {
            lineLengthIndicators[i].SetActive(lineLength < (lineLengthIndicators.Length - i + 1) * lineLengthIndicatorPortion);
        }
    }

    private void UpdateLight()
    {
        spotLight.transform.position = transform.parent.position + (transform.position - transform.parent.position).normalized * lightOffset;
        spotLight.transform.rotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.up, transform.parent.position - transform.position, Vector3.forward));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("RandomTrash"))
        {
            collision.transform.parent = transform.Find("TrashContainer");
            foreach (Transform trash in collision.transform)
            {
                trash.transform.parent = transform.Find("TrashContainer");
            }
            if (state == ClawState.fire)
            {
                currentTrash++;
                if (currentTrash >= trashCatchLimit)
                {
                    state = ClawState.reel;
                    GetComponent<Animator>().SetBool("Closed", true);
                    currentTrash = 0;
                }
            }
            else
            {
                StoreTrash();
            }
        }
        else if (collision.gameObject.name == "Mine")
        {
            currentTrash = 0;
            collision.gameObject.GetComponent<Obstacle>().MineHit();
            if (state == ClawState.fire)
            {
                state = ClawState.reel;
            }
        }
    }

    private enum ClawState
    {
        aim,
        fire,
        reel
    }
}
