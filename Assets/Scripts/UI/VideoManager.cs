using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public float musicCrossfadeTime, fadeFramesBefore;
    public Cutscene[] cutScenes;
    public int testScene;
    public Button upgradeButton;

    [HideInInspector] public bool isPlayingCutscene;

    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private GameObject videoContainer;

    private List<VideoPlayer> videoPlayers = new();
    private AudioManager audioManager;
    public int currentScene = 0, currentCutscene = 0;
    private InputActionMap savedActionMap;
    private PromptManager promptManager;
    private bool looped;

    // Start is called before the first frame update
    void Awake()
    {
        audioManager = GameObject.Find("SoundManager").GetComponent<AudioManager>();
        promptManager = GameObject.Find("UI/Prompts").GetComponent<PromptManager>();
        videoContainer = transform.Find("Video").gameObject;
        renderTexture.Release();
    }

    private void Start()
    {
        InputManager.Instance.Proceed += NextVideo;
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
        isPlayingCutscene = true;
        promptManager.disablePersistents = true;
        promptManager.CancelPrompt();
        savedActionMap = InputManager.Instance.playerInput.currentActionMap;
        InputManager.Instance.playerInput.SwitchCurrentActionMap("Video");
        currentCutscene = cutscene;
        GameObject.Find("BongleIsland").GetComponent<BongleIsland>().isInputEnabled = false;
        if (GameManager.Instance.musicMuted == 1)
        {
            audioManager.PlayMusicWithFade("Story", musicCrossfadeTime);
        }
        StopVideo();
        for (int i = 0; i < cutScenes[currentCutscene].scenes.Length; i++)
        {
            VideoPlayer newVideoPlayer = videoContainer.AddComponent<VideoPlayer>();
            newVideoPlayer.enabled = false;
            newVideoPlayer.playOnAwake = false;
            newVideoPlayer.SetTargetAudioSource(0, audioManager.sfxSource);
            newVideoPlayer.isLooping = cutScenes[currentCutscene].scenes[i].isLooping;
            newVideoPlayer.targetTexture = renderTexture;
            newVideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            newVideoPlayer.errorReceived += VideoError;
            if (cutScenes[currentCutscene].scenes[i].isLooping)
            {
                newVideoPlayer.loopPointReached += LoopPoint;
            }
            if (GameManager.Instance.isWeb)
            {
                newVideoPlayer.source = VideoSource.Url;
                newVideoPlayer.url = /*"https://drive.google.com/uc?export=download&id=" + */cutScenes[currentCutscene].scenes[i].googleURL.Replace("https://www", "https://dl").Replace("?dl=0", "?dl=1");
            }
            else
            {
                newVideoPlayer.clip = cutScenes[currentCutscene].scenes[i].video;
            }
            videoPlayers.Add(newVideoPlayer);
        }
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        videoPlayers[0].enabled = true;
        videoPlayers[0].Prepare();
        StartCoroutine("PlayVideo");

    }

    public IEnumerator PlayVideo()
    {
        looped = false;
        videoPlayers[currentScene].frame = 0;
        if (currentScene < cutScenes[currentCutscene].scenes.Length - 1)
        {
            videoPlayers[currentScene + 1].enabled = true;
            videoPlayers[currentScene + 1].Prepare();
        }
        if (currentCutscene == 6 && currentScene == 4)
        {
            AudioManager.Instance.PlayMusicWithFade("Silence");
        }
        while (!videoPlayers[currentScene].isPrepared)
        {
            yield return null;
        }
        if (cutScenes[currentCutscene].scenes[currentScene].audio != null)
        {
            audioManager.PlayVideoSFX(cutScenes[currentCutscene].scenes[currentScene].audio);
        }
        videoPlayers[currentScene].Play();
        if (!videoPlayers[currentScene].isLooping && currentScene < cutScenes[currentCutscene].scenes.Length - 1)
        {
            while (videoPlayers[currentScene].frame < System.Convert.ToInt64(videoPlayers[currentScene].frameCount) - 1)
            {
                yield return new WaitForEndOfFrame();
            }
            videoPlayers[currentScene].errorReceived -= VideoError;
            Destroy(videoPlayers[currentScene]);
            currentScene++;
            StartCoroutine("PlayVideo");
        }
        if (currentScene == cutScenes[currentCutscene].scenes.Length - 1 && !cutScenes[currentCutscene].scenes[currentScene].isLooping)
        {
            StartCoroutine(CheckEnd(videoPlayers[currentScene].frameCount));
            currentScene = 0;
        }
    }

    private IEnumerator CheckEnd(ulong clipLength)
    {
        while (videoPlayers[cutScenes[currentCutscene].scenes.Length - 1].frame < System.Convert.ToInt64(clipLength) - fadeFramesBefore)
        {
            yield return null;
        }
        if (!(GameManager.Instance.musicMuted == 0))
        {
            audioManager.PlayMusicWithFade("Map", musicCrossfadeTime);
        }
        while (videoPlayers[cutScenes[currentCutscene].scenes.Length - 1].frame < System.Convert.ToInt64(clipLength) - 1)
        {
            yield return null;
        }
        EndCutscene();
    }

    private void EndCutscene()
    {
        StopVideo();
        InputManager.Instance.playerInput.currentActionMap = savedActionMap;
        int storyPoint = GameManager.Instance.storyPoint;
        if ((storyPoint == 2 || storyPoint == 6 || storyPoint == 10))
        {
            UpgradeMenu upgradeMenu = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Upgrades").GetComponent<UpgradeMenu>();
            upgradeMenu.SwitchTab(upgradeMenu.tabs[storyPoint == 2 ? 0 : storyPoint == 6 ? 1 : 2]);
            if (upgradeMenu.lerpDir == -1)
            {
                upgradeButton.onClick?.Invoke();
            }
        }
        GameManager.Instance.storyPoint++;
        Debug.Log("Story Point : " + GameManager.Instance.storyPoint);
        renderTexture.Release();
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        if (GameObject.Find("CloudCover") != null)
        {
            GameObject.Find("CloudCover").SetActive(false);
        }
        GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Map>().UpdateRegionsUnlocked();
        GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Map>().isLoaded = true;
        isPlayingCutscene = false;
        promptManager.disablePersistents = false;
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

    private void StopVideo()
    {
        foreach (VideoPlayer videoPlayer in videoPlayers)
        {
            videoPlayer.errorReceived -= VideoError;
            Destroy(videoPlayer);
        }
        videoPlayers.Clear();
        StopCoroutine("PlayVideo");
    }

    private void NextVideo()
    {
        if (videoPlayers.Count > currentScene && videoPlayers[currentScene] != null && videoPlayers[currentScene].isLooping && looped)
        {
            if (currentScene < videoPlayers.Count - 1)
            {
                StopCoroutine("PlayVideo");
                videoPlayers[currentCutscene].errorReceived -= VideoError;
                Destroy(videoPlayers[currentScene]);
                currentScene++;
                StartCoroutine("PlayVideo");
            }
            else
            {
                if (!(GameManager.Instance.musicMuted == 0))
                {
                    audioManager.PlayMusicWithFade("Map", musicCrossfadeTime);
                }
                EndCutscene();
            }
        }
    }

    private void LoopPoint(VideoPlayer videoPlayer)
    {
        if (!looped)
        {
            promptManager.Prompt(1);
        }
        looped = true;
    }

    private void VideoError(VideoPlayer videoPlayer, string message)
    {
        Debug.Log("Video Player Error : " + message);
        StopCoroutine("PlayVideo");
        StartCoroutine("PlayVideo");
    }

    void EditorUpdate()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard.vKey.wasPressedThisFrame)
        {
            PlayCutscene(testScene);
        }
        if (keyboard.bKey.wasPressedThisFrame && videoPlayers.Count > currentScene && videoPlayers[currentScene] != null)
        {
            videoPlayers[currentScene].frame = System.Convert.ToInt64(cutScenes[currentCutscene].scenes[currentScene].video.frameCount - fadeFramesBefore);
        }
    }

    [System.Serializable]
    public struct StoryScene
    {
        public VideoClip video;
        public string googleURL;
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
