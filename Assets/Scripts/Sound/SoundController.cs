using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public AudioSource soundController;

    public void ButtonPressSound()
    {
        AudioClip sound = Resources.Load<AudioClip>("Audio/button_press");
        soundController.clip = sound;
        soundController.Play();
    }

    public void CardPressSound()
    {
        AudioClip sound = Resources.Load<AudioClip>("Audio/token_select");
        soundController.clip = sound;
        soundController.Play();
    }

    public void CardToReactorSound()
    {
        AudioClip sound = Resources.Load<AudioClip>("Audio/token_placed_in_reactor");
        soundController.clip = sound;
        soundController.Play();
    }

    public void CardStackSound()
    {
        AudioClip sound = Resources.Load<AudioClip>("Audio/token_stack");
        soundController.clip = sound;
        soundController.Play();
    }

    public void CardRevealSound()
    {
        AudioClip sound = Resources.Load<AudioClip>("Audio/token_reveal");
        soundController.clip = sound;
        soundController.Play();
    }

    public void CardMatchSound()
    {
        AudioClip sound = Resources.Load<AudioClip>("Audio/card_match_c");
        soundController.clip = sound;
        soundController.Play();
    }

    public void DeckDeal()
    {
        AudioClip sound = Resources.Load<AudioClip>("Audio/deck_conveyer_belt_deal");
        soundController.clip = sound;
        soundController.Play();
    }

    public void DeckReshuffle()
    {
        AudioClip sound = Resources.Load<AudioClip>("Audio/deck_conveyer_belt_reshuffle");
        soundController.clip = sound;
        soundController.Play();
    }

    public void PauseMenuButtonSound()
    {
        AudioClip sound = Resources.Load<AudioClip>("Audio/pause_menu");
        soundController.clip = sound;
        soundController.Play();
    }

    public void ReactorExplodeSound()
    {
        AudioClip sound = Resources.Load<AudioClip>("Audio/reactor_explosion");
        soundController.clip = sound;
        soundController.Play();
    }
}
