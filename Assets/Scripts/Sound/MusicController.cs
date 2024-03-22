using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class MusicController : MonoBehaviour, ISound
{
    // Singleton instance.
    public static MusicController Instance { get; private set; }
    private static readonly WaitForSecondsRealtime pauseDelay = new(0.05f);

    [SerializeField]
    private AudioMixer audioMixer;

    // Audio players components
    [SerializeField]
    private AudioSource audioSource_1, audioSource_2;

    // Music files
    [SerializeField]
    private AudioClip menuMusic, themeMusic, transitionMusic, loseMusic, winMusic, aboutMusic, tutorialMusic;
    private AudioClip[] audioClips;

    [SerializeField]
    private int _playingTrack, _audioSourcePlaying;
    [SerializeField]
    private bool _muted, _paused;

    private float maxVolume;
    private Coroutine muteDelyCoroutine, pauseDelyCoroutine, fadeInCoroutine, fadeOutCoroutine;

    // Initialize the singleton instance.
    void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        // make instance persist across scenes
        DontDestroyOnLoad(this.gameObject);

        audioClips = new AudioClip[7]
        {
            menuMusic, themeMusic, transitionMusic, loseMusic, winMusic, aboutMusic, tutorialMusic
        };
        AudioSourcePlaying = 1;
        _playingTrack = -1;
        _muted = false;
        _paused = false;
    }

    void Start()
    {
        audioMixer.SetFloat(Constants.AudioMixerNames.track1, -80);
        audioMixer.SetFloat(Constants.AudioMixerNames.track2, -80);
        UpdateMaxVolume(PersistentSettings.MusicVolume);
    }

    private int PlayingTrack
    {
        get => _playingTrack;
        set
        {
            if (value == _playingTrack)
            {
                //PlayMusic();
                return;
            }

            _playingTrack = value;
            AudioClip newTrack = audioClips[value];
            Debug.Log($"Music Transition to: {newTrack.name}");
            if (Muted || Paused)
            {
                if (AudioSourcePlaying == 1)
                {
                    audioSource_1.clip = newTrack;
                }
                else
                {
                    audioSource_2.clip = newTrack;
                }
                return;
            }

            StopAllCoroutines();
            AudioSource audioSourceFadeIn, audioSourceFadeOut;
            string trackFadeIn, trackFadeOut;
            if (AudioSourcePlaying == 1)
            {
                audioSourceFadeIn = audioSource_2;
                audioSourceFadeOut = audioSource_1;
                trackFadeIn = Constants.AudioMixerNames.track2;
                trackFadeOut = Constants.AudioMixerNames.track1;
                AudioSourcePlaying = 2;
            }
            else
            {
                audioSourceFadeIn = audioSource_1;
                audioSourceFadeOut = audioSource_2;
                trackFadeIn = Constants.AudioMixerNames.track1;
                trackFadeOut = Constants.AudioMixerNames.track2;
                AudioSourcePlaying = 1;
            }

            StartFadeIn(trackFadeIn, GameValues.Music.musicFadeInDurationSec, audioSourceFadeIn, newTrack);
            StartFadeOut(trackFadeOut, GameValues.Music.musicFadeOutDurationSec, audioSourceFadeOut);
        }
    }

    private int AudioSourcePlaying
    {
        get => _audioSourcePlaying;
        set => _audioSourcePlaying = value;
    }

    private bool Muted
    {
        get => _muted;
        set
        {
            _muted = value;
            if (value)
            {
                if (fadeInCoroutine != null)
                    StopCoroutine(fadeInCoroutine);
                if (fadeOutCoroutine != null) 
                   StopCoroutine(fadeOutCoroutine);
                audioMixer.SetFloat(Constants.AudioMixerNames.master, -80);
                muteDelyCoroutine = StartCoroutine(PauseDelay());
                Debug.Log("muted music");
            }
            else
            {
                if (muteDelyCoroutine != null)
                    StopCoroutine(muteDelyCoroutine);
                FadeMusicIn();
                Debug.Log("unmuted music");
            }
        }
    }

    public bool Paused
    {
        get => _paused;
        set
        {
            _paused = value;
            if (Muted) return;
            if (value)
            {
                if (fadeInCoroutine != null)
                    StopCoroutine(fadeInCoroutine);
                if (fadeOutCoroutine != null)
                    StopCoroutine(fadeOutCoroutine);
                audioMixer.SetFloat(Constants.AudioMixerNames.master, -80);
                pauseDelyCoroutine = StartCoroutine(PauseDelay());
                Debug.Log("paused music");
            }
            else
            {
                audioMixer.SetFloat(Constants.AudioMixerNames.master, Mathf.Log10(maxVolume) * 20);
                if (pauseDelyCoroutine != null)
                    StopCoroutine(pauseDelyCoroutine);
                // note: playing a track that is already playing starts it from the beginning
                FadeMusicIn();
                Debug.Log("unpaused music");
            }
        }
    }

    public void UpdateMaxVolume(int newVolume)
    {
        Debug.Log($"setting music volume to: {newVolume}");
        //NormalizeFadeValues(newVolume);
        if (newVolume != 0)
        {
            if (Muted)
            {
                Muted = false;
            }

            maxVolume = (float)newVolume / GameValues.Settings.musicVolumeDenominator * GameValues.Music.musicLimit;
            audioMixer.SetFloat(Constants.AudioMixerNames.master, Mathf.Log10(maxVolume) * 20);
        }
        else
        {
            Muted = true;
        }
    }

    /// <summary>
    /// Fade the current music out from its current state.
    /// </summary>
    public void FadeMusicOut()
    {
        if (Muted || Paused) return;
        if (fadeInCoroutine != null)
            StopCoroutine(fadeInCoroutine);
        if (AudioSourcePlaying == 1)
        {
            StartFadeOut(Constants.AudioMixerNames.track1, GameValues.Music.musicFadeOutSlowDurationSec, audioSource_1);
        }
        else
        {
            StartFadeOut(Constants.AudioMixerNames.track2, GameValues.Music.musicFadeOutSlowDurationSec, audioSource_2);
        }
    }

    /// <summary>
    /// Fade the current music back in from its current state.
    /// </summary>
    public void FadeMusicIn()
    {
        if (Muted || Paused) return;
        if (fadeOutCoroutine != null)
            StopCoroutine(fadeOutCoroutine);
        if (AudioSourcePlaying == 1)
        {
            //audioMixer.SetFloat(Constants.audioMixerNameTrack1, 0.002f);
            StartFadeIn(Constants.AudioMixerNames.track1, GameValues.Music.musicFadeInDurationSec, audioSource_1);
        }
        else
        {
            //audioMixer.SetFloat(Constants.audioMixerNameTrack2, 0.002f);
            StartFadeIn(Constants.AudioMixerNames.track2, GameValues.Music.musicFadeInDurationSec, audioSource_2);
        }
    }

    public void PlayMusic()
    {
        if (Muted) return;
        // playing a track that is already playing starts it from the beginning
        if (AudioSourcePlaying == 1 && !audioSource_1.isPlaying)
        {
            audioSource_1.Play();
        }
        else if (AudioSourcePlaying == 2 && !audioSource_2.isPlaying)
        {
            audioSource_2.Play();
        }
    }

    public void MainMenuMusic()
    {
        PlayingTrack = 0;
    }

    public void GameMusic(bool noOverrideAlert = false)
    {
        // continuing a new game can trigger the alert music to play before
        // gameplay officially begins so don't override its playback
        if (noOverrideAlert && PlayingTrack == 2)
        {
            return;
        }

        PlayingTrack = 1;
    }

    public void AlertMusic()
    {
        PlayingTrack = 2;
    }

    public void LoseMusic()
    {
        PlayingTrack = 3;
    }

    public void WinMusic()
    {
        PlayingTrack = 4;
    }

    public void AboutMusic()
    {
        PlayingTrack = 5;
    }

    public void TutorialMusic()
    {
        PlayingTrack = 6;
    }

    private IEnumerator PauseDelay()
    {
        audioMixer.SetFloat(Constants.AudioMixerNames.track1, -80);
        audioMixer.SetFloat(Constants.AudioMixerNames.track2, -80);
        // to prevent audio blips lower the volume first and then pause the music
        yield return pauseDelay;
        audioSource_1.Pause();
        audioSource_2.Pause();
    }

    private void StartFadeIn(string fadeInAudioMixerName, float duration, AudioSource audioSource, AudioClip newClip = null)
    {
        FadeMixerGroup.FadeType fadeType = FadeMixerGroup.FadeType.play;
        if (newClip != null)
        {
            bool sameClip = newClip.Equals(audioSource.clip);
            if (sameClip)
            {
                fadeType = FadeMixerGroup.FadeType.persist;
            }
            else
            {
                audioSource.clip = newClip;
            }
        }

        fadeInCoroutine = StartCoroutine(FadeMixerGroup.StartFade(audioMixer,
            fadeInAudioMixerName, duration, 1, audioSource, fadeType));
    }

    private void StartFadeOut(string fadeOutAudioMixerName, float duration, AudioSource audioSource)
    {
        fadeOutCoroutine = StartCoroutine(FadeMixerGroup.StartFade(audioMixer,
            fadeOutAudioMixerName, duration, 0, audioSource, FadeMixerGroup.FadeType.stop));
    }
}
