using UnityEngine;

public class AudioInterface : MonoBehaviour
{
    AudioManager audioManager;
    private void Awake()
    {
        audioManager = GameObject.Find("SoundManager").GetComponent<AudioManager>();
    }

    public void OnClick()
    {
        audioManager.PlaySFX("Click");
    }
    public void OnHover()
    {
        audioManager.PlaySFXComplete("Hover");
    }
    public void SetMusicVolume(float volume)
    {
        GameObject.Find("SoundManager").GetComponent<AudioManager>().SetMusicVolume(volume);
    }
    public void SetSFXVolume(float volume)
    {
        GameObject.Find("SoundManager").GetComponent<AudioManager>().SetSFXVolume(volume);
    }
    public void ToggleMusic()
    {
        audioManager.ToggleMusic();
    }

    public void ToggleSFX()
    {
        audioManager.ToggleSFX();
    }
}
