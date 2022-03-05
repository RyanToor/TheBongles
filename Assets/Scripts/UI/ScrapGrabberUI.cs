using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScrapGrabberUI : MonoBehaviour
{
    public Text glassReadout, timeReadout;
    public float dangerTime, flashPeriod;
    public Color dangerColour;

    private Color normalColour;
    private bool isFlashing;

    LevelManager_ScrapGrabber levelManager;
    private void Awake()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_ScrapGrabber>();
        normalColour = timeReadout.color;
    }

    // Update is called once per frame
    void Update()
    {
        glassReadout.text = levelManager.glass.ToString();
        if (0 < levelManager.remainingTime && levelManager.remainingTime < dangerTime && !isFlashing)
        {
            StartCoroutine(Flash());
            isFlashing = true;
        }
    }

    private IEnumerator Flash()
    {
        float duration = 0;
        while (levelManager.remainingTime < dangerTime && levelManager.remainingTime > 0)
        {
            timeReadout.color = duration % flashPeriod < flashPeriod / 2 ? dangerColour : normalColour;
            duration += Time.deltaTime;
            yield return null;
        }
        timeReadout.color = normalColour;
        isFlashing = false;
    }

    private void FixedUpdate()
    {
        timeReadout.text = System.Math.Round(levelManager.remainingTime, 0).ToString() + " s";
    }
}
