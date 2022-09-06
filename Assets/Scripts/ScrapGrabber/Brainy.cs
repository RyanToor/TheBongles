using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brainy : MonoBehaviour
{
    [Range(0, 100)]
    public float focusChance;
    public float focusTime, eyeOffset;
    public Transform eyes, eyePoint, claw;

    private bool isFocussed;
    private float currentEyeOffset;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        currentEyeOffset = eyeOffset;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("RandomChance", Random.Range(0f, 100f));
    }

    private void FixedUpdate()
    {
        Eyes();
    }

    private void Eyes()
    {
        eyes.position = eyePoint.position + (claw.position - eyePoint.position).normalized * currentEyeOffset;
        if (!isFocussed)
        {
            if (Random.Range(0f, 100f) < focusChance)
            {
                StartCoroutine(EyeFocus());
            }
        }
    }

    private IEnumerator EyeFocus()
    {
        isFocussed = true;
        currentEyeOffset = 0;
        float duration = 0;
        while (duration < focusTime)
        {
            duration += Time.deltaTime;
            yield return null;
        }
        currentEyeOffset = eyeOffset;
        isFocussed = false;
    }

    private void MakeSound(string soundName)
    {
        AudioManager.Instance.PlayAudioAtObject(soundName, gameObject, 50, false, AudioRolloffMode.Linear);
    }
}
