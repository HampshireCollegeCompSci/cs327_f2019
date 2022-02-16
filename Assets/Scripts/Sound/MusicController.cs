using System.Collections;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    // Audio players components
    public AudioSource soundTrack1;
    public AudioSource soundTrack2;

    // Music files
    public AudioClip menuMusic, themeMusic, transitionMusic, loseMusic, winMusic, aboutMusic;

    // Variables to keep track of the current playing song
    private bool playing1;
    private byte playingTrack;

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
        playing1 = false;
        playingTrack = 0;
        UpdateMaxVolume(PlayerPrefs.GetFloat(Constants.musicVolumeKey, 0.5f));
    }

    public void UpdateMaxVolume(float newVolume)
    {
        Debug.Log($"updating music volume to: {newVolume}");
        maxVolume = newVolume;

        if (playing1)
            soundTrack1.volume = maxVolume;
        else
            soundTrack2.volume = maxVolume;
    }

    public void PauseMusic()
    {
        if (playing1)
            soundTrack1.Pause();
        else
            soundTrack2.Pause();
    }

    public void PlayMusic()
    {
        if (playing1 && !soundTrack1.isPlaying)
            soundTrack1.Play();
        else if (!soundTrack2.isPlaying)
            soundTrack2.Play();
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
        if (playing1)
        {
            soundTrack2.clip = newTrack;
            StartCoroutine(FadeOut(soundTrack1));
            StartCoroutine(FadeIn(soundTrack2));
        }
        else
        {
            soundTrack1.clip = newTrack;
            StartCoroutine(FadeOut(soundTrack2));
            StartCoroutine(FadeIn(soundTrack1));
        }
        playing1 = !playing1;
    }

    // https://medium.com/@wyattferguson/how-to-fade-out-in-audio-in-unity-8fce422ab1a8
    public IEnumerator FadeOut(AudioSource audioSource, float FadeTime = 1.5f)
    {
        if (!audioSource.isPlaying) yield break;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= Time.deltaTime / FadeTime;
            yield return null;
        }
        audioSource.Stop();
        //audioSource.clip.UnloadAudioData();
    }

    public IEnumerator FadeIn(AudioSource audioSource, float FadeTime = 2f)
    {
        audioSource.volume = 0;
        audioSource.Play();
        while (audioSource.volume < maxVolume)
        {
            audioSource.volume += Time.deltaTime / FadeTime;
            yield return null;
        }
    }

}
