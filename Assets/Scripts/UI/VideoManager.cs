using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{

    public VideoClip testVideo;

    private VideoPlayer videoPlayer;
    private RawImage image;

    // Start is called before the first frame update
    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        image = GetComponent<RawImage>();
        videoPlayer.loopPointReached += EndReached;
        videoPlayer.targetTexture.Release();
    }

    private void Update()
    {
        EditorUpdate();
    }

    public void PlayVideo(VideoClip video)
    {
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
    }

    void EditorUpdate()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            PlayVideo(testVideo);
        }
    }
}
