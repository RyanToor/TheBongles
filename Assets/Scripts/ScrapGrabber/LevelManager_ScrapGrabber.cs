using UnityEngine;

public class LevelManager_ScrapGrabber : LevelManager
{
    public float maxTime;
    
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
    }
}
