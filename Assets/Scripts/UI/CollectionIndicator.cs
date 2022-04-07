using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CollectionIndicator : MonoBehaviour
{
    public Vector3 startPos;
    public string text;
    public float maxAngle, maxTravelDistance, speed;
    public Color textColor;
    public AnimationCurve fadeout;

    void Start()
    {
        transform.SetPositionAndRotation(Camera.main.WorldToScreenPoint(startPos), Quaternion.Euler(0, 0, Random.Range(-maxAngle, maxAngle)));
        if (text != "")
        {
            GetComponent<Text>().text = text;
        }
        StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        Vector3 endPos = startPos + transform.TransformDirection(Vector3.up) * (maxTravelDistance + 1);
        Vector3 currentPos = startPos;
        while (Vector3.Distance(currentPos, endPos) > 1f)
        {
            currentPos = Vector3.Lerp(currentPos, endPos, Time.unscaledDeltaTime * speed);
            transform.position = Camera.main.WorldToScreenPoint(currentPos);
            GetComponent<Text>().color = Color.Lerp(new Color(textColor.r, textColor.g, textColor.b, 0), textColor, fadeout.Evaluate(1 - Vector3.Distance(currentPos, endPos) / maxTravelDistance));
            yield return null;
        }
        Destroy(gameObject);
    }
}
