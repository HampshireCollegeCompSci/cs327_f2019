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
        UpdateMaxVolume(PlayerPrefs.GetFloat(PlayerPrefKeys.soundEffectsVolumeKey, 0.5f));
    }

    public void UpdateMaxVolume(float newVolume)
    {
        Debug.Log($"updating sound effects volume to: {newVolume}");
        soundController.volume = newVolume;
    }

    public void ButtonPressSound()
    {
        // doesn't like PlayOneShot
        Vibration.Vibrate(Config.config.vibrationButton);
        soundController.clip = buttonPressSound;
        soundController.Play();
    }

    public void UndoPressSound()
    {
        Vibration.Vibrate(Config.config.vibrationButton);
        soundController.PlayOneShot(undoPressSound, 0.5f);
    }

    public void CardPressSound()
    {
        Vibration.Vibrate(Config.config.vibrationCard);
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
    }

    public void CardToReactorSound()
    {
        Vibration.Vibrate(Config.config.vibrationCard);
        soundController.PlayOneShot(tokenInReactorSound);
    }

    public void CardStackSound()
    {
        Vibration.Vibrate(Config.config.vibrationCard);
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
    }

    public void CardRevealSound()
    {
        soundController.PlayOneShot(tokenRevealSound);
    }

    public void DeckDeal()
    {
        Vibration.Vibrate(Config.config.vibrationCard);
        soundController.PlayOneShot(deckDealSound, 0.6f);
    }

    public void DeckReshuffle()
    {
        Vibration.Vibrate(Config.config.vibrationCard);
        soundController.PlayOneShot(deckReshuffleSound, 0.6f);
    }

    public void PauseMenuButtonSound()
    {
        Vibration.Vibrate(Config.config.vibrationButton);
        soundController.Stop();
        soundController.clip = pauseButtonSound;
        soundController.Play();
    }

    public void AlertSound()
    {
        soundController.PlayOneShot(alertSound, 0.3f);
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
        StartCoroutine(ExplosionVibration());
    }

    public void ExplosionSound()
    {
        soundController.clip = explosionSound;
        soundController.Play();
        Vibration.Vibrate(Config.config.vibrationExplosion);
    }

    IEnumerator ExplosionVibration()
    {
        yield return new WaitForSeconds(0.9f);
        Vibration.Vibrate(Config.config.vibrationExplosion);
    }

    public void FoodMatch(string suit)
    {
        Vibration.Vibrate(Config.config.vibrationMatch);
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
    }
}
