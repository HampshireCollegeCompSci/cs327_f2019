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
        tokenSelectSoundA, tokenSelectSoundB, tokenSelectSoundC,
        tokenInReactorSound, tokenStackSoundA, tokenStackSoundB, tokenStackSoundC, tokenStackSoundD,
        deckDealSound, deckReshuffleSound,
        winSound, loseSound, alertSound, winTransition,
        mushroomSound, bugSound, fruitSound, rockSound,
        explosionSound;

    private bool vibrationEnabled;

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
        UpdateMaxVolume(PlayerPrefKeys.GetSoundEffectsVolume());
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

    public void FoodMatch(byte suit)
    {
        switch (suit)
        {
            case Constants.heartsSuitIndex:
                soundController.PlayOneShot(mushroomSound, 1);
                break;
            case Constants.diamondsSuitIndex:
                soundController.PlayOneShot(bugSound, 1);
                break;
            case Constants.spadesSuitIndex:
                soundController.PlayOneShot(rockSound, 1);
                break;
            case Constants.clubsSuitIndex:
                soundController.PlayOneShot(fruitSound, 1);
                break;
            default:
                throw new System.Exception($"{suit} isn't a suit!");
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
