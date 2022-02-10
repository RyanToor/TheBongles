using UnityEngine;

public class MapCamera : MonoBehaviour
{
    public GameObject target;
    public float minZoom, maxZoom, startOffset, targetHeightOffset;
    public Vector3 desiredOffset;
    [Range(0.0f, 10.0f)]
    public float offsetLerpSpeed;
    public UpgradeMenu upgradeMenu;

    private Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        desiredOffset = desiredOffset.normalized * startOffset;
        offset = desiredOffset;
    }

    // Update is called once per frame
    void Update()
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
        if (desiredOffset.magnitude == minZoom && GameManager.Instance.storyPoint >= 3 && upgradeMenu.lerpDir == -1)
        {
            upgradeMenu.FlipLerpDir();
        }
    }

    public void ZoomToIsland()
    {
        desiredOffset -= (minZoom - desiredOffset.magnitude) * transform.forward;
    }
}
