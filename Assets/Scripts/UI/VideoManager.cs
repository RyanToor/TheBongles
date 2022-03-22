using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public float musicCrossfadeTime, fadeFramesBefore;
    public Cutscene[] cutScenes;
    public int testScene;

    private VideoPlayer videoPlayer;
    private RawImage image;
    private AudioManager audioManager;
    private Image background;
    private bool isPlayingCutscene;
    public int currentScene = 0, currentCutscene = 0;

    // Start is called before the first frame update
    void Awake()
    {
        background = transform.Find("Background").GetComponent<Image>();
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
        EditorUpdate();
    }

    public void PlayCutscene(int cutscene)
    {
        currentCutscene = cutscene;
        GameObject.Find("BongleIsland").GetComponent<BongleIsland>().isInputEnabled = false;
        background.enabled = true;
        if (GameManager.Instance.musicMuted == 1 && GameManager.Instance.storyPoint != 13)
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
            if (cutScenes[currentCutscene].scenes[currentScene].isLooping)
            {
                GameObject.Find("UI/Prompts").GetComponent<InputPrompts>().VideoPrompt();
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
        if (!(GameManager.Instance.musicMuted == 0) && GameManager.Instance.storyPoint != 12)
        {
            audioManager.PlayMusicWithFade("Map", musicCrossfadeTime);
        }
        while (videoPlayer.frame < System.Convert.ToInt64(clip.frameCount) - 1)
        {
            yield return null;
        }
        GameManager.Instance.storyPoint++;
        Debug.Log("Story Point : " + GameManager.Instance.storyPoint);
        if (GameManager.Instance.storyPoint == 13)
        {
            StopCoroutine("PlayVideo");
            CheckCutscene();
            yield break;
        }
        videoPlayer.enabled = false;
        videoPlayer.targetTexture.Release();
        image.enabled = false;
        background.enabled = false;
        if (GameObject.Find("CloudCover") != null)
        {
            GameObject.Find("CloudCover").SetActive(false);
        }
        GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_Map>().UpdateRegionsUnlocked();
        GameObject.Find("UI/Prompts").GetComponent<InputPrompts>().StartPrompt();
        GameObject.FindGameObjectWithTag("Player").GetComponent<BongleIsland>().isInputEnabled = true;
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
