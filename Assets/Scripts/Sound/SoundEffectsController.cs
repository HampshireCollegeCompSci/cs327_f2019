using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class SoundEffectsController : MonoBehaviour, ISound
{
    // Singleton instance.
    public static SoundEffectsController Instance { get; private set; }
    private static readonly WaitForSecondsRealtime alertDelay0 = new(0.5f),
        alertDelay1 = new(0.1f),
        alertDelay2 = new(0.3f);

    [SerializeField]
    private AudioMixer soundEffectMixer;

    // Audio players component
    [SerializeField]
    private AudioSource soundController;
    [SerializeField]
    private AudioSource[] buttonControllers;

    // Sound files
    [SerializeField]
    private AudioClip undoPressSound, pauseButtonSound,
        deckDealSound, deckReshuffleSound,
        winSound, loseSound, alertSound, winTransition,
        explosionSound, achievementSound;

    [SerializeField]
    private float minTimeBetweenButtonSounds = 0.1f; // 100ms gap
    private float lastPlayTime;
    private int buttonToggle;
    private Coroutine volumeChangeCoroutine;

    // Initialize the singleton instance.
    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    private void Start()
    {
        soundEffectMixer.SetFloat(Constants.AudioMixerNames.master, -80);
        UpdateMaxVolume(PersistentSettings.SoundEffectsVolume);
    }

    public bool Muted { get; private set; }

    public void UpdateMaxVolume(int newVolume)
    {
        if (newVolume == 0)
        {
            //UpdateMixerVolume(0);
            soundEffectMixer.SetFloat(Constants.AudioMixerNames.master, -80);
            Muted = true;
            return;
        }
        if (Muted) Muted = false;

        // 0 to 1
        float maxVolume = (float)newVolume / GameValues.Settings.soundEffectsVolumeDenominator;
        soundEffectMixer.SetFloat(Constants.AudioMixerNames.master, Mathf.Log10(maxVolume) * 20);
        //UpdateMixerVolume(maxVolume);
    }

    private void UpdateMixerVolume(float newVolume)
    {
        if (volumeChangeCoroutine != null) StopCoroutine(volumeChangeCoroutine);
        volumeChangeCoroutine = StartCoroutine(FadeMixerGroup.VolumeChange(
            soundEffectMixer, Constants.AudioMixerNames.master, GameValues.Audio.volumeDelay, newVolume));
    }

    public void UserVolumeUpdate(int volumeUpdate)
    {
        if (volumeUpdate == 0)
        {
            UpdateMaxVolume(0);
            return;
        }
        StartCoroutine(ButtonDelay());
        UpdateMaxVolume(volumeUpdate);
    }

    public void ButtonPressSound(bool volumeChange = false)
    {
        if (!volumeChange) VibrationController.Instance.VibrateMedium();
        if (!volumeChange && Muted) return;

        // if we tried to play another sound too soon, ignore it
        if (Time.unscaledTime - lastPlayTime < minTimeBetweenButtonSounds) return;
        lastPlayTime = Time.unscaledTime;
        //buttonControllers[buttonToggle].pitch = Random.Range(0.98f, 1.02f);
        buttonControllers[buttonToggle ^= 1].Play();
    }

    public void UndoPressSound()
    {
        VibrationController.Instance.VibrateMedium();
        if (Muted) return;
        soundController.PlayOneShot(undoPressSound, 0.5f);
    }

    public void DeckDeal()
    {
        VibrationController.Instance.VibrateMedium();
        if (Muted) return;
        soundController.PlayOneShot(deckDealSound, 0.3f);
    }

    public void DeckReshuffle()
    {
        VibrationController.Instance.VibrateLarge();
        if (Muted) return;
        soundController.PlayOneShot(deckReshuffleSound, 0.3f);
    }

    public void PauseMenuButtonSound()
    {
        VibrationController.Instance.VibrateMedium();
        soundController.Stop();
        if (Muted) return;
        soundController.PlayOneShot(pauseButtonSound, 0.4f);
    }

    public void AlertSound()
    {
        if (Muted) return;
        StartCoroutine(AlertVibration());
    }

    public void WinSound()
    {
        if (Muted) return;
        soundController.clip = winSound;
        soundController.Play();
    }

    public void LoseSound()
    {
        if (Muted) return;
        soundController.clip = loseSound;
        soundController.Play();
    }

    public void WinTransition()
    {
        if (Muted) return;
        soundController.clip = winTransition;
        soundController.Play();
    }

    public void ExplosionSound()
    {
        VibrationController.Instance.VibrateMedium();
        if (Muted) return;
        soundController.PlayOneShot(explosionSound, 0.6f);
    }

    public void AchievementSound()
    {
        if (Muted) return;
        soundController.PlayOneShot(achievementSound, 0.7f);
    }

    private IEnumerator AlertVibration()
    {
        yield return alertDelay0;
        soundController.PlayOneShot(alertSound, 0.5f);
        yield return alertDelay1;
        VibrationController.Instance.VibrateMedium();
        yield return alertDelay2;
        VibrationController.Instance.VibrateMedium();
    }

    private IEnumerator ButtonDelay()
    {
        yield return null;
        ButtonPressSound(true);
    }
}
