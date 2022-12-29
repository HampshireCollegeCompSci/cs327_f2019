using System;
using System.Collections;
using UnityEngine;

public class SoundEffectsController : MonoBehaviour, ISound
{
    // Singleton instance.
    public static SoundEffectsController Instance;

    // Audio players component
    [SerializeField]
    private AudioSource soundController;

    // Sound files
    [SerializeField]
    private AudioClip buttonPressSound, undoPressSound, pauseButtonSound,
        tokenInReactorSound,
        deckDealSound, deckReshuffleSound,
        winSound, loseSound, alertSound, winTransition,
        explosionSound;

    [SerializeField]
    private AudioClip[] tokenSelectSounds, tokenStackSounds, foodMatchSounds;

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
        UpdateMaxVolume(PersistentSettings.SoundEffectsVolume);
    }

    public void UpdateMaxVolume(int newVolume)
    {
        Debug.Log($"updating sound effects volume to: {newVolume}");
        soundController.volume = ((float)newVolume) / GameValues.Settings.soundEffectsVolumeDenominator;
    }

    public void ButtonPressSound(bool vibrate = true)
    {
        soundController.PlayOneShot(buttonPressSound, 0.6f);

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
        soundController.PlayOneShot(tokenSelectSounds[UnityEngine.Random.Range(0, tokenSelectSounds.Length)], 0.3f);
        VibrateSmall();
    }

    public void CardToReactorSound()
    {
        soundController.PlayOneShot(tokenInReactorSound, 0.5f);
        VibrateSmall();
    }

    public void CardStackSound()
    {
        soundController.PlayOneShot(tokenStackSounds[UnityEngine.Random.Range(0, tokenStackSounds.Length)], 0.7f);
        VibrateSmall();
    }

    public void DeckDeal()
    {
        soundController.PlayOneShot(deckDealSound, 0.6f);
        VibrateMedium();
    }

    public void DeckReshuffle()
    {
        soundController.PlayOneShot(deckReshuffleSound, 0.6f);
        VibrateLarge();
    }

    public void PauseMenuButtonSound()
    {
        soundController.Stop();
        soundController.clip = pauseButtonSound;
        soundController.Play();
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

    public void FoodMatch(Suit suit)
    {
        if (suit.Index < 0 || suit.Index > foodMatchSounds.Length)
        {
            throw new IndexOutOfRangeException($"the suit {suit}'s index is not between 0-{foodMatchSounds.Length}");
        }
        soundController.PlayOneShot(foodMatchSounds[suit.Index], 1);
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
        yield return new WaitForSeconds(0.5f);
        soundController.PlayOneShot(alertSound, 0.2f);
        yield return new WaitForSeconds(0.1f);
        VibrateMedium();
        yield return new WaitForSeconds(0.3f);
        VibrateMedium();
    }
}
