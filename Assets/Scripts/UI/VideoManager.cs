using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public float musicCrossfadeTime, fadeFramesBefore;
    public Image background;
    public Cutscene[] cutScenes;
    public int testScene;
    public Button upgradeButton;

    private VideoPlayer videoPlayer;
    private RawImage image;
    private AudioManager audioManager;
    private Image frame;
    private bool isPlayingCutscene;
    public int currentScene = 0, currentCutscene = 0;
    private InputActionMap savedActionMap;
    private PromptManager promptManager;

    // Start is called before the first frame update
    void Awake()
    {
        frame = transform.Find("Frame").GetComponent<Image>();
        audioManager = GameObject.Find("SoundManager").GetComponent<AudioManager>();
        promptManager = GameObject.Find("UI/Prompts").GetComponent<PromptManager>();
        videoPlayer = GetComponent<VideoPlayer>();
        image = GetComponent<RawImage>();
        videoPlayer.targetTexture.Release();
        videoPlayer.SetTargetAudioSource(0, audioManager.sfxSource);
    }

    private void Start()
    {
        InputManager.Instance.Proceed += StopLoop;
    }

    public void CheckCutscene()
    {
        for (int i = 0; i < cutScenes.Length; i++)
        {
            if (cutScenes[i].storyPoint == GameManager.Instance.storyPoint)
            {
                PlayCutscene(i);
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
        promptManager.CancelPrompt();
        savedActionMap = InputManager.Instance.playerInput.currentActionMap;
        InputManager.Instance.playerInput.SwitchCurrentActionMap("Video");
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
                promptManager.Prompt(1);
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
        InputManager.Instance.playerInput.currentActionMap = savedActionMap;
        if ((GameManager.Instance.storyPoint == 2 || GameManager.Instance.storyPoint == 6 || GameManager.Instance.storyPoint == 10) && GameObject.Find("UI/Upgrades").GetComponent<UpgradeMenu>().lerpDir == -1)
        {
            upgradeButton.onClick?.Invoke();
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
        GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Map>().isLoaded = true;
        promptManager.Prompt(0);
        GameObject.FindGameObjectWithTag("Player").GetComponent<BongleIsland>().isInputEnabled = true;
        InputManager.Instance.EnableCursor();
        foreach (Transform region in GameObject.Find("BossRegions").transform)
        {
            if (region.Find("BossIsland/Arrow").gameObject.activeSelf)
            {
                region.Find("BossIsland/Arrow").gameObject.SetActive(false);
            }
        }
    }

    private void StopLoop()
    {
        if (videoPlayer != null)
        {
            videoPlayer.isLooping = false;
        }
    }

    void EditorUpdate()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard.vKey.wasPressedThisFrame)
        {
            PlayCutscene(testScene);
        }
        if (keyboard.bKey.wasPressedThisFrame)
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
