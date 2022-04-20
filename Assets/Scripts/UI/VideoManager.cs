using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public float musicCrossfadeTime, fadeFramesBefore;
    public Image background;
    public Cutscene[] cutScenes;
    public int testScene;

    private VideoPlayer videoPlayer;
    private RawImage image;
    private AudioManager audioManager;
    private Image frame;
    private bool isPlayingCutscene;
    public int currentScene = 0, currentCutscene = 0;

    // Start is called before the first frame update
    void Awake()
    {
        frame = transform.Find("Frame").GetComponent<Image>();
        audioManager = GameObject.Find("SoundManager").GetComponent<AudioManager>();
        videoPlayer = GetComponent<VideoPlayer>();
        image = GetComponent<RawImage>();
        videoPlayer.targetTexture.Release();
        videoPlayer.SetTargetAudioSource(0, audioManager.sfxSource);
    }

    public void CheckCutscene()
    {
        for (int i = 0; i < cutScenes.Length; i++)
        {
            if (cutScenes[i].storyPoint == GameManager.Instance.storyPoint)
            {
                PlayCutscene(i);
                if ((GameManager.Instance.storyPoint == 2 || GameManager.Instance.storyPoint == 6 || GameManager.Instance.storyPoint == 10) && GameObject.Find("UI/Upgrades").GetComponent<UpgradeMenu>().lerpDir == -1)
                {
                    GameObject.Find("UI/Upgrades").GetComponent<UpgradeMenu>().FlipLerpDir();
                }
            }
        }
    }

    private void Update()
    {
        if (Application.isEditor)
        {
            EditorUpdate();
        }
    }

    public void PlayCutscene(int cutscene)
    {
        GameObject.Find("UI/Prompts").GetComponent<InputPrompts>().startPrompted = true;
        GameObject.Find("UI/Prompts").GetComponent<InputPrompts>().coroutines.Add(StartCoroutine(GameObject.Find("UI/Prompts").GetComponent<InputPrompts>().Fade(-1, 0.1f)));
        currentCutscene = cutscene;
        GameObject.Find("BongleIsland").GetComponent<BongleIsland>().isInputEnabled = false;
        background.enabled = true;
        frame.enabled = true;
        if (GameManager.Instance.musicMuted == 1)
        {
            audioManager.PlayMusicWithFade("Story", musicCrossfadeTime);
        }
        isPlayingCutscene = true;
        StopCoroutine("PlayVideo");
        StartCoroutine("PlayVideo");

    }

    public IEnumerator PlayVideo()
    {
        if (isPlayingCutscene)
        {
            if (currentCutscene == 6 && currentScene == 4)
            {
                AudioManager.Instance.PlayMusicWithFade("Silence");
            }
            if (cutScenes[currentCutscene].scenes[currentScene].isLooping)
            {
                GameObject.Find("UI/Prompts").GetComponent<InputPrompts>().Prompt(1);
            }
            if (cutScenes[currentCutscene].scenes[currentScene].audio != null)
            {
                audioManager.PlayVideoSFX(cutScenes[currentCutscene].scenes[currentScene].audio);
            }
            VideoClip video = cutScenes[currentCutscene].scenes[currentScene].video;
            if (!videoPlayer.enabled)
            {
                videoPlayer.enabled = true;
                image.enabled = true;
            }
            videoPlayer.clip = video;
            videoPlayer.isLooping = cutScenes[currentCutscene].scenes[currentScene].isLooping;
            videoPlayer.frame = 0;
            videoPlayer.Play();
            if (currentScene == cutScenes[currentCutscene].scenes.Length - 1)
            {
                StartCoroutine(CheckEnd(video));
                currentScene = 0;
            }
            else
            {
                currentScene++;
            }
            while (videoPlayer.frame < System.Convert.ToInt64(video.frameCount) - 1 || videoPlayer.isLooping)
            {
                yield return null;
                if (Input.GetAxis("Jump") == 1 || Input.GetMouseButtonDown(0))
                {
                    videoPlayer.isLooping = false;
                }
            }
            if (currentScene != cutScenes[currentCutscene].scenes.Length)
            {
                StartCoroutine("PlayVideo");
            }
        }
        else
        {
            yield break;
        }
    }

    private IEnumerator CheckEnd(VideoClip clip)
    {
        isPlayingCutscene = false;
        while (videoPlayer.frame < System.Convert.ToInt64(clip.frameCount) - fadeFramesBefore)
        {
            yield return null;
        }
        if (!(GameManager.Instance.musicMuted == 0))
        {
            audioManager.PlayMusicWithFade("Map", musicCrossfadeTime);
        }
        while (videoPlayer.frame < System.Convert.ToInt64(clip.frameCount) - 1)
        {
            yield return null;
        }
        GameManager.Instance.storyPoint++;
        Debug.Log("Story Point : " + GameManager.Instance.storyPoint);
        videoPlayer.enabled = false;
        videoPlayer.targetTexture.Release();
        image.enabled = false;
        background.enabled = false;
        frame.enabled = false;
        if (GameObject.Find("CloudCover") != null)
        {
            GameObject.Find("CloudCover").SetActive(false);
        }
        GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Map>().UpdateRegionsUnlocked();
        GameObject.Find("UI/Prompts").GetComponent<InputPrompts>().startPrompted = false;
        GameObject.Find("UI/Prompts").GetComponent<InputPrompts>().ResetTimers();
        GameObject.FindGameObjectWithTag("Player").GetComponent<BongleIsland>().isInputEnabled = true;
        Cursor.visible = true;
        foreach (Transform region in GameObject.Find("BossRegions").transform)
        {
            if (region.Find("BossIsland/Arrow").gameObject.activeSelf)
            {
                region.Find("BossIsland/Arrow").gameObject.SetActive(false);
            }
            if (GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Map>().arrowCoroutine != null)
            {
                StopCoroutine(GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Map>().arrowCoroutine);
            }
        }
    }

    void EditorUpdate()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            PlayCutscene(testScene);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            videoPlayer.frame = System.Convert.ToInt64(cutScenes[currentCutscene].scenes[currentScene].video.frameCount - fadeFramesBefore);
        }
    }

    [System.Serializable]
    public struct StoryScene
    {
        public VideoClip video;
        public AudioClip audio;
        public bool isLooping;
    }

    [System.Serializable]
    public struct Cutscene
    {
        public int storyPoint;
        public StoryScene[] scenes;
    }
}
