using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Claw : MonoBehaviour
{
    public float maxAimAngle, aimSpeed, fireSpeed, turnSpeed, linePointSeparation, maxLineLength, reelSpeed, reelRotateSpeed, fuelTime, secondsFlashPeriod;
    public int trashCatchLimit;
    public GameObject fuelBar, spotLight, freeze, bellAssembly;
    public Color lightSafeColour, lightDangerColour, secondsDangerColour;
    public GameObject[] lineLengthIndicators;
    public TrashRequests trashRequestScript;
    public Vector2[] clawExtensionAndItems;
    public Sprite[] numbers;
    public SpriteRenderer[] seconds;

    [HideInInspector]
    public bool isCaught, isMultiClaw;
    [HideInInspector]
    public ClawState state;

    private LineRenderer lineRenderer;
    private List<Vector3> linePoints = new List<Vector3>();
    private bool isReleasing, isFlashing;
    private LevelManager_ScrapGrabber levelManager;
    private float fuelBarStartLength, lineLength, lineLengthIndicatorPortion, lightOffset, startOffset, dangerTime;
    private int currentTrash, roundedSeconds;
    private Spawner spawner;
    private Color[] normalColour;
    private AudioSource fireSound, reelSound;

    // Start is called before the first frame update
    void Start()
    {
        spawner = GameObject.Find("Spawner").GetComponent<Spawner>();
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_ScrapGrabber>();
        linePoints.AddRange(new Vector3[2]{ transform.parent.position, transform.position});
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetPositions(linePoints.ToArray());
        fuelBarStartLength = fuelBar.transform.localScale.x;
        lineLength = (transform.position - transform.parent.position).magnitude;
        lineLengthIndicatorPortion = (maxLineLength - transform.localPosition.magnitude) / (lineLengthIndicators.Length + 1);
        startOffset = transform.localPosition.magnitude;
        lightOffset = (spotLight.transform.position - transform.parent.position).magnitude;
        dangerTime = levelManager.dangerTime;
        normalColour = new Color[] { seconds[0].color, seconds[1].color };
        for (int i = 0; i < 3; i++)
        {
            if (transform.Find("Upgrade" + (i + 1) + "-" + GameManager.Instance.upgrades[2][i]) != null)
            {
                transform.Find("Upgrade" + (i + 1) + "-" + GameManager.Instance.upgrades[2][i]).gameObject.SetActive(true);
            }
            Upgrade(new Vector2Int(i + 1, GameManager.Instance.upgrades[2][i]));
        }
    }

    // Update is called once per frame
    void Update()
    {
        lineLength = Mathf.Clamp(linePoints.Count - 3, 0, int.MaxValue) * linePointSeparation + (linePoints[1] - linePoints[0]).magnitude + (linePoints[linePoints.Count - 1] - linePoints[linePoints.Count - 2]).magnitude;
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

    private void FixedUpdate()
    {
        roundedSeconds = (int)System.Math.Round(levelManager.remainingTime, 0);
        seconds[0].sprite = numbers[roundedSeconds % 10];
        seconds[1].sprite = numbers[Mathf.FloorToInt(roundedSeconds / 10)];
    }

    private void Aim()
    {
        if (reelSound != null)
        {
            Destroy(reelSound);
        }
        transform.RotateAround(transform.parent.position, Vector3.forward, Input.GetAxisRaw("Horizontal") * aimSpeed * Time.unscaledDeltaTime);
        float currentAngle = Vector3.SignedAngle(transform.position - transform.parent.position, Vector3.down, Vector3.forward);
        if (Mathf.Abs(currentAngle) > maxAimAngle)
        {
            transform.RotateAround(transform.parent.position, Mathf.Sign(currentAngle) > 0? Vector3.forward : Vector3.back, Mathf.Abs(currentAngle) - maxAimAngle);
        }
        if (Input.GetAxisRaw("Jump") > 0)
        {
            state = ClawState.fire;
            fireSound = AudioManager.Instance.PlayAudioAtObject("ClawExtending", gameObject, 20, true);
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
        transform.Rotate(Vector3.forward, Input.GetAxisRaw("Horizontal") * turnSpeed * Time.unscaledDeltaTime);
        float currentAngle = transform.rotation.eulerAngles.z < 180? transform.rotation.eulerAngles.z : -(360 - transform.rotation.eulerAngles.z);
        if (Mathf.Abs(currentAngle) > maxAimAngle)
        {
            transform.Rotate(Mathf.Sign(currentAngle) > 0 ? Vector3.back : Vector3.forward, Mathf.Abs(currentAngle) - maxAimAngle);
        }
        transform.position += (Time.timeScale != 0? 1 / Time.timeScale : 1) * fireSpeed * Time.deltaTime * -transform.up;
        linePoints[linePoints.Count - 1] = transform.position;
        if ((transform.position - linePoints[linePoints.Count - 2]).magnitude > linePointSeparation)
        {
            for (float distanceToCover = (transform.position - linePoints[linePoints.Count - 2]).magnitude; distanceToCover > linePointSeparation; distanceToCover -= linePointSeparation)
            {
                linePoints.Insert(linePoints.Count - 1, linePoints[linePoints.Count - 2] + (transform.position - linePoints[linePoints.Count - 2]).normalized * linePointSeparation);
            }
        }
        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray());
        if ((lineLength > maxLineLength || (Input.GetAxis("Jump") > 0 && !isReleasing)) && !isCaught)
        {
            currentTrash = 0;
            state = ClawState.reel;
            if (fireSound != null)
            {
                Destroy(fireSound);
            }
            reelSound = AudioManager.Instance.PlayAudioAtObject("ClawRetracting", gameObject, 20, true);
            GetComponent<Animator>().SetBool("Closed", true);
        }
    }

    private void Reel()
    {
        int checkPoint = linePoints.Count;
        int pointsToRemove = 0;
        float remainder = 0;
        for (float moveDistance = reelSpeed * Time.unscaledDeltaTime; moveDistance > 0; moveDistance -= Vector3.Distance(linePoints[Mathf.Clamp(checkPoint - 1, 1, linePoints.Count - 2)], linePoints[Mathf.Clamp(checkPoint, 0, linePoints.Count - 1)]))
        {
            remainder = Vector3.Distance(linePoints[Mathf.Clamp(checkPoint - 1, 0, int.MaxValue)], linePoints[Mathf.Clamp(checkPoint - 2, 0, int.MaxValue)]) - moveDistance;
            if (remainder < 0)
            {
                pointsToRemove++;
                checkPoint--;
            }
        }
        if (pointsToRemove > 0)
        {
            if (linePoints.Count - 1 - pointsToRemove < 2)
            {
                transform.position = linePoints[1];
                state = ClawState.aim;
                GetComponent<Animator>().SetBool("Closed", false);
                if (!levelManager.gameEnded)
                {
                    StoreTrash();
                }
                for (int i = linePoints.Count - 1; i > 1; i--)
                {
                    linePoints.RemoveAt(i);
                    linePoints.TrimExcess();
                }
            }
            else
            {
                transform.position = linePoints[linePoints.Count - 2 - pointsToRemove] + (linePoints[linePoints.Count - 1 - pointsToRemove] - linePoints[linePoints.Count - 2 - pointsToRemove]).normalized * remainder;
                for (int i = 0; i < pointsToRemove; i++)
                {
                    linePoints.RemoveAt(linePoints.Count - 2);
                    linePoints.TrimExcess();
                }
            }
        }
        else
        {
            transform.position += reelSpeed * Time.unscaledDeltaTime * (linePoints[linePoints.Count - 2] - transform.position).normalized;
        }
        linePoints[linePoints.Count - 1] = transform.position;
        float desiredClawAngle = Vector3.SignedAngle(Vector3.up, linePoints[linePoints.Count - 2] - transform.position, Vector3.forward);
        Quaternion desiredRotation = desiredClawAngle == 0? Quaternion.identity : Quaternion.Euler(0, 0, desiredClawAngle);
        transform.rotation = state == ClawState.aim? desiredRotation : Quaternion.RotateTowards(transform.rotation, desiredRotation, reelRotateSpeed * Time.unscaledDeltaTime);
        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray());
    }

    private void StoreTrash()
    {
        List<string> collectedTrash = new List<string>();
        if (transform.Find("TrashContainer").childCount > 0)
        {
            levelManager.brainy.SetBool("Happy", true);
        }
        foreach (Transform trash in transform.Find("TrashContainer"))
        {
            if (trash.name == "Fuel(Clone)" || trash.name == "Fuel")
            {
                levelManager.remainingTime = Mathf.Clamp(levelManager.remainingTime + fuelTime, 0, levelManager.maxTime);
                AudioManager.Instance.PlayAudioAtObject("Fuel", gameObject, 20, false);
            }
            else
            {
                levelManager.glass++;
                AudioManager.Instance.PlayAudioAtObject("Glass", gameObject, 20, false);
                collectedTrash.Add(trash.gameObject.GetComponent<CollectableTrash>().trashName);
                GameManager.Instance.SpawnCollectionIndicator(trash.position, levelManager.collectionIndicatorColor);
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
            lineLengthIndicators[i].SetActive(lineLength <= startOffset + (lineLengthIndicators.Length - i) * lineLengthIndicatorPortion);
        }
        if (0 < roundedSeconds && roundedSeconds < dangerTime && !isFlashing)
        {
            StartCoroutine(Flash());
        }
    }

    private IEnumerator Flash()
    {
        isFlashing = true;
        levelManager.brainy.SetBool("Stress", true);
        float duration = 0;
        AudioSource CountdownSource = AudioManager.Instance.PlayAudioAtObject("Countdown", gameObject, 20, true);
        while (levelManager.remainingTime < dangerTime && levelManager.remainingTime > 0)
        {
            for (int i = 0; i < seconds.Length; i++)
            {
                seconds[i].color = duration % secondsFlashPeriod < secondsFlashPeriod / 2 ? secondsDangerColour : normalColour[i];
            }
            duration += Time.deltaTime;
            yield return null;
        }
        Destroy(CountdownSource);
        isFlashing = false;
        if (!isCaught)
        {
            levelManager.brainy.SetBool("Stress", false);
        }
        for (int i = 0; i < seconds.Length; i++)
        {
            if (levelManager.remainingTime > 0)
            {
                seconds[i].color = normalColour[i];
            }
            else
            {
                seconds[i].color = secondsDangerColour;
            }
        }
    }

    private void UpdateLight()
    {
        spotLight.transform.SetPositionAndRotation(transform.parent.position + (transform.position - transform.parent.position).normalized * lightOffset, Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.up, transform.parent.position - transform.position, Vector3.forward)));
        if (levelManager.remainingTime < levelManager.dangerTime)
        {
            spotLight.GetComponent<Light2D>().color = lightDangerColour;
        }
        else
        {
            spotLight.GetComponent<Light2D>().color = lightSafeColour;
        }
    }

    private void Upgrade(Vector2Int upgradeNumber)
    {
        switch (upgradeNumber.x)
        {
            case 1:
                if (upgradeNumber.y > 0)
                {
                    maxLineLength += clawExtensionAndItems[upgradeNumber.y - 1].x;
                    trashCatchLimit = (int)(clawExtensionAndItems[upgradeNumber.y - 1].y);
                    GetComponent<Animator>().SetInteger("ClawLevel", upgradeNumber.y);
                    if (upgradeNumber.y > 1)
                    {
                        isMultiClaw = true;
                    }
                }
                break;
            case 2:
                if (upgradeNumber.y > 0)
                {
                    levelManager.isBellEnabled = true;
                    levelManager.bellAnimator.SetBool("Available", true);
                    levelManager.bellPromptText.SetActive(true);
                }
                else
                {
                    bellAssembly.SetActive(false);
                    levelManager.bellPromptText.SetActive(false);
                }
                break;
            case 3:
                if (upgradeNumber.y > 0)
                {
                    levelManager.isFreezeEnabled = true;
                }
                else
                {
                    freeze.SetActive(false);
                }
                break;
            default:
                break;
        }
    }

    public bool IsCaught
    {
        get { return isCaught; }
        set
        {
            isCaught = value;
            if (value == true)
            {
                levelManager.brainy.SetBool("Stress", true);
            }
            else
            {
                if (!isFlashing)
                {
                    levelManager.brainy.SetBool("Stress", false);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("RandomTrash") && state == ClawState.fire)
        {
            spawner.objectsToRemove.Add(collision.gameObject);
            collision.transform.SetParent(transform.Find("TrashContainer"), true);
            foreach (Transform trash in collision.transform)
            {
                trash.transform.SetParent(transform.Find("TrashContainer"), true);
            }
            if (state == ClawState.fire)
            {
                currentTrash++;
                if (currentTrash >= trashCatchLimit)
                {
                    state = ClawState.reel;
                    if (fireSound != null)
                    {
                        Destroy(fireSound);
                    }
                    reelSound = AudioManager.Instance.PlayAudioAtObject("ClawRetracting", gameObject, 20, true);
                    GetComponent<Animator>().SetBool("Closed", true);
                    currentTrash = 0;
                }
            }
            else
            {
                StoreTrash();
            }
        }
        else if (collision.CompareTag("Boss"))
        {
            if (state == ClawState.fire)
            {
                state = ClawState.reel;
                if (fireSound != null)
                {
                    Destroy(fireSound);
                }
                reelSound = AudioManager.Instance.PlayAudioAtObject("ClawRetracting", gameObject, 20, true);
            }
            if (collision.gameObject.name == "Electric_Eel(Clone)")
            {
                StartCoroutine(levelManager.LightsOut(collision.gameObject));
                StartCoroutine(levelManager.BrainyShock());
            }
            spawner.Escape(spawner.spawnedObjects.IndexOf(collision.gameObject));
            levelManager.brainy.SetBool("Sad", true);
        }
        else if (collision.gameObject.CompareTag("Emergency"))
        {
            currentTrash = 0;
            collision.gameObject.GetComponent<Obstacle>().MineHit();
            AudioManager.Instance.PlayAudioAtObject("Mine", gameObject, 20, false);
            if (state == ClawState.fire)
            {
                state = ClawState.reel;
                if (fireSound != null)
                {
                    Destroy(fireSound);
                }
                reelSound = AudioManager.Instance.PlayAudioAtObject("ClawRetracting", gameObject, 20, true);
            }
            levelManager.brainy.SetBool("Sad", true);
        }
        else if (collision.gameObject.name == "SharkMouth" && state == ClawState.fire)
        {
            StartCoroutine(collision.transform.parent.GetComponent<Obstacle>().SharkGrab(this));
            AudioManager.Instance.PlayAudioAtObject("Shark", gameObject, 20, false);
        }
    }

    public enum ClawState
    {
        aim,
        fire,
        reel
    }
}
