using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    #region Static Instance
    private static AudioManager instance;
    public static AudioManager Instance {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null)
                {
                    instance = new GameObject("SoundManager", typeof(AudioManager)).GetComponent<AudioManager>();
                }
            }
            return instance;
        }
        private set
        {
            instance = value;
        }
    }
    #endregion

    #region Fields
    public List<Sound> music;
    public List<Sound> sFX;

    private AudioSource musicSource;
    private AudioSource musicSource2;
    private AudioSource sfxSource;

    private Slider volumeSlider;

    private bool firstMusicSourceIsPlaying;
    #endregion

    private void Awake()
    {
        // Make sure there isn't already an AudioManager in the scene
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }

        // Make sure we don't destroy this instance
        DontDestroyOnLoad(gameObject);

        // Create audio sources, and save them as references
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource2 = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1);
        musicSource2.volume = PlayerPrefs.GetFloat("MusicVolume", 1);
        sfxSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1);

        volumeSlider = GameObject.Find("UI/MainMenu/Settings/Sounds").GetComponent<Slider>();
        volumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
    }

    private Sound FindSound(List<Sound> soundType, string name)
    {
        foreach (Sound sound in soundType)
        {
            if (sound.name == name)
            {
                return sound;
            }
        }
        Debug.Log("Sound not found in list : Have you selected the correct list, is the sound located there, and do the names match?");
        return null;
    }

    public void PlayMusic(string musicName)
    {
        musicSource.Stop();
        musicSource2.Stop();

        // Determine which source is active
        AudioSource activeSource = musicSource;
        firstMusicSourceIsPlaying = true;
        Sound musicSound = FindSound(music, musicName);

        activeSource.clip = musicSound.clip;
        activeSource.pitch = musicSound.pitch;
        activeSource.volume = musicSound.volume * PlayerPrefs.GetFloat("MusicVolume", 1);
        activeSource.loop = musicSound.loop;
        activeSource.Play();
    }
    public void PlayMusicWithFade(string musicName, float transitionTime = 1.0f)
    {
        // Determine which source is active
        AudioSource activeSource = (firstMusicSourceIsPlaying) ? musicSource : musicSource2;
        AudioSource innactiveSource = (firstMusicSourceIsPlaying) ? musicSource2 : musicSource;

        StartCoroutine(UpdateMusicWithFade(activeSource, innactiveSource, FindSound(music, musicName), transitionTime));
    }
    private IEnumerator UpdateMusicWithFade(AudioSource activeSource, AudioSource innactiveSource, Sound newSound, float transitionTime)
    {
        {
            float t = 0.0f;
            innactiveSource.clip = newSound.clip;
            innactiveSource.pitch = newSound.pitch;
            innactiveSource.loop = newSound.loop;
            float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1);

            // Fade out
            for (t = 0; t < transitionTime; t += Time.deltaTime)
            {
                if (activeSource.clip != null)
                {
                    activeSource.volume = (newSound.volume * musicVolume - (newSound.volume * musicVolume / transitionTime));
                }
                innactiveSource.volume = (t * newSound.volume * musicVolume / transitionTime);
                yield return null;
            }
            innactiveSource.volume = newSound.volume * musicVolume;
            firstMusicSourceIsPlaying = (innactiveSource == musicSource);
        }
    }
    
    public void PlaySFX(string sound)
    {
        Sound newSound = FindSound(sFX, sound);
        sfxSource.volume = newSound.volume;
        sfxSource.pitch = newSound.pitch;
        sfxSource.PlayOneShot(newSound.clip, newSound.volume);
    }

    public void PlaySFXComplete(string sound)
    {
        Sound newSound = FindSound(sFX, sound);
        sfxSource.volume = newSound.volume;
        sfxSource.pitch = newSound.pitch;
        if (!sfxSource.isPlaying)
        {
            sfxSource.PlayOneShot(newSound.clip, newSound.volume);
            sfxSource.pitch = newSound.pitch;
        }
    }

    public AudioSource PlaySFXAtLocation(string sound, Vector3 location)
    {
        Sound newSound = FindSound(sFX, sound);
        GameObject tempObj = new GameObject("TempAudio");
        tempObj.transform.position = location;
        AudioSource tempSource = tempObj.AddComponent<AudioSource>();
        tempSource.clip = newSound.clip;
        tempSource.pitch = newSound.pitch;
        tempSource.volume = newSound.volume;
        tempSource.Play();
        Destroy(tempObj, newSound.clip.length);
        return tempSource;
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        musicSource.volume = volume;
        musicSource2.volume = volume;
    }
    public void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat("SFXVolume", volume);
        sfxSource.volume = volume;
    }
    public void ToggleMusic()
    {
        if (musicSource.volume != 0)
        {
            musicSource.volume = 0;
            musicSource2.volume = 0;
        }
        else
        {
            musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1);
            musicSource2.volume = PlayerPrefs.GetFloat("MusicVolume", 1);
        }
    }
}

[System.Serializable]
public class Sound
{
    //Initialise name and audioclip and source
    public string name;
    public AudioClip clip;

    //Sliders and floats to affect sound playback
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.5f, 1.5f)]
    public float pitch = 1f;
    [Range(0f, 0.5f)]
    public float volumeDeviation = 0.1f;
    [Range(0f, 0.5f)]
    public float pitchDeviation = 0.5f;

    //Set if sound will loop
    public bool loop = false;
}