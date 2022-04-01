using System.Collections;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    public GameObject target;
    public float minZoom, maxZoom, startOffset, targetHeightOffset;
    public Vector3 desiredOffset;
    [Range(0.0f, 10.0f)]
    public float offsetLerpSpeed, zoomToMapDuration = 2;
    public UpgradeMenu upgradeMenu;

    private Vector3 offset;
    private bool upgradesPoppedOut, isStarted;
    private Vector3 initialOffset;

    // Start is called before the first frame update
    void Start()
    {
        desiredOffset = desiredOffset.normalized * startOffset;
        initialOffset = transform.position - target.transform.position;
        if (GameManager.Instance.gameStarted)
        {
            offset = desiredOffset;
            transform.rotation = Quaternion.Euler(new Vector3(-60, 0, 0));
            isStarted = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isStarted)
        {
            float mouseScroll = Input.mouseScrollDelta.y;
            Vector3 targetPos = target.transform.position + target.transform.up * targetHeightOffset;
            desiredOffset += mouseScroll * transform.forward;
            if (offset != desiredOffset)
            {
                if (desiredOffset.magnitude < minZoom)
                {
                    desiredOffset -= (minZoom - desiredOffset.magnitude) * transform.forward;
                }
                else if (desiredOffset.magnitude > maxZoom)
                {
                    desiredOffset -= (maxZoom - desiredOffset.magnitude) * transform.forward;
                }
                offset = Vector3.Lerp(offset, desiredOffset, offsetLerpSpeed * Time.deltaTime);
                if ((offset - desiredOffset).magnitude < 0.05)
                {
                    offset = desiredOffset;
                }
            }
            transform.position = targetPos + offset;
            if (desiredOffset.magnitude == minZoom && GameManager.Instance.storyPoint >= 3)
            {
                if (upgradeMenu.lerpDir == -1 && !upgradesPoppedOut)
                {
                    upgradeMenu.FlipLerpDir();
                }
                upgradesPoppedOut = true;
            }
            else if (upgradesPoppedOut)
            {
                upgradesPoppedOut = false;
            }
        }
    }

    public IEnumerator ZoomToMap()
    {
        float duration = 0;
        Vector3 targetPos = target.transform.position + target.transform.up * targetHeightOffset;
        Quaternion startRotation = transform.rotation;
        while (offset.magnitude != startOffset)
        {
            offset = Vector3.Lerp(initialOffset, desiredOffset, duration / zoomToMapDuration);
            transform.SetPositionAndRotation(targetPos + offset, Quaternion.Lerp(startRotation, Quaternion.Euler(new Vector3(-60, 0, 0)), duration / zoomToMapDuration));
            duration += Time.deltaTime;
            yield return null;
        }
        isStarted = true;
    }

    public void ZoomToIsland()
    {
        desiredOffset -= (minZoom - desiredOffset.magnitude) * transform.forward;
    }
}
