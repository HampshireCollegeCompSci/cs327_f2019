using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using static FadeMixerGroup;

public class MusicController : MonoBehaviour, ISound
{
    // Singleton instance.
    public static MusicController Instance { get; private set; }
    private static readonly WaitForSecondsRealtime muteDelay = new(GameValues.Audio.volumeDelay + 0.05f);

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
    private int _musicTrack;
    [SerializeField]
    private bool _muted, _paused, pendingTrackChange;

    private bool fastUnmute;
    private float maxVolume;
    private Coroutine muteDelyCoroutine, fadeInCoroutine, fadeOutCoroutine, volumeChangeCoroutine;

    // Initialize the singleton instance.
    void Awake()
    {
        if (Instance != null) return;
        Instance = this;

        audioClips = new AudioClip[7]
        {
            menuMusic, themeMusic, transitionMusic, loseMusic, winMusic, aboutMusic, tutorialMusic
        };
        AudioSourcePlaying = 1;
        _musicTrack = -1;
    }

    void Start()
    {
        //audioMixer.SetFloat(Constants.AudioMixerNames.master, -80);
        audioMixer.SetFloat(Constants.AudioMixerNames.track1, -80);
        audioMixer.SetFloat(Constants.AudioMixerNames.track2, -80);
        UpdateMaxVolume(PersistentSettings.MusicVolume);
    }

    private int MusicTrack
    {
        get => _musicTrack;
        set
        {
            if (value == _musicTrack) return;
            _musicTrack = value;
            ChangeTrack();
        }
    }

    private int AudioSourcePlaying;

    public bool Muted
    {
        get => _muted;
        set
        {
            if (value == _muted) return;
            _muted = value;
            if (value)
            {
                UpdateMixerVolume(GameValues.Audio.volumeDelay, 0);
                //audioMixer.SetFloat(Constants.AudioMixerNames.master, -80);
                muteDelyCoroutine = StartCoroutine(MuteDelay());
                Debug.Log("muted music");
                return;
            }

            if (maxVolume == 0) return;

            float duration = GameValues.Music.musicFadeInDurationSec;
            if (fastUnmute)
            {
                duration = GameValues.Audio.volumeDelay;
                fastUnmute = false;
            }

            // tracks are not updated completely while muted so they must be properly updated when unmuted
            // also if muted at game start the audio mixer will not increase the track volume
            if (pendingTrackChange)
            {
                FastChangeTrack();
                pendingTrackChange = false;
            }
            else
            {
                PlayMusic();
            }

            UpdateMixerVolume(duration, maxVolume);
            Debug.Log("unmuted music");
        }
    }

    public bool Paused
    {
        get => _paused;
        set
        {
            if (value == _paused) return;
            _paused = value;
            if (maxVolume == 0) return;
            Muted = value;
        }
    }

    public void UpdateMaxVolume(int newVolume)
    {
        // 0 to 1 * limit
        maxVolume = (float)newVolume / GameValues.Settings.musicVolumeDenominator * GameValues.Music.musicLimit;
        Debug.Log($"updating music volume to: {newVolume}, muted: {Muted}, paused: {Paused}");

        if (maxVolume == 0)
        {
            Muted = true;
            return;
        }

        if (Paused) return;
        if (Muted)
        {
            fastUnmute = true;
            Muted = false;
            return;
        }

        UpdateMixerVolume(GameValues.Audio.volumeDelay, maxVolume);
    }

    /// <summary>
    /// Fade the current music out from its current state.
    /// </summary>
    public void FadeMusicOut()
    {
        if (Muted) return;
        StopFadeCoroutines();

        if (AudioSourcePlaying == 1)
            StartFadeOut(Constants.AudioMixerNames.track1, GameValues.Music.musicFadeOutSlowDurationSec, audioSource_1);
        else
            StartFadeOut(Constants.AudioMixerNames.track2, GameValues.Music.musicFadeOutSlowDurationSec, audioSource_2);
    }

    /// <summary>
    /// Fade the current music back in from its current state.
    /// </summary>
    public void FadeMusicIn()
    {
        if (Muted) return;
        StopFadeCoroutines();

        if (AudioSourcePlaying == 1)
            StartFadeIn(Constants.AudioMixerNames.track1, GameValues.Music.musicFadeInDurationSec, audioSource_1);
        else
            StartFadeIn(Constants.AudioMixerNames.track2, GameValues.Music.musicFadeInDurationSec, audioSource_2);
    }

    public void MainMenuMusic()
    {
        MusicTrack = 0;
    }

    public void GameMusic(bool noOverrideAlert = false)
    {
        // continuing a new game can trigger the alert music to play before
        // gameplay officially begins so don't override its playback
        if (noOverrideAlert && MusicTrack == 2)
        {
            return;
        }

        MusicTrack = 1;
    }

    public void AlertMusic()
    {
        MusicTrack = 2;
    }

    public void LoseMusic()
    {
        MusicTrack = 3;
    }

    public void WinMusic()
    {
        MusicTrack = 4;
    }

    public void AboutMusic()
    {
        MusicTrack = 5;
    }

    public void TutorialMusic()
    {
        MusicTrack = 6;
    }

    private void ChangeTrack()
    {
        if (Muted)
        {
            Debug.Log($"pending track change");
            pendingTrackChange = true;
            return;
        }

        AudioClip newTrack = audioClips[MusicTrack];
        Debug.Log($"Music Transition to: {newTrack.name}");

        StopFadeCoroutines();
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

    private void FastChangeTrack()
    {
        AudioClip newTrack = audioClips[MusicTrack];
        Debug.Log($"Fast Music Transition to: {newTrack.name}");
        StopFadeCoroutines();

        if (newTrack.Equals(audioSource_1.clip))
        {
            Debug.Log("found existing audio source 2");
            audioMixer.SetFloat(Constants.AudioMixerNames.track1, 0);
            audioMixer.SetFloat(Constants.AudioMixerNames.track2, -80);
            audioSource_1.Play();
            AudioSourcePlaying = 1;
        }
        else if (newTrack.Equals(audioSource_2.clip))
        {
            Debug.Log("found existing audio source 1");
            audioMixer.SetFloat(Constants.AudioMixerNames.track1, -80);
            audioMixer.SetFloat(Constants.AudioMixerNames.track2, 0);
            audioSource_2.Play();
            AudioSourcePlaying = 2;
        }
        else
        {
            Debug.Log("didn't find existing audio source");
            audioMixer.SetFloat(Constants.AudioMixerNames.track1, 0);
            audioMixer.SetFloat(Constants.AudioMixerNames.track2, -80);
            audioSource_1.clip = newTrack;
            audioSource_1.Play();
            AudioSourcePlaying = 1;
        }
    }

    private void PlayMusic()
    {
        // playing a track that is already playing starts it from the beginning
        if (AudioSourcePlaying == 1 && !audioSource_1.isPlaying)
            audioSource_1.Play();
        else if (AudioSourcePlaying == 2 && !audioSource_2.isPlaying)
            audioSource_2.Play();
    }

    private void UpdateMixerVolume(float duration, float newVolume)
    {
        if (volumeChangeCoroutine != null) StopCoroutine(volumeChangeCoroutine);
        if (muteDelyCoroutine != null) StopCoroutine(muteDelyCoroutine);
        volumeChangeCoroutine = StartCoroutine(VolumeChange(
            audioMixer, Constants.AudioMixerNames.master, duration, newVolume));
    }

    private IEnumerator MuteDelay()
    {
        muteDelay.Reset();
        // to prevent audio blips lower the volume first and then pause the music
        yield return muteDelay;
        audioSource_1.Pause();
        audioSource_2.Pause();
    }

    private void StartFadeIn(string fadeInAudioMixerName, float duration, AudioSource audioSource, AudioClip newClip)
    {
        if (newClip.Equals(audioSource.clip))
        {
            StartFadeIn(fadeInAudioMixerName, duration, audioSource, FadeType.persist);
        }
        else
        {
            audioSource.clip = newClip;
            StartFadeIn(fadeInAudioMixerName, duration, audioSource);
        }
    }

    private void StartFadeIn(string fadeInAudioMixerName, float duration, AudioSource audioSource, FadeType fadeType = FadeType.play)
    {
        fadeInCoroutine = StartCoroutine(StartFade(audioMixer,
            fadeInAudioMixerName, duration, 1, audioSource, fadeType));
    }

    private void StartFadeOut(string fadeOutAudioMixerName, float duration, AudioSource audioSource)
    {
        fadeOutCoroutine = StartCoroutine(StartFade(audioMixer,
            fadeOutAudioMixerName, duration, 0, audioSource, FadeType.stop));
    }

    private void StopFadeCoroutines()
    {
        if (fadeOutCoroutine != null) StopCoroutine(fadeOutCoroutine);
        if (fadeInCoroutine != null) StopCoroutine(fadeInCoroutine);
    }
}
