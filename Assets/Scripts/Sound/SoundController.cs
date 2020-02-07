using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public AudioSource soundController;

    public AudioClip buttonPressSound, undoPressSound, pauseButtonSound;
    public AudioClip tokenRevealSound, tokenSelectSoundA, tokenSelectSoundB, tokenSelectSoundC;
    public AudioClip tokenInReactorSound, tokenStackSoundA, tokenStackSoundB, tokenStackSoundC, tokenStackSoundD;
    public AudioClip tokenMatchSoundA, tokenMatchSoundB, tokenMatchSoundC;
    public AudioClip deckDealSound, deckReshuffleSound;
    public AudioClip winSound, loseSound, alertSound;
    public AudioClip mushroomSound, bugSound, fruitSound, rockSound;

    private long[] pattern = new long[] { 5, 5 };

    public void ButtonPressSound()
    {
        // doesn't like PlayOneShot
        Vibration.Vibrate(10);
        soundController.clip = buttonPressSound;
        soundController.Play();
    }

    public void UndoPressSound()
    {
        Vibration.Vibrate(10);
        soundController.PlayOneShot(undoPressSound, 0.5f);
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
    }

    public void CardToReactorSound()
    {
        soundController.PlayOneShot(tokenInReactorSound);
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
    }

    public void CardRevealSound()
    {
        soundController.PlayOneShot(tokenRevealSound);
    }

    public void CardMatchSound()
    {
        int randomNo = Random.Range(0, 2);
        if (randomNo == 0)
        {
            soundController.PlayOneShot(tokenMatchSoundA, 0.6f);
        }
        else if (randomNo == 1)
        {
            soundController.PlayOneShot(tokenMatchSoundB, 0.6f);
        }
        else
        {
            soundController.PlayOneShot(tokenMatchSoundC, 0.6f);
        }
    }

    public void DeckDeal()
    {
        soundController.PlayOneShot(deckDealSound, 0.6f);
    }

    public void DeckReshuffle()
    {
        soundController.PlayOneShot(deckReshuffleSound, 0.6f);
    }

    public void PauseMenuButtonSound()
    {
        Vibration.Vibrate(10);
        soundController.Stop();
        soundController.clip = pauseButtonSound;
        soundController.Play();
    }

    public void AlertSound()
    {
        Vibration.Vibrate(pattern, 1);
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

    IEnumerator ExplosionVibration()
    {
        yield return new WaitForSeconds(1);
        Vibration.Vibrate();
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
    }
}
