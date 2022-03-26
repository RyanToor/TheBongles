using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.UI;

public class LevelManager_ScrapGrabber : LevelManager
{
    public float maxTime, freezeTimeMultiplier, freezeTimeDuration, freezeTimeCooldown, freezeFadeTime;
    public bool lightsOn = true;
    public LightType[] lights;
    public GameObject[] freezeElements;
    
    [HideInInspector]
    public float remainingTime;
    [HideInInspector]
    public bool canFreeze = false;

    private ScrapGrabberUI uI;
    private bool isFreezing, gameEnded = false;
    private Color[] freezeColours;

    protected override void Awake()
    {
        remainingTime = maxTime;
        uI = GameObject.Find("Canvas").GetComponent<ScrapGrabberUI>();
        Destroy(GameObject.Find("LoadingCanvas(Clone)"));
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
    }

    // Update is called once per frame
    protected override void Update()
    {
        remainingTime = Mathf.Clamp(remainingTime - Time.deltaTime, 0, float.MaxValue);
        if (remainingTime == 0 && !gameEnded)
        {
            gameEnded = true;
            uI.EndGame();
        }
        if (Input.GetKeyDown(KeyCode.F) && !isFreezing && canFreeze)
        {
            StartCoroutine(FreezeTime());
        }
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

    private IEnumerator FreezeTime()
    {
        isFreezing = true;
        float duration = 0;
        Time.timeScale = freezeTimeMultiplier;
        StartCoroutine(Fade(true));
        while (duration < freezeTimeDuration)
        {
            duration += Time.deltaTime / Time.timeScale;
            yield return null;
        }
        Time.timeScale = 1;
        StartCoroutine(Fade(false));
        duration = freezeTimeCooldown;
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            yield return null;
        }
        isFreezing = false;
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

    private void EditorUpdate()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LightsOn = !LightsOn;
        }
    }

    [System.Serializable]
    public struct LightType
    {
        public Light2D lightComponent;
        public bool isBlackoutLight;
    }
}
