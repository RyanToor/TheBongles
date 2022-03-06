using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class LevelManager_ScrapGrabber : LevelManager
{
    public float maxTime;
    public bool lightsOn = true;
    public LightType[] lights;
    
    [HideInInspector]
    public float remainingTime;

    protected override void Awake()
    {
        remainingTime = maxTime;
    }

    // Update is called once per frame
    protected override void Update()
    {
        remainingTime = Mathf.Clamp(remainingTime - Time.deltaTime, 0, float.MaxValue);
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
