using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InputPrompts : MonoBehaviour
{
    public float fadeTime, videoDelay, inputDelayMap, inputDelayTrashHunt, inputDelayRobotLaunch, inputDelayRobotMove, inputDelayClawLaunch, inputDelayClawTurn, inputDelayClawReel;

    [HideInInspector]
    public bool startPrompted, videoPlaying;
    [HideInInspector]
    public List<Coroutine> coroutines = new List<Coroutine>();

    private int level, launchPrompted;
    private float horizontalLastActive, verticalLastActive, jumpLastActive;
    private bool promptActive, flyPrompted, firePrompted, reelPrompted, turnPrompted;
    private LevelManager_Robot levelManagerRobot;
    private Claw claw;
    private Image angleDial, powerWheel;

    // Start is called before the first frame update
    void Start()
    {
        level = SceneManager.GetActiveScene().buildIndex;
        if (level == 2)
        {
            levelManagerRobot = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Robot>();
            angleDial = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("ThrowParameters/Angle dial").GetComponent<Image>();
            powerWheel = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("ThrowParameters/Power wheel").GetComponent<Image>();
        }
        if (level == 3)
        {
            claw = GameObject.FindGameObjectWithTag("Player").GetComponent<Claw>();
        }
        GetComponent<Animator>().SetInteger("Level", level);
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            if (!GameManager.Instance.levelsPrompted[SceneManager.GetActiveScene().buildIndex])
            {
                transform.Find("UpgradeBook").gameObject.SetActive(true);
                GetComponent<PauseMenu>().UpgradeBook();
                GameManager.Instance.PauseGame(true);
                GameManager.Instance.levelsPrompted[SceneManager.GetActiveScene().buildIndex] = true;
            }
        }
    }

    public void Prompt(int prompt)
    {
        GetComponent<Animator>().SetInteger("Prompt", prompt);
        if (!startPrompted && level == 0 && prompt == 0)
        {
            startPrompted = true;
        }
        coroutines.Add(StartCoroutine(Fade(1, fadeTime, level == 0 && prompt == 1? videoDelay : 0)));
        promptActive = true;
    }

    public IEnumerator LevelPrompts()
    {
        ResetTimers();
        while (SceneManager.GetActiveScene().buildIndex == level)
        {
            if (Input.GetAxisRaw("Horizontal") != 0)
            {
                horizontalLastActive = Time.time;
            }
            if (Input.GetAxisRaw("Vertical") != 0)
            {
                verticalLastActive = Time.time;
            }
            if (Input.GetAxisRaw("Jump") != 0)
            {
                jumpLastActive = Time.time;
            }
            switch (level)
            {
                case 0:
                    if (!startPrompted && (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0))
                    {
                        startPrompted = true;
                    }
                    if (!startPrompted && Time.time - horizontalLastActive > inputDelayMap && Time.time - verticalLastActive > inputDelayMap)
                    {
                        Prompt(0);
                    }
                    break;
                case 1:
                    if (!promptActive && Time.time - horizontalLastActive > inputDelayTrashHunt && Time.time - verticalLastActive > inputDelayTrashHunt)
                    {
                        Prompt(0);
                    }
                    break;
                case 2:
                    if (!promptActive && ((levelManagerRobot.State == LevelState.launch && launchPrompted < 2 && Time.time - jumpLastActive > inputDelayRobotLaunch && (angleDial.color.a == 1 || powerWheel.color.a == 1)) || (levelManagerRobot.State == LevelState.fly && !flyPrompted && levelManagerRobot.throwPowerLevel > 1 && Time.time - jumpLastActive > inputDelayRobotLaunch)))
                    {
                        Prompt(0);
                        if (levelManagerRobot.State == LevelState.launch)
                        {
                            launchPrompted++;
                        }
                        else if (levelManagerRobot.State == LevelState.fly)
                        {
                            flyPrompted = true;
                        }
                    }
                    else if (!promptActive && levelManagerRobot.State == LevelState.move && Time.time - horizontalLastActive > inputDelayRobotMove && Time.time - verticalLastActive > inputDelayRobotMove && Time.time - jumpLastActive > inputDelayRobotMove)
                    {
                        Prompt(1);
                    }
                    break;
                case 3:
                    if (!firePrompted && (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Jump") != 0) && claw.state == Claw.ClawState.aim)
                    {
                        firePrompted = true;
                    }
                    if (!turnPrompted && Input.GetAxisRaw("Horizontal") != 0 && claw.state == Claw.ClawState.fire)
                    {
                        turnPrompted = true;
                    }
                    if (!reelPrompted && Input.GetAxisRaw("Jump") != 0  && claw.transform.Find("TrashContainer").childCount > 1)
                    {
                        reelPrompted = true;
                    }
                    if (!promptActive && !firePrompted && claw.state == Claw.ClawState.aim && Time.time - jumpLastActive > inputDelayClawLaunch)
                    {
                        Prompt(0);
                        firePrompted = true;
                    }
                    else if (!promptActive && !turnPrompted && claw.state == Claw.ClawState.fire && Time.time - horizontalLastActive > inputDelayClawTurn && Time.time - jumpLastActive > inputDelayClawTurn)
                    {
                        Prompt(1);
                        turnPrompted = true;
                    }
                    else if (!reelPrompted && claw.isMultiClaw && claw.transform.Find("TrashContainer").childCount > 0 && Time.time - jumpLastActive > inputDelayClawReel)
                    {
                        Prompt(2);
                        reelPrompted = true;
                    }
                    break;
                default:
                    break;
            }
            yield return null;
        }
    }

    private IEnumerator Delay(int direction, float fadeDuration, float delay)
    {
        float t = 0;
        while (t < delay)
        {
            t += Time.deltaTime;
            yield return null;
        }
        FadeInterrupt(direction, fadeDuration);
    }

    public IEnumerator Fade(int direction, float fadeDuration, float delay = 0)
    {
        if (direction == 1)
        {
            coroutines.Add(StartCoroutine(ConfirmInput()));
        }
        if (delay > 0)
        {
            coroutines.Add(StartCoroutine(Delay(direction, fadeDuration, delay)));
            yield break;
        }
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
    }

    private IEnumerator ConfirmInput()
    {
        while (true)
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Jump") == 1 || (Input.GetMouseButtonDown(0) && level == 0))
            {
                FadeInterrupt(-1, fadeTime);
                yield break;
            }
            else if (level == 3)
            {
                if (claw.state == Claw.ClawState.reel)
                {
                    FadeInterrupt(-1, fadeTime);
                    yield break;
                }
            }
            yield return null;
        }
    }

    private void FadeInterrupt(int direction, float fadeDuration)
    {
        List<Coroutine> coroutinesToRemove = new List<Coroutine>();
        foreach (Coroutine coroutine in coroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            else
            {
                coroutinesToRemove.Add(coroutine);
            }
        }
        foreach (Coroutine coroutine in coroutinesToRemove)
        {
            coroutines.Remove(coroutine);
        }
        coroutines.TrimExcess();
        if (direction < 0)
        {
            promptActive = false;
        }
        coroutines.Add(StartCoroutine(Fade(direction, fadeDuration)));
    }

    public void ResetTimers()
    {
        horizontalLastActive = Time.time; verticalLastActive = Time.time; jumpLastActive = Time.time;
    }
}
