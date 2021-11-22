using UnityEngine;

public class AudioInterface : MonoBehaviour
{
    AudioManager audioManager;
    private void Start()
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
        audioManager.SetMusicVolume(volume);
    }
    public void ToggleMusic()
    {
        audioManager.ToggleMusic();
    }
}
