using UnityEngine;

public class FollowCameraVertical : MonoBehaviour
{
    public GameObject target;
    [Range(0, 10)]
    public float lerpSpeed;
    public float minDistance;

    private Vector3 desiredPos;

    // Update is called once per frame
    void Update()
    {
        desiredPos = new Vector3(0, Mathf.Clamp(target.transform.position.y, -Mathf.Infinity, 0),-10f);
        transform.position = Vector3.Lerp(transform.position, desiredPos, lerpSpeed);
        if ((desiredPos - transform.position).magnitude < minDistance)
        {
            transform.position = desiredPos;
        }
    }
}
