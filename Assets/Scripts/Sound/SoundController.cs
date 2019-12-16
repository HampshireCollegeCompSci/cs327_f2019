using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public AudioSource soundController;

    public AudioClip buttonPressSound, undoPressSound, pauseButtonSound,
                    tokenRevealSound, tokenSelectSoundA, tokenSelectSoundB, tokenSelectSoundC,
                    tokenInReactorSound, tokenStackSoundA, tokenStackSoundB, tokenStackSoundC, tokenStackSoundD,
                    cardMatchSound, deckDealSound, deckReshuffleSound, winSound, loseSound,
                    mushroomSound, bugSound, fruitSound, rockSound;


    public void ButtonPressSound()
    {
        // doesn't like PlayOneShot
        soundController.clip = buttonPressSound;
        soundController.Play();
    }

    public void UndoPressSound()
    {
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
        soundController.PlayOneShot(cardMatchSound, 0.6f);
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
        soundController.Stop();
        soundController.clip = pauseButtonSound;
        soundController.Play();
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
