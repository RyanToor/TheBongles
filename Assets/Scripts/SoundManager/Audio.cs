using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour
{
    [SerializeField] private AudioClip musicOriginal;
    [SerializeField] private AudioClip TrashHuntMusic;
    [SerializeField] private AudioClip OnClickSFX;
    [SerializeField] private AudioClip OnHoverSFX;
    [SerializeField] private AudioClip UIClose;
    [SerializeField] private AudioClip UIConfirm;
    [SerializeField] private AudioClip UITwinkle;
    [SerializeField] private AudioClip UIHarp;
    [SerializeField] private AudioClip UISoundCrinckle;

    private void Start()
    {
        AudioManager.Instance.PlayMusic(musicOriginal);
    }

    public void OnClick()
    {
        AudioManager.Instance.PlaySFX(OnClickSFX);
    }
    public void musicOn()
    {
        AudioManager.Instance.SetMusicVolume(1.0f);
    }
    public void musicOff()
    {
        AudioManager.Instance.SetMusicVolume(0.0f);
    }
    public void TrashHunt()
    {
        AudioManager.Instance.StopAllCoroutines();
        AudioManager.Instance.PlayMusic(TrashHuntMusic);
    }
}
