using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class LevelManager_ScrapGrabber : LevelManager
{
    public float startTime, maxTime, dangerTime, freezeTimeMultiplier, freezeTimeDuration, freezeTimeCooldown, freezeFadeTime, bellDuration, bellCooldownDuration, lightsOutDuration;
    public bool lightsOn = true, loseFuel;
    public LightType[] lights;
    public GameObject freezeCooldownPanel, bellPromptText;
    public Image freezeIcon;
    public Animator freezeFrameAnimator, bellAnimator, bellAddonAnimator, submarineAnimator;
    public GameObject[] freezeElements;
    public GameObject[] bellIndicators;
    public Spawner spawner;
    public Color darkCollectionIndicatorColour;
    public Animator brainy;
    public float shockLength;

    [Header("Vibration Data")]
    [SerializeField] float shockIntensity;

    [HideInInspector]
    public float remainingTime;
    [HideInInspector]
    public bool isFreezeEnabled = false, isBellEnabled = false, isBellActive = false, gameEnded = false;

    private ScrapGrabberUI uI;
    private bool isFreezing, isBellCooling;
    private Color[] freezeColours;
    private Color freezeIconDisabled;

    protected override void Awake()
    {
        base.Awake();
        freezeFrameAnimator.SetBool("Freeze", true);
        remainingTime = startTime;
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
        if (!Application.isEditor)
        {
            loseFuel = true;
        }
    }

    private void OnEnable()
    {
        InputManager.Instance.PrimaryAbility += CheckFreeze;
        InputManager.Instance.SecondaryAbility += CheckBell;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override IEnumerator CheckLoaded()
    {
        while (!Camera.main.GetComponent<Camera_ScrapGrabber>().isInitialised)
        {
            yield return null;
        }
        promptManager.Prompt(0);
        isLoaded = true;
        StartCoroutine(base.CheckLoaded());
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
        submarineAnimator.SetFloat("RandomChance", Random.Range(0f, 100f));
        base.Update();
        if (Application.isEditor)
        {
            EditorUpdate();
        }
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

    private void CheckFreeze()
    {
        if (!isFreezing && isFreezeEnabled)
        {
            StartCoroutine(FreezeTime());
            AudioManager.Instance.PlaySFX("Freeze");
        }
    }

    private IEnumerator FreezeTime()
    {
        freezeFrameAnimator.SetBool("Available", false);
        brainy.SetBool("Freeze", true);
        submarineAnimator.SetBool("Freeze", true);
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
        submarineAnimator.SetBool("Freeze", false);
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
        AudioManager.Instance.PlaySFX("AbilityReady");
    }

    private void CheckBell()
    {
        if (!(isBellActive || isBellCooling) && isBellEnabled)
        {
            StartCoroutine(Bell());
        }
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
        AudioSource BellSource = AudioManager.Instance.PlayAudioAtObject("Bell", gameObject, 20, true);
        while (duration > 0)
        {
            duration -= Time.deltaTime / Time.timeScale;
            for (int i = 0; i < bellIndicators.Length; i++)
            {
                bellIndicators[i].SetActive(duration > i * (bellDuration / bellIndicators.Length));
            }
            yield return null;
        }
        Destroy(BellSource);
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
        AudioManager.Instance.PlaySFX("AbilityReady");
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
        InputManager.VibrationData shockVibration = InputManager.Instance.Vibrate(shockIntensity);
        brainy.SetBool("Shock", true);
        float duration = 0;
        AudioSource ElectricEelSource = AudioManager.Instance.PlayAudioAtObject("Electricute", gameObject, 20, true);
        while (duration < shockLength)
        {
            duration += Time.deltaTime;
            yield return null;
        }
        brainy.SetBool("Shock", false);
        InputManager.Instance.vibrations.Remove(shockVibration);
        Destroy(ElectricEelSource);
    }

    private void OnDisable()
    {
        InputManager.Instance.PrimaryAbility -= CheckFreeze;
        InputManager.Instance.SecondaryAbility -= CheckBell;
    }

    private void EditorUpdate()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard.lKey.wasPressedThisFrame)
        {
            LightsOn = !LightsOn;
        }
        if (keyboard.tKey.wasPressedThisFrame)
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
        public UnityEngine.Rendering.Universal.Light2D lightComponent;
        public bool isBlackoutLight;
    }
}
