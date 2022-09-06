using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    #region Fields
    public List<Sound> music;
    public List<Sound> sFX;

    [HideInInspector]
    public AudioSource musicSource;
    [HideInInspector]
    public AudioSource musicSource2;
    
    public AudioSource sfxSource;

    private bool firstMusicSourceIsPlaying = true;
    #endregion

    private void Awake()
    {
        // Make sure there isn't already an AudioManager in the scene
        if (Instance == null)
        {
            Instance = this;
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
    }

    private void Start()
    {
        musicSource.volume = GameManager.Instance.musicVolume * GameManager.Instance.musicMuted;
        musicSource2.volume = GameManager.Instance.musicVolume * GameManager.Instance.musicMuted;
        sfxSource.volume = GameManager.Instance.sFXVolume * GameManager.Instance.sFXMuted;
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
        if (musicName != "Silence")
        {
            Sound musicSound = FindSound(music, musicName);

            activeSource.clip = musicSound.clip;
            activeSource.pitch = musicSound.pitch;
            activeSource.volume = GameManager.Instance.musicVolume * GameManager.Instance.musicMuted;
            activeSource.loop = musicSound.loop;
            activeSource.Play();
        }
        else
        {
            activeSource.volume = 0;
        }
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
            innactiveSource.clip = newSound.clip;
            innactiveSource.pitch = newSound.pitch;
            innactiveSource.loop = newSound.loop;
            float musicVolume = GameManager.Instance.musicVolume * GameManager.Instance.musicMuted;

            // Fade out
            for (float t = 0; t < transitionTime; t += Time.deltaTime)
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
        if (!sfxSource.isPlaying)
        {
            sfxSource.volume = RandomiseValue(newSound.volume, newSound.volumeDeviation, GameManager.Instance.sFXVolume * GameManager.Instance.sFXMuted);
            sfxSource.pitch = RandomiseValue(newSound.pitch, newSound.pitchDeviation);
        }
        sfxSource.PlayOneShot(newSound.clip, newSound.volume);
    }

    public void PlayVideoSFX(AudioClip videoAudio)
    {
        sfxSource.volume = GameManager.Instance.sFXVolume * GameManager.Instance.sFXMuted;
        sfxSource.pitch = 1;
        sfxSource.PlayOneShot(videoAudio);
    }

    public void PlaySFXComplete(string sound)
    {
        Sound newSound = FindSound(sFX, sound);
        sfxSource.volume = RandomiseValue(newSound.volume, newSound.volumeDeviation, GameManager.Instance.sFXVolume * GameManager.Instance.sFXMuted);
        if (!sfxSource.isPlaying)
        {
            sfxSource.pitch = RandomiseValue(newSound.pitch, newSound.pitchDeviation);
        }
        if (!sfxSource.isPlaying)
        {
            sfxSource.PlayOneShot(newSound.clip, 1);
            sfxSource.pitch = newSound.pitch;
        }
    }

    public AudioSource PlaySFXAtLocation(string sound, Vector3 location, float radius)
    {
        Sound newSound = FindSound(sFX, sound);
        GameObject tempObj = new GameObject("TempAudio");
        tempObj.transform.position = location;
        AudioSource tempSource = tempObj.AddComponent<AudioSource>();
        tempSource.clip = newSound.clip;
        tempSource.pitch = RandomiseValue(newSound.pitch, newSound.pitchDeviation);
        tempSource.volume = RandomiseValue(newSound.volume, newSound.volumeDeviation, GameManager.Instance.sFXVolume * GameManager.Instance.sFXMuted);
        tempSource.spatialBlend = 1;
        tempSource.maxDistance = radius;
        tempSource.Play();
        Destroy(tempObj, newSound.clip.length);
        return tempSource;
    }

    public AudioSource PlayAudioAtObject(string sound, GameObject parentObject, float radius, bool loop = false, AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic)
    {
        Sound newSound = FindSound(sFX, sound);
        AudioSource audioSource = parentObject.AddComponent<AudioSource>();
        audioSource.clip = newSound.clip;
        audioSource.pitch = RandomiseValue(newSound.pitch, newSound.pitchDeviation);
        audioSource.volume = RandomiseValue(newSound.volume, newSound.volumeDeviation, GameManager.Instance.sFXVolume * GameManager.Instance.sFXMuted);
        audioSource.spatialBlend = 1;
        audioSource.maxDistance = radius;
        audioSource.rolloffMode = rolloffMode;
        audioSource.Play();
        audioSource.loop = loop;
        if (!loop)
        {
            Destroy(audioSource, newSound.clip.length);
        }
        return audioSource;
    }

    public void SetMusicVolume(float volume)
    {
        GameManager.Instance.musicMuted = 1;
        GameManager.Instance.musicVolume =  volume;
        musicSource.volume = GameManager.Instance.musicVolume;
        musicSource2.volume = musicSource.volume;
        GameManager.Instance.SaveSettings();
    }
    public void SetSFXVolume(float volume)
    {
        GameManager.Instance.sFXMuted = 1;
        GameManager.Instance.sFXVolume = volume;
        sfxSource.volume = volume;
        GameManager.Instance.SaveSettings();
    }
    public void ToggleMusic()
    {
        GameManager.Instance.musicMuted = Mathf.Abs(GameManager.Instance.musicMuted - 1);
        musicSource.volume = GameManager.Instance.musicVolume * GameManager.Instance.musicMuted;
        musicSource2.volume = musicSource.volume;
        musicSource.pitch = 1;
        musicSource2.pitch = 1;
        GameManager.Instance.SaveSettings();
    }
    public void ToggleSFX()
    {
        GameManager.Instance.sFXMuted = Mathf.Abs(GameManager.Instance.sFXMuted - 1);
        sfxSource.volume = GameManager.Instance.sFXVolume * GameManager.Instance.sFXMuted;
        sfxSource.pitch = 1;
        GameManager.Instance.SaveSettings();
    }
    private float RandomiseValue(float baseValue, float deviation)
    {
        return (baseValue + Random.Range(-deviation / 2, deviation / 2)) * GameManager.Instance.sFXMuted;
    }
    private float RandomiseValue(float baseValue, float deviation, float soundTypeVolume)
    {
        return (soundTypeVolume * baseValue + Random.Range(-deviation / 2, deviation / 2)) * GameManager.Instance.sFXMuted;
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