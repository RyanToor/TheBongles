using UnityEngine;

public class AudioInterface : MonoBehaviour
{
    public void OnClick()
    {
        AudioManager.Instance.PlaySFX("Click");
    }
    public void OnHover()
    {
        AudioManager.Instance.PlaySFXComplete("Hover");
    }
    public void SetMusicVolume(float volume)
    {
        AudioManager.Instance.SetMusicVolume(volume);
    }
    public void SetSFXVolume(float volume)
    {
        AudioManager.Instance.SetSFXVolume(volume);
    }
    public void ToggleMusic()
    {
        AudioManager.Instance.ToggleMusic();
    }

    public void ToggleSFX()
    {
        AudioManager.Instance.ToggleSFX();
    }
}
