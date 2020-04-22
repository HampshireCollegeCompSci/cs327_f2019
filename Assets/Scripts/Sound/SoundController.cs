﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public AudioSource soundController;

    public AudioClip buttonPressSound, undoPressSound, pauseButtonSound;
    public AudioClip tokenRevealSound, tokenSelectSoundA, tokenSelectSoundB, tokenSelectSoundC;
    public AudioClip tokenInReactorSound, tokenStackSoundA, tokenStackSoundB, tokenStackSoundC, tokenStackSoundD;
    public AudioClip deckDealSound, deckReshuffleSound;
    public AudioClip winSound, loseSound, alertSound;
    public AudioClip mushroomSound, bugSound, fruitSound, rockSound;
    public AudioClip explosionSound;

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
        Debug.Log("alert");
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
        Debug.Log("EX");
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
