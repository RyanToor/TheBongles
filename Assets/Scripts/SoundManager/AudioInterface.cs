using UnityEngine;

public class AudioInterface : MonoBehaviour
{
    public void OnClick()
    {
        AudioManager.instance.PlaySFX("Click");
    }
    public void OnHover()
    {
        AudioManager.instance.PlaySFXComplete("Hover");
    }
    public void SetMusicVolume(float volume)
    {
        AudioManager.instance.SetMusicVolume(volume);
    }
    public void SetSFXVolume(float volume)
    {
        AudioManager.instance.SetSFXVolume(volume);
    }
    public void ToggleMusic()
    {
        AudioManager.instance.ToggleMusic();
    }

    public void ToggleSFX()
    {
        AudioManager.instance.ToggleSFX();
    }
}
