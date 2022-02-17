using System.Collections;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    // Audio players components
    public AudioSource audioSource_1;
    public AudioSource audioSource_2;

    // Music files
    public AudioClip menuMusic, themeMusic, transitionMusic, loseMusic, winMusic, aboutMusic;

    // Variables to keep track of the current playing song
    private byte playingTrack;
    private byte pausedAudioSource;

    private float maxVolume;

    // Singleton instance.
    public static MusicController Instance = null;

    // Initialize the singleton instance.
    private void Awake()
    {
        // If there is not already an instance, set it to this.
        if (Instance == null)
        {
            Instance = this;
        }
        //If an instance already exists, destroy whatever this object is to enforce the singleton.
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        //Set the GameObject to DontDestroyOnLoad so that it won't be destroyed when reloading our scene.
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        playingTrack = 0;
        UpdateMaxVolume(PlayerPrefs.GetFloat(Constants.musicVolumeKey));
    }

    public void UpdateMaxVolume(float newVolume)
    {
        Debug.Log($"updating music volume to: {newVolume}");
        maxVolume = newVolume;

        if (audioSource_1.isPlaying)
        {
            audioSource_1.volume = maxVolume;
        }
        else
        {
            audioSource_2.volume = maxVolume;
        }
    }

    public void PauseMusic()
    {
        if (audioSource_1.isPlaying)
        {
            pausedAudioSource = 1;
            audioSource_1.Pause();
        }
        else
        {
            pausedAudioSource = 2;
            audioSource_2.Pause();
        }
    }

    public void PlayMusic()
    {
        // playing a track that is already playing starts it from the beginning
        if (pausedAudioSource == 1 && !audioSource_1.isPlaying)
        {
            audioSource_1.Play();
        }
        else if (!audioSource_2.isPlaying)
        {
            audioSource_2.Play();
        }
    }

    public void MainMenuMusic()
    {
        if (playingTrack == 1)
        {
            PlayMusic();
            return;
        }

        playingTrack = 1;
        Transition(menuMusic);
    }

    public void GameMusic()
    {
        if (playingTrack == 2)
        {
            PlayMusic();
            return;
        }

        playingTrack = 2;
        Transition(themeMusic);
    }

    public void AlertMusic()
    {
        // will play over win/lose music without
        if (playingTrack == 3 || playingTrack != 2)
        {
            PlayMusic();
            return;
        }

        playingTrack = 3;
        Transition(transitionMusic);
    }

    public void LoseMusic()
    {
        if (playingTrack == 4)
        {
            PlayMusic();
            return;
        }

        playingTrack = 4;
        Transition(loseMusic);
    }

    public void WinMusic()
    {
        if (playingTrack == 5)
        {
            PlayMusic();
            return;
        }

        playingTrack = 5;
        Transition(winMusic);
    }

    public void AboutMusic()
    {
        if (playingTrack == 6)
        {
            PlayMusic();
            return;
        }

        playingTrack = 6;
        Transition(aboutMusic);
    }

    private void Transition(AudioClip newTrack)
    {
        Debug.Log($"Music Transition to: {newTrack.name}");

        StopAllCoroutines();
        if (audioSource_1.isPlaying)
        {
            audioSource_2.clip = newTrack;
            StartCoroutine(FadeOut(audioSource_1));
            StartCoroutine(FadeIn(audioSource_2));
        }
        else
        {
            audioSource_1.clip = newTrack;
            StartCoroutine(FadeOut(audioSource_2));
            StartCoroutine(FadeIn(audioSource_1));
        }
    }

    // https://medium.com/@wyattferguson/how-to-fade-out-in-audio-in-unity-8fce422ab1a8
    public IEnumerator FadeOut(AudioSource audioSource)
    {
        if (!audioSource.isPlaying) yield break;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= Time.deltaTime / Config.GameValues.musicFadeOut;
            yield return null;
        }
        audioSource.Stop();
        //audioSource.clip.UnloadAudioData();
    }

    public IEnumerator FadeIn(AudioSource audioSource)
    {
        audioSource.volume = 0;
        audioSource.Play();
        while (audioSource.volume < maxVolume)
        {
            audioSource.volume += Time.deltaTime / Config.GameValues.musicFadeIn;
            yield return null;
        }
    }

}
