using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InputPrompts : MonoBehaviour
{
    public float inputDelayWindow, fadeTime;

    private bool startPrompted;
    private Image storyPromptImage, videoPromptImage;

    // Start is called before the first frame update
    void Start()
    {
        storyPromptImage = transform.Find("StartPrompt").GetComponent<Image>();
        videoPromptImage = transform.Find("VideoPrompt").GetComponent<Image>();
    }

    public void StartPrompt()
    {
        if (!startPrompted)
        {
            StartCoroutine(InputDelay(storyPromptImage));
            startPrompted = true;
        }
    }

    public void VideoPrompt()
    {
        if (!videoPromptImage.gameObject.activeInHierarchy)
        {
            StartCoroutine(InputDelay(videoPromptImage));
            StartCoroutine(ConfirmInput(videoPromptImage));
        }
    }

    private IEnumerator InputDelay(Image image)
    {
        float t = 0;
        while (t < inputDelayWindow)
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                yield break;
            }
            t += Time.deltaTime;
            yield return null;
        }
        StartCoroutine(Fade(image, 1, fadeTime));
    }

    private IEnumerator Fade(Image image, int direction, float delay)
    {
        if (direction == 1)
        {
            image.gameObject.SetActive(true);
        }
        float t = 0;
        float startOpacity = image.color.a;
        float duration = Mathf.Abs(direction - startOpacity) * delay;
        while (t < duration)
        {
            image.color = new Color(1, 1, 1, startOpacity + direction * t / duration);
            t = Mathf.Clamp(t + Time.deltaTime, 0, duration);
            yield return null;
        }
        if (direction == -1)
        {
            image.gameObject.SetActive(false);
        }
    }

    private IEnumerator ConfirmInput(Image image)
    {
        while (true)
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0 || Input.GetAxis("Jump") == 1 || Input.GetMouseButtonDown(0))
            {
                StopAllCoroutines();
                StartCoroutine(Fade(image, -1, fadeTime));
                yield break;
            }
            yield return null;
        }
    }
}
