using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScrapGrabberUI : MinigameUI
{
    [SerializeField] private Color freezeFrameTextColour;
    [SerializeField] private TextMeshPro bellText;
    [SerializeField] private SpriteRenderer bellSprite;

    private LevelManager_ScrapGrabber levelManager;
    private TMPro.TextMeshProUGUI freezeText;

    protected override void Start()
    {
        starScoreIndex = 2;
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_ScrapGrabber>();
        freezeText = primaryAbilityFrame.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        secondaryCount = (float)System.Math.Round(levelManager.remainingTime, 0);
        base.Update();
    }

    protected override void UpdateAbilityFrames()
    {
        base.UpdateAbilityFrames();
        bellText.gameObject.SetActive(controlSchemeIndex == 0);
        bellSprite.gameObject.SetActive(controlSchemeIndex != 0);
        if (controlSchemeIndex == 0)
        {
            bellText.text = InputManager.Instance.abilityPrompts[2].abilityPrompts[controlSchemeIndex].text;
            bellText.color = InputManager.Instance.abilityPrompts[2].abilityPrompts[controlSchemeIndex].colour;
            freezeText.color = freezeFrameTextColour;
        }
        else
        {
            bellSprite.sprite = InputManager.Instance.abilityPrompts[2].abilityPrompts[controlSchemeIndex].sprite;
        }
    }

    public override void EndGame()
    {
        Time.timeScale = 0;
        secondaryCount = Mathf.Ceil(trashScore * trashCount + Time.timeSinceLevelLoad);
        GameObject.FindGameObjectWithTag("Player").GetComponent<Claw>().StopSounds();
        base.EndGame();
    }
}
