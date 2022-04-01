using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Obstacle : MonoBehaviour
{
    public float mineRadius;
    public AnimationCurve mineBlast;
    public float mineAnimationLength;

    [HideInInspector]
    public Spawner spawner;

    public void MineHit()
    {
        GetComponent<Animator>().SetTrigger("Hit");
        StartCoroutine(LightPulse(mineBlast, mineAnimationLength));
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, mineRadius);
        foreach (Collider2D collider in nearbyObjects)
        {
            if (collider.CompareTag("RandomTrash"))
            {
                if (collider.transform.parent.name == "TrashContainer")
                {
                    Destroy(collider.gameObject);
                }
                else
                {
                    spawner.destroyRequests.Add(collider.gameObject);
                }
            }
        }
    }

    private IEnumerator LightPulse(AnimationCurve curve, float duration)
    {
        Light2D light = GetComponent<Light2D>();
        float lightMaxIntensity = light.intensity;
        light.enabled = true;
        float currentTime = 0;
        while (currentTime < duration)
        {
            light.intensity = curve.Evaluate(currentTime / duration) * lightMaxIntensity;
            currentTime += Time.deltaTime;
            yield return null;
        }
        light.intensity = lightMaxIntensity;
        light.enabled = false;
    }

    public void RequestDestruction()
    {
        GameObject.Find("Spawner").GetComponent<Spawner>().destroyRequests.Add(gameObject);
    }

    public void FlipSprite()
    {
        GetComponent<SpriteRenderer>().flipX = !GetComponent<SpriteRenderer>().flipX;
    }
}
