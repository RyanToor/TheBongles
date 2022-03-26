using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sonar : MonoBehaviour
{
    public float sonarWaitPeriod, sonarPulsePeriod, sonarRange;

    private Transform player;
    private CircleCollider2D sonarCollider;

    // Start is called before the first frame update
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        sonarCollider = GetComponent<CircleCollider2D>();
        StartCoroutine(SonarWait());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = player.position;
    }

    private IEnumerator SonarWait()
    {
        float duration = 0;
        while (duration < sonarWaitPeriod)
        {
            yield return null;
            duration += Time.deltaTime;
        }
        StartCoroutine(SonarPing());
    }

    private IEnumerator SonarPing()
    {
        float duration = 0;
        while (duration < sonarPulsePeriod)
        {
            sonarCollider.radius = Mathf.Lerp(0, sonarRange, duration / sonarPulsePeriod);
            yield return null;
            duration += Time.deltaTime;
        }
        sonarCollider.radius = 0;
        StartCoroutine(SonarWait());
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.IsTouching(sonarCollider))
        {
            collision.transform.GetChild(0).GetComponent<Animator>().SetTrigger("Blip");
        }
    }
}
