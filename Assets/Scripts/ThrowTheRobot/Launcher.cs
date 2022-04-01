using System.Collections;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    public float reelSpeed, lineWidth;
    public Material lineMaterial;

    [HideInInspector]
    public bool isReeling;

    private LevelManager_Robot levelManager;
    private Transform reelBottom, reelTop;
    private GameObject robot;
    private bool reelStarted;

    private void Awake()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Robot>();
        robot = GameObject.FindGameObjectWithTag("Player");
        reelBottom = transform.Find("Bubba/LineBottom");
        reelTop = transform.Find("Bubba/LineStop");
        isReeling = true;
    }

    private void Update()
    {
        if (Application.isEditor)
        {
            EditorUpdate();
        }
    }

    public void Reel()
    {
        if (!reelStarted)
        {
            StartCoroutine(ReelIn());
            reelStarted = true;
        }
    }

    public IEnumerator ReelIn()
    {
        Vector3 robotStartPos = robot.transform.position;
        float progress = 0;
        transform.Find("Bubba").GetComponent<Animator>().SetTrigger("Reel");
        reelBottom.position = new Vector3(reelBottom.position.x, robot.transform.position.y, reelBottom.position.z);
        AudioSource reelSound = AudioManager.Instance.PlayAudioAtObject("Reel", gameObject, 20, true);
        while (progress != 1)
        {
            if (isReeling)
            {
                progress += reelSpeed * Time.deltaTime;
                progress = Mathf.Clamp(progress, 0, 1);
                Vector3 desiredPosition = Vector3.Lerp(reelBottom.position, reelTop.position, progress);
                robot.transform.position = Vector3.Lerp(robotStartPos, desiredPosition, progress);
                yield return new WaitForFixedUpdate();
            }
        }
        Destroy(reelSound);
        levelManager.State = LevelState.launch;
        transform.Find("Bubba").GetComponent<Animator>().SetTrigger("Eat");
        reelStarted = false;
    }

    private void EditorUpdate()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reel();
        }
    }
}
