using System.Collections;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    // Audio players components
    public AudioSource audioSource_1;
    public AudioSource audioSource_2;

    // Music files
    public AudioClip menuMusic, themeMusic, transitionMusic, loseMusic, winMusic, aboutMusic, tutorialMusic;

    // Variables to keep track of the current playing song
    private byte playingTrack;
    private byte pausedAudioSource;

    private float fadeInSpeed;
    private float fadeOutSpeedFast;
    private float fadeOutSpeedSlow;
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
            DontDestroyOnLoad(gameObject);
        }
        //If an instance already exists, destroy whatever this object is to enforce the singleton.
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playingTrack = 0;
        UpdateMaxVolume(PlayerPrefs.GetFloat(Constants.musicVolumeKey));
    }

    public void UpdateMaxVolume(float newVolume)
    {
        Debug.Log($"setting music volume to: {newVolume}");
        NormalizeFadeValues();
        maxVolume = newVolume;

        Debug.Log($"1: {audioSource_1.isPlaying}, 2: {audioSource_2.isPlaying}");

        if (audioSource_1.isPlaying || pausedAudioSource == 1)
        {
            audioSource_1.volume = maxVolume;
        }
        else if (audioSource_2.isPlaying || pausedAudioSource == 2)
        {
            audioSource_2.volume = maxVolume;
        }
        else
        {
            Debug.LogWarning("tried to update max volume on no music");
        }
    }

    private void NormalizeFadeValues()
    {
        // changing the music volume requires that the fade timings be updated as well 
        double audioDifference = PlayerPrefs.GetFloat(Constants.musicVolumeKey) / 0.5;
        fadeInSpeed = (float)(Config.GameValues.musicFadeIn * audioDifference);
        fadeOutSpeedFast = (float)(Config.GameValues.musicFadeOutFast * audioDifference);
        fadeOutSpeedSlow = (float)(Config.GameValues.musicFadeOutSlow * audioDifference);
    }

    public void FadeMusicOut()
    {
        StopAllCoroutines();
        if (audioSource_1.isPlaying)
        {
            StartCoroutine(FadeOut(audioSource_1, fadeOutSpeedSlow));
        }
        else if (audioSource_2.isPlaying)
        {
            StartCoroutine(FadeOut(audioSource_2, fadeOutSpeedSlow));
        }
    }

    public void PauseMusic()
    {
        if (audioSource_1.isPlaying)
        {
            pausedAudioSource = 1;
            audioSource_1.Pause();
        }
        else if (audioSource_2.isPlaying)
        {
            pausedAudioSource = 2;
            audioSource_2.Pause();
        }
        else
        {
            Debug.LogWarning("tried to pause no music");
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
        pausedAudioSource = 0;
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

    public void GameMusic(bool noOverrideAlert = false)
    {
        // continuing a new game can trigger the alert music to play before
        // gameplay begins so don't override its playback
        if (playingTrack == 2 || (playingTrack == 3 && noOverrideAlert))
        {
            PlayMusic();
            return;
        }

        playingTrack = 2;
        Transition(themeMusic);
    }

    public void AlertMusic()
    {
        if (playingTrack == 3)
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

    public void TutorialMusic()
    {
        if (playingTrack == 7)
        {
            PlayMusic();
            return;
        }

        playingTrack = 7;
        Transition(tutorialMusic);
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

    private IEnumerator FadeOut(AudioSource audioSource, float fadeOutSpeed = 0)
    {
        if (!audioSource.isPlaying)
        {
            yield break;
        }

        if (fadeOutSpeed == 0)
        {
            fadeOutSpeed = fadeOutSpeedFast;
        }

        while (audioSource.volume > 0)
        {
            audioSource.volume -= Time.deltaTime * fadeOutSpeed;
            yield return null;
        }
        audioSource.Stop();
        //audioSource.clip.UnloadAudioData();
    }

    private IEnumerator FadeIn(AudioSource audioSource)
    {
        audioSource.volume = 0;
        audioSource.time = 0;
        audioSource.Play();
        while (audioSource.volume < maxVolume)
        {
            audioSource.volume += Time.deltaTime * fadeInSpeed;
            yield return null;
        }
    }
}
