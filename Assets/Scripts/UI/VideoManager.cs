using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{

    public VideoClip testVideo;
    public VideoClip[] storyVideos;

    private VideoPlayer videoPlayer;
    private RawImage image;
    private AudioManager audioManager;
    private bool isMusicMuted;

    // Start is called before the first frame update
    void Awake()
    {
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
        }
    }

    private void Update()
    {
        EditorUpdate();
    }

    public void PlayVideo(VideoClip video)
    {
        isMusicMuted = audioManager.musicSource.volume == 0;
        if (!isMusicMuted)
        {
            audioManager.ToggleMusic();
        }
        if (!videoPlayer.enabled)
        {
            videoPlayer.clip = video;
            videoPlayer.enabled = true;
            image.enabled = true;
        }
    }

    void EndReached(VideoPlayer videoPlayer)
    {
        videoPlayer.enabled = false;
        videoPlayer.targetTexture.Release();
        image.enabled = false;
        if (!isMusicMuted)
        {
            audioManager.PlayMusic("Map");
            //audioManager.ToggleMusic();
        }
    }

    void EditorUpdate()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            PlayVideo(testVideo);
        }
    }
}
