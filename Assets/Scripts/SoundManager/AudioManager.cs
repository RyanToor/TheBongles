using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set;}

    #region Fields
    public List<Sound> music;
    public List<Sound> sFX;

    [HideInInspector]
    public AudioSource musicSource;
    [HideInInspector]
    public AudioSource musicSource2;
    
    private AudioSource sfxSource;
    private Slider volumeSlider;

    private bool firstMusicSourceIsPlaying = true;
    #endregion

    private void Awake()
    {
        // Make sure there isn't already an AudioManager in the scene
        if (instance == null)
        {
            instance = this;
            // Make sure we don't destroy this instance
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }


        // Create audio sources, and save them as references
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource2 = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 0.25f);
        musicSource2.volume = PlayerPrefs.GetFloat("MusicVolume", 0.25f);
        sfxSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1);

        if (SceneManager.GetActiveScene().name == "Map")
        {
            volumeSlider = GameObject.Find("UI/MainMenu/Settings/Music_Sound").GetComponent<Slider>();
            volumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.25f);
        }
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

    public void PlayMusic(string musicName, bool isFading = false)
    {
        // Determine which source is active
        AudioSource activeSource;
        if (isFading)
        {
            activeSource = firstMusicSourceIsPlaying ? musicSource2 : musicSource;
        }
        else
        {
            activeSource = firstMusicSourceIsPlaying ? musicSource : musicSource2;
        }
        firstMusicSourceIsPlaying = activeSource == musicSource;
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
        AudioSource activeSource = firstMusicSourceIsPlaying ? musicSource : musicSource2;
        AudioSource innactiveSource = firstMusicSourceIsPlaying ? musicSource2 : musicSource;

        PlayMusic(musicName, true);

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
                    activeSource.volume = (newSound.volume * musicVolume * (1 - t) / transitionTime);
                }
                innactiveSource.volume = (newSound.volume * musicVolume * t / transitionTime);
                yield return null;
            }
            innactiveSource.volume = newSound.volume * musicVolume;
            firstMusicSourceIsPlaying = (innactiveSource != musicSource2);
            activeSource.Stop();
        }
    }
    
    public void PlaySFX(string sound)
    {
        Sound newSound = FindSound(sFX, sound);
        sfxSource.volume = RandomiseValue(newSound.volume, newSound.volume, PlayerPrefs.GetFloat("SFXVolume", 1));
        sfxSource.pitch = RandomiseValue(newSound.pitch, newSound.pitchDeviation);
        sfxSource.PlayOneShot(newSound.clip, newSound.volume);
    }

    public void PlaySFXComplete(string sound)
    {
        Sound newSound = FindSound(sFX, sound);
        sfxSource.volume = RandomiseValue(newSound.volume, newSound.volume, PlayerPrefs.GetFloat("SFXVolume", 1));
        sfxSource.pitch = RandomiseValue(newSound.pitch, newSound.pitchDeviation);
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
        tempSource.pitch = RandomiseValue(newSound.pitch, newSound.pitchDeviation);
        tempSource.volume = RandomiseValue(newSound.volume, newSound.volume, PlayerPrefs.GetFloat("SFXVolume", 1));
        tempSource.Play();
        Destroy(tempObj, newSound.clip.length);
        return tempSource;
    }

    public void SetMasterVolume(float volume)
    {
        PlayerPrefs.SetFloat("MasterVolume", volume);
        musicSource.volume = volume * PlayerPrefs.GetFloat("MusicVolume", 1);
        musicSource2.volume = volume * PlayerPrefs.GetFloat("MusicVolume", 1);
        sfxSource.volume = volume * PlayerPrefs.GetFloat("SFXVolume", 1);
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        musicSource.volume = volume * PlayerPrefs.GetFloat("MasterVolume", 1);
        musicSource2.volume = volume * PlayerPrefs.GetFloat("MasterVolume", 1);
    }
    public void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat("SFXVolume", volume);
        sfxSource.volume = volume * PlayerPrefs.GetFloat("MasterVolume", 1);
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
    private float RandomiseValue(float baseValue, float deviation)
    {
        return baseValue + Random.Range(-deviation / 2, deviation / 2);
    }
    private float RandomiseValue(float baseValue, float deviation, float soundTypeVolume)
    {
        return PlayerPrefs.GetFloat("MasterVolume", 1) * PlayerPrefs.GetFloat("MasterVolume", 1) * soundTypeVolume * baseValue + Random.Range(-deviation / 2, deviation / 2);
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
    [Range(0f, 1f)]
    public float pitchDeviation = 0.5f;

    //Set if sound will loop
    public bool loop = false;
}