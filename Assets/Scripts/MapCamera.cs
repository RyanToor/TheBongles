using UnityEngine;

public class MapCamera : MonoBehaviour
{
    public GameObject target;
    public float minZoom, maxZoom;
    private Vector3 offset, desiredOffset, targetStartPos;
    [Range(0.0f, 10.0f)]
    public float offsetLerpSpeed;
    // Start is called before the first frame update
    void Start()
    {
        targetStartPos = target.transform.position;
        desiredOffset = gameObject.transform.position - targetStartPos;
        offset = desiredOffset;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseScroll = Input.mouseScrollDelta.y;
        Vector3 targetPos = target.transform.position;
        desiredOffset -= mouseScroll * desiredOffset.normalized;
        if (offset != desiredOffset)
        {
            if (desiredOffset.magnitude < minZoom)
            {
                desiredOffset += (minZoom - desiredOffset.magnitude) * desiredOffset.normalized;
            }
            else if (desiredOffset.magnitude > maxZoom)
            {
                desiredOffset += (maxZoom - desiredOffset.magnitude) * desiredOffset.normalized;
            }
            offset = Vector3.Lerp(offset, desiredOffset, offsetLerpSpeed * Time.deltaTime);
            if ((offset - desiredOffset).magnitude < 0.05)
            {
                offset = desiredOffset;
            }
        }
        transform.position = targetPos + offset;
    }
}
