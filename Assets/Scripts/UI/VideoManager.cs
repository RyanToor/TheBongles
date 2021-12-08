using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public float musicCrossfadeTime, fadeFramesBefore;
    public VideoClip[] storyVideos;

    private VideoPlayer videoPlayer;
    private RawImage image;
    private AudioManager audioManager;
    private Image background;

    // Start is called before the first frame update
    void Awake()
    {
        background = transform.Find("Background").GetComponent<Image>();
        audioManager = GameObject.Find("SoundManager").GetComponent<AudioManager>();
        videoPlayer = GetComponent<VideoPlayer>();
        image = GetComponent<RawImage>();
        videoPlayer.loopPointReached += EndReached;
        videoPlayer.targetTexture.Release();
    }

    private void Start()
    {
        if (PlayerPrefs.GetInt("storyPoint", 0) == 2 && PlayerPrefs.GetInt("eelMet", 0) == 0)
        {
            PlayVideo(storyVideos[1]);
            PlayerPrefs.SetInt("eelMet", 1);
            GameObject.Find("UI/Upgrades").GetComponent<UpgradeMenu>().FlipLerpDir();
        }
    }

    private void Update()
    {
        EditorUpdate();
    }

    public void PlayVideo(VideoClip video)
    {
        background.enabled = true;
        videoPlayer.SetTargetAudioSource(0, audioManager.sfxSource);
        if (!videoPlayer.enabled)
        {
            videoPlayer.clip = video;
            videoPlayer.enabled = true;
            image.enabled = true;
        }
        if (!(PlayerPrefs.GetInt("MusicMuted", 1) == 0))
        {
            audioManager.PlayMusicWithFade("Story", musicCrossfadeTime);
        }
        StartCoroutine(CheckMusic(video));
    }

    void EndReached(VideoPlayer videoPlayer)
    {
        videoPlayer.enabled = false;
        videoPlayer.targetTexture.Release();
        image.enabled = false;
        background.enabled = false;
        if (videoPlayer.clip == storyVideos[0])
        {
            GameObject.Find("CloudCover").SetActive(false);
        }
    }

    private IEnumerator CheckMusic(VideoClip clip)
    {
        while (videoPlayer.frame < System.Convert.ToInt64(clip.frameCount) - fadeFramesBefore)
        {
            yield return null;
        }
        if (!(PlayerPrefs.GetInt("MusicMuted", 1) == 0))
        {
            audioManager.PlayMusicWithFade("Map", musicCrossfadeTime);
        }
    }

    void EditorUpdate()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            PlayVideo(storyVideos[0]);
        }
    }
}
