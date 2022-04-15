using System.Collections;
using UnityEngine;

public class SoundEffectsController : MonoBehaviour
{
    // Audio players component
    public AudioSource soundController;

    // Sound files
    public AudioClip buttonPressSound, undoPressSound, pauseButtonSound;
    public AudioClip tokenSelectSoundA, tokenSelectSoundB, tokenSelectSoundC;
    public AudioClip tokenInReactorSound, tokenStackSoundA, tokenStackSoundB, tokenStackSoundC, tokenStackSoundD;
    public AudioClip deckDealSound, deckReshuffleSound;
    public AudioClip winSound, loseSound, alertSound, winTransition;
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
        UpdateMaxVolume(PlayerPrefs.GetFloat(Constants.soundEffectsVolumeKey));
        bool.TryParse(PlayerPrefs.GetString(Constants.vibrationEnabledKey), out vibrationEnabled);
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
        int randomNo = Random.Range(0, 3);
        if (randomNo == 0)
        {
            soundController.PlayOneShot(tokenSelectSoundA, 0.3f);
        }
        else if (randomNo == 1)
        {
            soundController.PlayOneShot(tokenSelectSoundB, 0.3f);
        }
        else
        {
            soundController.PlayOneShot(tokenSelectSoundC, 0.3f);
        }
        VibrateSmall();
    }

    public void CardToReactorSound()
    {
        soundController.PlayOneShot(tokenInReactorSound, 0.5f);
        VibrateSmall();
    }

    public void CardStackSound()
    {
        int randomNo = Random.Range(0, 4);
        if (randomNo == 0)
        {
            soundController.PlayOneShot(tokenStackSoundA, 0.7f);
        }
        else if (randomNo == 1)
        {
            soundController.PlayOneShot(tokenStackSoundB, 0.7f);
        }
        else if (randomNo == 2)
        {
            soundController.PlayOneShot(tokenStackSoundC, 0.7f);
        }
        else
        {
            soundController.PlayOneShot(tokenStackSoundD, 0.7f);
        }
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

    IEnumerator AlertVibration()
    {
        yield return new WaitForSeconds(0.5f);
        soundController.PlayOneShot(alertSound, 0.2f);
        yield return new WaitForSeconds(0.05f);
        VibrateMedium();
        yield return new WaitForSeconds(0.25f);
        VibrateMedium();
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
        VibrateSmall();
    }

    public void FoodMatch(string suit)
    {
        if (suit == "hearts")
        {
            soundController.PlayOneShot(mushroomSound, 1);
        }
        if (suit == "diamonds")
        {
            soundController.PlayOneShot(bugSound, 0.6f);
        }
        if (suit == "spades")
        {
            soundController.PlayOneShot(rockSound, 1.8f);
        }
        if (suit == "clubs")
        {
            soundController.PlayOneShot(fruitSound, 1.2f);
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
