using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour
{
    [SerializeField] private AudioClip music;
    [SerializeField] private AudioClip OnClickSFX;
    [SerializeField] private AudioClip OnHoverSFX;

    private void Start()
    {
            AudioManager.Instance.PlayMusic(music);
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
}
