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
        int randomNo = Random.Range(0, 2);
        AudioClip sound = Resources.Load<AudioClip>("Audio/token_select");
        if (randomNo == 1)
            sound = Resources.Load<AudioClip>("Audio/token_select_B");
        else if (randomNo == 2)
            sound = Resources.Load<AudioClip>("Audio/token_select_C");

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
        int randomNo = Random.Range(0, 3);
        AudioClip sound = Resources.Load<AudioClip>("Audio/token_stack");
        if (randomNo == 1)
            sound = Resources.Load<AudioClip>("Audio/token_stack_B");
        else if (randomNo == 2)
            sound = Resources.Load<AudioClip>("Audio/token_stack_C");
        else if (randomNo == 3)
            sound = Resources.Load<AudioClip>("Audio/token_stack_D");

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
