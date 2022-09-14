using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PromptManager : MonoBehaviour
{
    public float fadeTime;

    [SerializeField] private PromptConditionArray[] promptConditions;
    [SerializeField] private InputPromptArray[] prompts;
    [SerializeField] private VideoManager videoManager;

    [HideInInspector] public List<Coroutine> coroutines = new List<Coroutine>();
    [HideInInspector] public int prompt = 0;
    [HideInInspector] public bool disablePersistents;

    private int level;
    private bool promptActive;
    private Animator animator;
    private LevelManager levelManager;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.EnablePrompts += EnablePrompts;
        InputManager.Instance.Jump += ResetJump;
        InputManager.Instance.Move += ResetMove;
        InputManager.Instance.Proceed += Proceed;
        InputManager.Instance.SwitchControlScheme += SetAnimationLayers;
        animator = GetComponent<Animator>();
        level = SceneManager.GetActiveScene().buildIndex;
        animator.SetInteger("Level", level);
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
        StartCoroutine(CheckLoaded());
    }

    private IEnumerator CheckLoaded()
    {
        while (!GameManager.Instance.gameStarted || !levelManager.isLoaded || (videoManager != null && videoManager.isPlayingCutscene))
        {
            yield return null;
        }
        SetAnimationLayers();
        Prompt(0);
    }

    private void Update()
    {
        if (InputManager.Instance.move != Vector2.zero)
        {
            ResetMove();
        }
    }

    public void Prompt(int newPrompt = -1)
    {
        if (newPrompt >= 0)
        {
            prompt = newPrompt;
            animator.SetInteger("Prompt", prompt);
            Delay(prompts[level].prompts[prompt].fadeTime, 1, prompts[level].prompts[prompt].inputDelay);
        }
    }

    private void Delay(float fadeDuration, int direction, float delay)
    {
        if (!prompts[level].prompts[prompt].prompted)
        {
            StartCoroutine(DelayCoroutine(fadeDuration, direction, delay));
        }
    }

    private IEnumerator DelayCoroutine(float fadeDuration, int direction, float delay)
    {
        float t = 0;
        yield return new WaitForEndOfFrame();
        promptActive = true;
        while (t < delay)
        {
            if (GameManager.Instance.gameStarted && (GameManager.Instance.promptsEnabled || direction < 0) && !((promptConditions[level].promptConditions[prompt].resetOnHorizontal || promptConditions[level].promptConditions[prompt].resetOnVertical) && InputManager.Instance.move != Vector2.zero))
            {
                t += Time.deltaTime;
            }
            yield return null;
        }
        if (!prompts[level].prompts[prompt].isPersistent && !(level == 0 && prompt == 1))
        {
            prompts[level].prompts[prompt].extraPrompts--;
            if (prompts[level].prompts[prompt].extraPrompts < 0)
            {
                prompts[level].prompts[prompt].prompted = true;
            }
        }
        if ((InputManager.Instance.move.x != 0 && promptConditions[level].promptConditions[prompt].resetOnHorizontal) || (InputManager.Instance.move.y != 0 && promptConditions[level].promptConditions[prompt].resetOnVertical))
        {
            StartCoroutine(DelayCoroutine(fadeDuration, direction, delay));
        }
        else
        {
            Fade(fadeDuration, direction);
        }
    }

    private void EnablePrompts(bool enabled)
    {
        if (enabled && promptActive)
        {
            GetComponent<Image>().color = Color.white;
        }
        else
        {
            GetComponent<Image>().color = Color.clear;
        }
    }

    public void CancelPrompt()
    {
        Fade(prompts[level].prompts[prompt].fadeTime);
    }

    private void Fade(float fadeDuration = 0, int direction = -1)
    {
        if (fadeDuration == 0)
        {
            fadeDuration = fadeTime;
        }
        StopAllCoroutines();
        StartCoroutine(FadeCoroutine(direction, fadeDuration));
    }

    private IEnumerator FadeCoroutine(int direction, float fadeDuration)
    {
        Image image = GetComponent<Image>();
        float t = 0;
        float startOpacity = image.color.a;
        while (image.color.a != (direction == 1? 1 : 0))
        {
            image.color = new Color(1, 1, 1, Mathf.Clamp(startOpacity + direction * t, 0, 1));
            t += Time.deltaTime / fadeDuration;
            yield return null;
        }
        image.color = new Color(1, 1, 1, direction < 0? 0 : 1);
        if (direction == -1 && prompts[level].prompts[prompt].isPersistent && !disablePersistents)
        {
            Delay(prompts[level].prompts[prompt].fadeTime, 1, prompts[level].prompts[prompt].inputDelay);
        }
        if (direction < 0)
        {
            promptActive = false;
        }
    }

    private void ResetMove()
    {
        if (InputManager.Instance.move.x == 0)
        {
            if (promptConditions[level].promptConditions[prompt].resetOnHorizontal && promptActive)
            {
                CancelPrompt();
            }
        }
        if (InputManager.Instance.move.y == 0)
        {
            if (promptConditions[level].promptConditions[prompt].resetOnVertical && promptActive)
            {
                CancelPrompt();
            }
        }
    }

    private void ResetJump()
    {
        if (promptConditions[level].promptConditions[prompt].resetOnJump && promptActive)
        {
            CancelPrompt();
        }
    }

    private void Proceed()
    {
        if (promptConditions[level].promptConditions[prompt].resetOnClick && promptActive)
        {
            CancelPrompt();
        }
    }

    private void SetAnimationLayers()
    {
        animator.SetLayerWeight(1, (!GameManager.Instance.playstationLayout && InputManager.Instance.inputMethod == "Gamepad") ? 1 : 0);
        animator.SetLayerWeight(2, (GameManager.Instance.playstationLayout && InputManager.Instance.inputMethod == "Gamepad") ? 1 : 0);
    }

    private void OnDisable()
    {
        GameManager.Instance.EnablePrompts -= EnablePrompts;
        InputManager.Instance.Jump -= ResetJump;
        InputManager.Instance.Move -= ResetMove;
        InputManager.Instance.Proceed -= Proceed;
        InputManager.Instance.SwitchControlScheme -= SetAnimationLayers;
    }

    [System.Serializable]
    private struct PromptConditions
    {
        public bool resetOnJump;
        public bool resetOnClick;
        public bool resetOnVertical;
        public bool resetOnHorizontal;
    }

    [System.Serializable]
    private struct PromptConditionArray
    {
        public PromptConditions[] promptConditions;
    }

    [System.Serializable]
    private struct InputPrompt
    {
        public float inputDelay;
        public float fadeTime;
        public bool isPersistent;
        public int extraPrompts;
        [HideInInspector] public bool prompted;
    }

    [System.Serializable]
    private struct InputPromptArray
    {
        public InputPrompt[] prompts;
    }
}
