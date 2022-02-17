using System.Collections;
using UnityEngine;

public class SoundEffectsController : MonoBehaviour
{
    // Audio players component
    public AudioSource soundController;

    // Sound files
    public AudioClip buttonPressSound, undoPressSound, pauseButtonSound;
    public AudioClip tokenRevealSound, tokenSelectSoundA, tokenSelectSoundB, tokenSelectSoundC;
    public AudioClip tokenInReactorSound, tokenStackSoundA, tokenStackSoundB, tokenStackSoundC, tokenStackSoundD;
    public AudioClip deckDealSound, deckReshuffleSound;
    public AudioClip winSound, loseSound, alertSound;
    public AudioClip mushroomSound, bugSound, fruitSound, rockSound;
    public AudioClip explosionSound;

    private bool vibrationEnabled;

    // Singleton instance.
    public static SoundEffectsController Instance = null;

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
        UpdateMaxVolume(PlayerPrefs.GetFloat(Constants.soundEffectsVolumeKey));

        Vibration.Init();
        System.Boolean.TryParse(PlayerPrefs.GetString(Constants.vibrationEnabledKey), out vibrationEnabled);
    }

    public void UpdateMaxVolume(float newVolume)
    {
        Debug.Log($"updating sound effects volume to: {newVolume}");
        soundController.volume = newVolume;
    }

    public void UpdateVibration(bool vibrationEnabled)
    {
        this.vibrationEnabled = vibrationEnabled;
    }

    public void ButtonPressSound()
    {
        soundController.clip = buttonPressSound;
        // doesn't like PlayOneShot
        soundController.Play();
        VibrateMedium();
    }

    public void UndoPressSound()
    {
        soundController.PlayOneShot(undoPressSound, 0.5f);
        VibrateMedium();
    }

    public void CardPressSound()
    {
        int randomNo = Random.Range(0, 3);
        if (randomNo == 0)
        {
            soundController.PlayOneShot(tokenSelectSoundA);
        }
        else if (randomNo == 1)
        {
            soundController.PlayOneShot(tokenSelectSoundB);
        }
        else
        {
            soundController.PlayOneShot(tokenSelectSoundC);
        }
        VibrateSmall();
    }

    public void CardToReactorSound()
    {
        soundController.PlayOneShot(tokenInReactorSound);
        VibrateSmall();
    }

    public void CardStackSound()
    {
        int randomNo = Random.Range(0, 4);
        if (randomNo == 0)
        {
            soundController.PlayOneShot(tokenStackSoundA);
        }
        else if (randomNo == 1)
        {
            soundController.PlayOneShot(tokenStackSoundB);
        }
        else if (randomNo == 2)
        {
            soundController.PlayOneShot(tokenStackSoundC);
        }
        else
        {
            soundController.PlayOneShot(tokenStackSoundD);
        }
        VibrateSmall();
    }

    public void CardRevealSound()
    {
        soundController.PlayOneShot(tokenRevealSound);
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
        soundController.PlayOneShot(alertSound, 0.2f);
        StartCoroutine(AlertVibration());
    }

    IEnumerator AlertVibration()
    {
        VibrateSmall();
        yield return new WaitForSeconds(0.1f);
        VibrateSmall();
    }

    public void WinSound()
    {
        soundController.clip = winSound;
        soundController.Play();
        VibrateLarge();
    }

    public void LoseSound()
    {
        soundController.clip = loseSound;
        soundController.Play();
        StartCoroutine(LoseVibration());
    }

    private IEnumerator LoseVibration()
    {
        yield return new WaitForSeconds(0.9f);
        VibrateLarge();
    }

    public void ExplosionSound()
    {
        soundController.clip = explosionSound;
        soundController.Play();
        VibrateLarge();
    }

    public void FoodMatch(string suit)
    {
        if (suit == "hearts")
        {
            soundController.PlayOneShot(mushroomSound);
        }
        if (suit == "diamonds")
        {
            soundController.PlayOneShot(bugSound);
        }
        if (suit == "spades")
        {
            soundController.PlayOneShot(rockSound);
        }
        if (suit == "clubs")
        {
            soundController.PlayOneShot(fruitSound);
        }
        VibrateMedium();
    }

    public void VibrateSmall()
    {
        if (vibrationEnabled) Vibration.VibratePop();
    }

    public void VibrateMedium()
    {
        if (vibrationEnabled) Vibration.VibratePeek();
    }

    public void VibrateLarge()
    {
        if (vibrationEnabled) Vibration.Vibrate();
    }
}
