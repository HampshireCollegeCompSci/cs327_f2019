using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class MusicController : MonoBehaviour, ISound
{
    // Singleton instance.
    public static MusicController Instance;

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
    private byte _playingTrack, _audioSourcePlaying;
    [SerializeField]
    private bool _muted, _paused;

    private float maxVolume;
    private Coroutine muteDelyCoroutine, pauseDelyCoroutine, fadeInCoroutine, fadeOutCoroutine;

    // Initialize the singleton instance.
    void Awake()
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

    void Start()
    {
        audioClips = new AudioClip[7]
        {
            menuMusic, themeMusic, transitionMusic, loseMusic, winMusic, aboutMusic, tutorialMusic
        };
        AudioSourcePlaying = 1;
        _playingTrack = byte.MaxValue;
        _muted = false;
        _paused = false;

        audioMixer.SetFloat(Constants.audioMixerNameTrack1, -80);
        audioMixer.SetFloat(Constants.audioMixerNameTrack2, -80);
        UpdateMaxVolume(PlayerPrefKeys.GetMusicVolume());
    }

    private byte PlayingTrack
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
            }
            else
            {
                StopAllCoroutines();
                if (AudioSourcePlaying == 1)
                {
                    audioSource_2.clip = newTrack;
                    StartFadeIn(Constants.audioMixerNameTrack2, Config.GameValues.musicFadeInDurationSec, audioSource_2);
                    StartFadeOut(Constants.audioMixerNameTrack1, Config.GameValues.musicFadeOutDurationSec, audioSource_1);
                    AudioSourcePlaying = 2;
                }
                else
                {
                    audioSource_1.clip = newTrack;
                    StartFadeIn(Constants.audioMixerNameTrack1, Config.GameValues.musicFadeInDurationSec, audioSource_1);
                    StartFadeOut(Constants.audioMixerNameTrack2, Config.GameValues.musicFadeOutDurationSec, audioSource_2);
                    AudioSourcePlaying = 1;
                }
            }
        }
    }

    private byte AudioSourcePlaying
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
                {
                    StopCoroutine(fadeInCoroutine);
                }
                audioMixer.SetFloat(Constants.audioMixerNameMaster, -80);
                muteDelyCoroutine = StartCoroutine(PauseDelay());
                Debug.Log("muted music");
            }
            else
            {
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
                StopCoroutine(fadeInCoroutine);
                audioMixer.SetFloat(Constants.audioMixerNameMaster, -80);
                pauseDelyCoroutine = StartCoroutine(PauseDelay());
                Debug.Log("paused music");
            }
            else
            {
                audioMixer.SetFloat(Constants.audioMixerNameMaster, Mathf.Log10(maxVolume) * 20);
                StopCoroutine(pauseDelyCoroutine);
                // note: playing a track that is already playing starts it from the beginning
                FadeMusicIn();
                Debug.Log("unpaused music");
            }
        }
    }

    public void UpdateMaxVolume(float newVolume)
    {
        Debug.Log($"setting music volume to: {newVolume}");
        //NormalizeFadeValues(newVolume);
        if (newVolume != 0)
        {
            if (Muted)
            {
                Muted = false;
            }

            maxVolume = newVolume;
            audioMixer.SetFloat(Constants.audioMixerNameMaster, Mathf.Log10(newVolume) * 20);
        }
        else
        {
            Muted = true;
        }
    }

    public void FadeMusicOut()
    {
        if (Muted || Paused) return;
        StopCoroutine(fadeInCoroutine);
        if (AudioSourcePlaying == 1)
        {
            StartFadeOut(Constants.audioMixerNameTrack1, Config.GameValues.musicFadeOutSlowDurationSec, audioSource_1);
        }
        else
        {
            StartFadeOut(Constants.audioMixerNameTrack2, Config.GameValues.musicFadeOutSlowDurationSec, audioSource_2);
        }
    }

    /// <summary>
    /// Fade the current music back in from its current state.
    /// </summary>
    public void FadeMusicIn()
    {
        if (Muted || Paused) return;
        if (AudioSourcePlaying == 1)
        {
            //audioMixer.SetFloat(Constants.audioMixerNameTrack1, 0.002f);
            StartFadeIn(Constants.audioMixerNameTrack1, Config.GameValues.musicFadeInDurationSec, audioSource_1);
        }
        else
        {
            //audioMixer.SetFloat(Constants.audioMixerNameTrack2, 0.002f);
            StartFadeIn(Constants.audioMixerNameTrack2, Config.GameValues.musicFadeInDurationSec, audioSource_2);
        }
        //else
        //{
        //    Transition(audioClips[PlayingTrack]);
        //}
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
        AudioSource audioSource;
        if (AudioSourcePlaying == 1)
        {
            audioSource = audioSource_1;
            audioMixer.SetFloat(Constants.audioMixerNameTrack1, -80);
        }
        else
        {
            audioSource = audioSource_2;
            audioMixer.SetFloat(Constants.audioMixerNameTrack2, -80);
        }

        // to prevent audio blips lower the volume first and then pause the music
        yield return new WaitForSecondsRealtime(0.1f);
        audioSource.Pause();
    }

    private void StartFadeIn(string fadeInAudioMixerName, float duration, AudioSource audioSource)
    {
        fadeInCoroutine = StartCoroutine(FadeMixerGroup.StartFade(audioMixer, fadeInAudioMixerName, duration, 1, audioSource, false));
    }

    private void StartFadeOut(string fadeOutAudioMixerName, float duration, AudioSource audioSource)
    {
        fadeOutCoroutine = StartCoroutine(FadeMixerGroup.StartFade(audioMixer, fadeOutAudioMixerName, duration, 0, audioSource, true));
    }
}
