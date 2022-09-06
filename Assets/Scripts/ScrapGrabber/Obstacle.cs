using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Obstacle : MonoBehaviour
{
    public float mineRadius, sharkDragPeriod;
    public AnimationCurve mineBlast;
    public float mineAnimationLength;
    [Header("Vibration Data")]
    [SerializeField] float mineIntensity;
    [SerializeField] float sharkIntensity;

    [HideInInspector]
    public Spawner spawner;

    private bool hit;

    public void MineHit()
    {
        if (!hit)
        {
            hit = true;
            AudioManager.Instance.PlayAudioAtObject("Mine", gameObject, 50, false, AudioRolloffMode.Linear);
            GetComponent<Animator>().SetTrigger("Hit");
            StartCoroutine(LightPulse(mineBlast, mineAnimationLength));
            Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, mineRadius);
            foreach (Collider2D collider in nearbyObjects)
            {
                if (collider.CompareTag("RandomTrash"))
                {
                    if (collider.transform.parent.name == "Spawner")
                    {
                        spawner.destroyRequests.Add(collider.gameObject);
                    }
                    else
                    {
                        Destroy(collider.gameObject);
                    }
                }
                else if (collider.CompareTag("Boss"))
                {
                    spawner.Escape(spawner.spawnedObjects.IndexOf(collider.gameObject));
                }
                else if (collider.CompareTag("Emergency"))
                {
                    collider.gameObject.GetComponent<Obstacle>().MineHit();
                }
            }
        }
    }

    private IEnumerator LightPulse(AnimationCurve curve, float duration)
    {
        UnityEngine.Rendering.Universal.Light2D light = GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        InputManager.Instance.Vibrate(mineIntensity, mineAnimationLength, mineBlast);
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

    public IEnumerator SharkGrab(Claw clawScript)
    {
        GetComponent<Animator>().SetBool("Drag", true);
        AudioManager.Instance.PlayAudioAtObject("Shark", gameObject, 50, false, AudioRolloffMode.Linear);
        clawScript.IsCaught = true;
        InputManager.VibrationData grabVibration = InputManager.Instance.Vibrate(sharkIntensity);
        Transform catchPoint = transform.Find("CatchPoint");
        float duration = 0;
        while (duration < sharkDragPeriod && Mathf.Abs(transform.position.x) < 9.6)
        {
            clawScript.transform.position = catchPoint.position;
            duration += Time.deltaTime;
            yield return null;
        }
        GetComponent<Animator>().SetBool("Drag", false);
        InputManager.Instance.vibrations.Remove(grabVibration);
        clawScript.IsCaught = false;
    }

    public void RequestDestruction()
    {
        if (transform.parent.name == "Spawner")
        {
            spawner.destroyRequests.Add(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void FlipSprite()
    {
        GetComponent<SpriteRenderer>().flipX = !GetComponent<SpriteRenderer>().flipX;
    }
}
