using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public float musicCrossfadeTime, fadeFramesBefore;
    public Cutscene[] cutScenes;

    private VideoPlayer videoPlayer;
    private RawImage image;
    private AudioManager audioManager;
    private Image background;
    private bool isPlayingCutscene;
    public int currentScene = 0;

    // Start is called before the first frame update
    void Awake()
    {
        background = transform.Find("Background").GetComponent<Image>();
        audioManager = GameObject.Find("SoundManager").GetComponent<AudioManager>();
        videoPlayer = GetComponent<VideoPlayer>();
        image = GetComponent<RawImage>();
        videoPlayer.targetTexture.Release();
    }

    private void Start()
    {
        if (PlayerPrefs.GetInt("storyPoint", 0) == 2 && PlayerPrefs.GetInt("eelMet", 0) == 0)
        {
            PlayCutscene(1);
            PlayerPrefs.SetInt("eelMet", 1);
            GameObject.Find("UI/Upgrades").GetComponent<UpgradeMenu>().FlipLerpDir();
        }
        videoPlayer.SetTargetAudioSource(0, audioManager.sfxSource);
    }

    private void Update()
    {
        EditorUpdate();
    }

    public void PlayCutscene(int cutscene)
    {
        GameObject.Find("BongleIsland").GetComponent<BongleIsland>().isInputEnabled = false;
        background.enabled = true;
        if (PlayerPrefs.GetInt("MusicMuted", 1) == 1)
        {
            audioManager.PlayMusicWithFade("Story", musicCrossfadeTime);
        }
        isPlayingCutscene = true;
        StartCoroutine(PlayVideo(cutscene));
    }

    public IEnumerator PlayVideo(int cutScene)
    {
        if (isPlayingCutscene)
        {
            if (cutScenes[cutScene].scenes[currentScene].audio != null)
            {
                audioManager.PlayVideoSFX(cutScenes[cutScene].scenes[currentScene].audio);
            }
            VideoClip video = cutScenes[cutScene].scenes[currentScene].video;
            if (!videoPlayer.enabled)
            {
                videoPlayer.enabled = true;
                image.enabled = true;
            }
            videoPlayer.clip = video;
            videoPlayer.isLooping = cutScenes[cutScene].scenes[currentScene].isLooping;
            videoPlayer.frame = 0;
            videoPlayer.Play();
            if (currentScene == cutScenes[cutScene].scenes.Length - 1)
            {
                StartCoroutine(CheckEnd(video));
                currentScene = 0;
            }
            else
            {
                currentScene++;
            }
            while (videoPlayer.frame < System.Convert.ToInt64(video.frameCount) || videoPlayer.isLooping)
            {
                yield return null;
                if (Input.GetAxis("Jump") == 1 || Input.GetMouseButtonDown(0))
                {
                    videoPlayer.isLooping = false;
                }
            }
            StartCoroutine(PlayVideo(cutScene));
        }
    }

    private IEnumerator CheckEnd(VideoClip clip)
    {
        isPlayingCutscene = false;
        while (videoPlayer.frame < System.Convert.ToInt64(clip.frameCount) - fadeFramesBefore)
        {
            yield return null;
        }
        if (!(PlayerPrefs.GetInt("MusicMuted", 1) == 0))
        {
            audioManager.PlayMusicWithFade("Map", musicCrossfadeTime);
        }
        while (videoPlayer.frame < System.Convert.ToInt64(clip.frameCount) - 1)
        {
            yield return null;
        }
        videoPlayer.enabled = false;
        videoPlayer.targetTexture.Release();
        image.enabled = false;
        background.enabled = false;
        if (GameObject.Find("CloudCover") != null)
        {
            GameObject.Find("CloudCover").SetActive(false);
        }
        GameObject.Find("BongleIsland").GetComponent<BongleIsland>().isInputEnabled = true;
    }

    void EditorUpdate()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            PlayCutscene(1);
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
        public StoryScene[] scenes;
    }
}
