using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.UI;

public class LevelManager_ScrapGrabber : LevelManager
{
    public float maxTime, dangerTime, freezeTimeMultiplier, freezeTimeDuration, freezeTimeCooldown, freezeFadeTime, bellDuration, bellCooldownDuration, lightsOutDuration;
    public bool lightsOn = true, loseFuel;
    public LightType[] lights;
    public GameObject freezeCooldownPanel, bellPromptText;
    public Image freezeIcon;
    public Animator freezeFrameAnimator, bellAnimator, bellAddonAnimator;
    public GameObject[] freezeElements;
    public GameObject[] bellIndicators;
    public Spawner spawner;
    public Color darkCollectionIndicatorColour;

    [Header("Brainy Settings")]
    public Animator brainy;
    [Range(0, 100)]
    public float focusChance;
    public float shockLength, eyeOffset, focusTime;
    public Transform eyes, eyePoint, claw;
    private bool isFocussed;
    private float currentEyeOffset;

    [HideInInspector]
    public float remainingTime;
    [HideInInspector]
    public bool isFreezeEnabled = false, isBellEnabled = false, isBellActive = false;

    private ScrapGrabberUI uI;
    private bool isFreezing, isBellCooling, gameEnded = false;
    private Color[] freezeColours;
    private Color freezeIconDisabled;

    protected override void Awake()
    {
        freezeFrameAnimator.SetBool("Freeze", true);
        remainingTime = maxTime;
        uI = GameObject.Find("Canvas").GetComponent<ScrapGrabberUI>();
        freezeColours = new Color[freezeElements.Length];
        for (int i = 0; i < freezeElements.Length; i++)
        {
            if (freezeElements[i].GetComponent<SpriteRenderer>() != null)
            {
                freezeColours[i] = freezeElements[i].GetComponent<SpriteRenderer>().color;
                freezeElements[i].GetComponent<SpriteRenderer>().color = Color.clear;
            }
            else
            {
                freezeColours[i] = freezeElements[i].GetComponent<Image>().color;
                freezeElements[i].GetComponent<Image>().color = Color.clear;
            }
        }
        freezeIconDisabled = freezeIcon.color;
        freezeCooldownPanel.transform.localScale = Vector2.right;
        freezeIcon.color = Color.white;
        foreach (GameObject bellIndicator in bellIndicators)
        {
            bellIndicator.SetActive(true);
        }
        currentEyeOffset = eyeOffset;
        StartCoroutine(CheckLoaded());
        if (!Application.isEditor)
        {
            loseFuel = true;
        }
    }

    private IEnumerator CheckLoaded()
    {
        while (false)
        {
            yield return null;
        }
        Destroy(GameObject.Find("LoadingCanvas(Clone)"));
        AudioManager.Instance.PlayMusic("Scrap Grabber");
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (loseFuel)
        {
            remainingTime = Mathf.Clamp(remainingTime - Time.deltaTime, 0, float.MaxValue);
        }
        if (remainingTime == 0 && !gameEnded)
        {
            gameEnded = true;
            uI.EndGame();
        }
        if (Input.GetAxisRaw("Primary Ability") == 1 && !isFreezing && isFreezeEnabled)
        {
            StartCoroutine(FreezeTime());
        }
        if (Input.GetAxisRaw("Secondary Ability") == 1 && !(isBellActive || isBellCooling) && isBellEnabled)
        {
            StartCoroutine(Bell());
        }
        brainy.SetFloat("RandomChance", Random.Range(0f, 100f));
        base.Update();
        if (Application.isEditor)
        {
            EditorUpdate();
        }
    }

    private void FixedUpdate()
    {
        BrainyEyes();
    }

    public bool LightsOn
    {
        get
        {
            return lightsOn;
        }
        set
        {
            lightsOn = value;
            foreach (LightType light in lights)
            {
                light.lightComponent.enabled = (light.isBlackoutLight ^ lightsOn);
            }
        }
    }

    private IEnumerator FreezeTime()
    {
        freezeFrameAnimator.SetBool("Available", false);
        brainy.SetBool("Freeze", true);
        isFreezing = true;
        float duration = 0;
        Time.timeScale = freezeTimeMultiplier;
        StartCoroutine(Fade(true));
        while (duration < freezeTimeDuration)
        {
            freezeCooldownPanel.transform.localScale = Vector2.Lerp(Vector2.right, Vector2.one, duration / freezeTimeDuration);
            duration += Time.deltaTime / Time.timeScale;
            yield return null;
        }
        brainy.SetBool("Freeze", false);
        freezeIcon.color = freezeIconDisabled;
        Time.timeScale = 1;
        StartCoroutine(Fade(false));
        duration = freezeTimeCooldown;
        while (duration > 0)
        {
            freezeCooldownPanel.transform.localScale = Vector2.Lerp(Vector2.right, Vector2.one, duration / freezeTimeCooldown);
            duration -= Time.deltaTime;
            yield return null;
        }
        isFreezing = false;
        freezeCooldownPanel.transform.localScale = Vector2.right;
        freezeIcon.color = Color.white;
        freezeFrameAnimator.SetBool("Available", true);
    }

    private IEnumerator Bell()
    {
        bellAnimator.SetTrigger("Ring");
        brainy.SetTrigger("Bell");
        isBellActive = true;
        float duration = bellDuration;
        isBellActive = true;
        bellPromptText.SetActive(false);
        bellAddonAnimator.SetBool("Available", false);
        spawner.Escape();
        while (duration > 0)
        {
            duration -= Time.deltaTime / Time.timeScale;
            for (int i = 0; i < bellIndicators.Length; i++)
            {
                bellIndicators[i].SetActive(duration > i * (bellDuration / bellIndicators.Length));
            }
            yield return null;
        }
        bellAnimator.SetBool("Available", false);
        duration = 0;
        isBellActive = false;
        isBellCooling = true;
        while (duration < bellCooldownDuration)
        {
            duration += Time.deltaTime;
            for (int i = 0; i < bellIndicators.Length; i++)
            {
                bellIndicators[i].SetActive(duration > i * (bellCooldownDuration / bellIndicators.Length) + bellCooldownDuration / bellIndicators.Length);
            }
            yield return null;
        }
        isBellCooling = false;
        foreach (GameObject bellIndicator in bellIndicators)
        {
            bellIndicator.SetActive(true);
        }
        bellAnimator.SetBool("Available", true);
        bellAddonAnimator.SetBool("Available", true);
        bellPromptText.SetActive(true);
    }

    private IEnumerator Fade(bool fadeIn)
    {
        float duration = 0;
        while (duration < freezeFadeTime)
        {
            for (int i = 0; i < freezeElements.Length; i++)
            {
                if (freezeElements[i].GetComponent<SpriteRenderer>() != null)
                {
                    freezeElements[i].GetComponent<SpriteRenderer>().color = Color.Lerp(fadeIn? Color.clear : freezeColours[i], fadeIn? freezeColours[i] : Color.clear, duration / freezeFadeTime);
                }
                else
                {
                    freezeElements[i].GetComponent<Image>().color = Color.Lerp(fadeIn ? Color.clear : freezeColours[i], fadeIn ? freezeColours[i] : Color.clear, duration / freezeFadeTime);
                }
            }
            duration += Time.deltaTime / (isFreezing? Time.timeScale : 1);
            yield return null;
        }
    }

    public IEnumerator LightsOut(GameObject eel = null)
    {
        if (LightsOn)
        {
            if (eel != null)
            {
                eel.GetComponent<Animator>().SetBool("Shock", true);
            }
            float duration = 0;
            LightsOn = false;
            while (duration < lightsOutDuration)
            {
                duration += Time.deltaTime;
                yield return null;
            }
            LightsOn = true;
            if (eel != null)
            {
                eel.GetComponent<Animator>().SetBool("Shock", false);
            }
        }
    }

    public IEnumerator BrainyShock()
    {
        brainy.SetBool("Shock", true);
        float duration = 0;
        while (duration < shockLength)
        {
            duration += Time.deltaTime;
            yield return null;
        }
        brainy.SetBool("Shock", false);
    }

    private void BrainyEyes()
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

    private void EditorUpdate()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LightsOn = !LightsOn;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (Time.timeScale == 1)
            {
                Time.timeScale = 0.2f;
            }
            else
            {
                Time.timeScale = 1;
            }
        }
    }

    [System.Serializable]
    public struct LightType
    {
        public Light2D lightComponent;
        public bool isBlackoutLight;
    }
}
