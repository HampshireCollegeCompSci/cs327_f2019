using System;
using System.Collections;
using UnityEngine;

public class SoundEffectsController : MonoBehaviour, ISound
{
    // Singleton instance.
    public static SoundEffectsController Instance { get; private set; }
    private static readonly WaitForSecondsRealtime alertDelay0 = new(0.5f),
        alertDelay1 = new(0.1f),
        alertDelay2 = new(0.3f);

    // Audio players component
    [SerializeField]
    private AudioSource soundController;

    // Sound files
    [SerializeField]
    private AudioClip buttonPressSound, undoPressSound, pauseButtonSound,
        tokenInReactorSound,
        deckDealSound, deckReshuffleSound,
        winSound, loseSound, alertSound, winTransition,
        explosionSound, achievementSound;

    [SerializeField]
    private AudioClip[] tokenSelectSounds, tokenStackSounds, foodMatchSounds;

    // Initialize the singleton instance.
    private void Awake()
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
    }

    private void Start()
    {
        UpdateMaxVolume(PersistentSettings.SoundEffectsVolume);
    }

    public void UpdateMaxVolume(int newVolume)
    {
        Debug.Log($"updating sound effects volume to: {newVolume}");
        soundController.volume = ((float)newVolume) / GameValues.Settings.soundEffectsVolumeDenominator;
    }

    public void ButtonPressSound(bool vibrate = true)
    {
        soundController.PlayOneShot(buttonPressSound, 0.8f);

        if (vibrate)
        {
            VibrateMedium();
        }
    }

    public void UndoPressSound()
    {
        soundController.PlayOneShot(undoPressSound, 0.5f);
        VibrateMedium();
    }

    public void CardPressSound()
    {
        soundController.PlayOneShot(tokenSelectSounds[UnityEngine.Random.Range(0, tokenSelectSounds.Length)], 0.6f);
        VibrateSmall();
    }

    public void CardToReactorSound()
    {
        soundController.PlayOneShot(tokenInReactorSound, 0.3f);
        VibrateSmall();
    }

    public void CardStackSound()
    {
        soundController.PlayOneShot(tokenStackSounds[UnityEngine.Random.Range(0, tokenStackSounds.Length)], 0.7f);
        VibrateSmall();
    }

    public void DeckDeal()
    {
        soundController.PlayOneShot(deckDealSound, 0.3f);
        VibrateMedium();
    }

    public void DeckReshuffle()
    {
        soundController.PlayOneShot(deckReshuffleSound, 0.3f);
        VibrateLarge();
    }

    public void PauseMenuButtonSound()
    {
        soundController.Stop();
        soundController.PlayOneShot(pauseButtonSound, 0.4f);
        VibrateMedium();
    }

    public void AlertSound()
    {
        StartCoroutine(AlertVibration());
    }

    public void WinSound()
    {
        soundController.clip = winSound;
        soundController.Play();
    }

    public void LoseSound()
    {
        soundController.clip = loseSound;
        soundController.Play();
    }

    public void WinTransition()
    {
        soundController.clip = winTransition;
        soundController.Play();
    }

    public void ExplosionSound()
    {
        soundController.PlayOneShot(explosionSound, 0.6f);
        VibrateMedium();
    }

    public void AchievementSound()
    {
        soundController.PlayOneShot(achievementSound, 1);
    }

    public void FoodMatch(Suit suit)
    {
        if (suit.Index < 0 || suit.Index > foodMatchSounds.Length)
        {
            throw new IndexOutOfRangeException($"the suit {suit}'s index is not between 0-{foodMatchSounds.Length}");
        }
        soundController.PlayOneShot(foodMatchSounds[suit.Index], 0.5f);
        VibrateMedium();
    }

    public void VibrateSmall()
    {
        if (PersistentSettings.VibrationEnabled) Vibration.VibratePop();
    }

    public void VibrateMedium()
    {
        if (PersistentSettings.VibrationEnabled) Vibration.VibratePeek();
    }

    public void VibrateLarge()
    {
        if (PersistentSettings.VibrationEnabled) Vibration.Vibrate();
    }

    private IEnumerator AlertVibration()
    {
        yield return alertDelay0;
        soundController.PlayOneShot(alertSound, 0.5f);
        yield return alertDelay1;
        VibrateMedium();
        yield return alertDelay2;
        VibrateMedium();
    }
}
