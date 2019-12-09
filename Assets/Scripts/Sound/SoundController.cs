using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public AudioSource soundController;

    public void ButtonPressSound()
    {
        // doesn't like PlayOneShot
        soundController.clip = Resources.Load<AudioClip>("Audio/button_press");
        soundController.Play();
    }
    public void ButtonPressSound2()
    {
        soundController.PlayOneShot(Resources.Load<AudioClip>("Audio/token_reveal"), 0.5f);
    }

    public void CardPressSound()
    {
        int randomNo = Random.Range(0, 3);
        Debug.Log(randomNo);
        if (randomNo == 0)
        {
            soundController.PlayOneShot(Resources.Load<AudioClip>("Audio/token_select"));
        }
        else if (randomNo == 1)
        {
            soundController.PlayOneShot(Resources.Load<AudioClip>("Audio/token_select_B"));
        }
        else
        {
            soundController.PlayOneShot(Resources.Load<AudioClip>("Audio/token_select_C"));
        }
    }

    public void CardToReactorSound()
    {
        soundController.PlayOneShot(Resources.Load<AudioClip>("Audio/token_placed_in_reactor"));
    }

    public void CardStackSound()
    {

        int randomNo = Random.Range(0, 4);
        if (randomNo == 0)
        {
            soundController.PlayOneShot(Resources.Load<AudioClip>("Audio/token_stack"));
        }
        else if (randomNo == 1)
        {
            soundController.PlayOneShot(Resources.Load<AudioClip>("Audio/token_stack_B"));
        }
        else if (randomNo == 2)
        {
            soundController.PlayOneShot(Resources.Load<AudioClip>("Audio/token_stack_C"));
        }
        else
        {
            soundController.PlayOneShot(Resources.Load<AudioClip>("Audio/token_stack_D"));
        }
    }

    public void CardRevealSound()
    {
        soundController.PlayOneShot(Resources.Load<AudioClip>("Audio/token_reveal"));
    }

    public void CardMatchSound()
    {
        soundController.PlayOneShot(Resources.Load<AudioClip>("Audio/card_match_c"));
    }

    public void DeckDeal()
    {
        soundController.PlayOneShot(Resources.Load<AudioClip>("Audio/deck_conveyer_belt_deal"), 0.3f);
    }

    public void DeckReshuffle()
    {
        soundController.PlayOneShot(Resources.Load<AudioClip>("Audio/deck_conveyer_belt_reshuffle"), 0.3f);
    }

    public void PauseMenuButtonSound()
    {
        soundController.Stop();
        soundController.clip = Resources.Load<AudioClip>("Audio/pause_menu");
        soundController.Play();
    }

    public void ReactorExplodeSound()
    {
        soundController.PlayOneShot(Resources.Load<AudioClip>("Audio/reactor_explosion"));
    }

    public void WinSound()
    {
        soundController.clip = Resources.Load<AudioClip>("Audio/sound_win"); ;
        soundController.Play();
    }
    public void FoodMatch(string suit)
    {
        if(suit == "hearts")
        {
            soundController.PlayOneShot(Resources.Load<AudioClip>("Audio/food_mushroom"));
        }
        if (suit == "diamonds")
        {
            soundController.PlayOneShot(Resources.Load<AudioClip>("Audio/food_bug"));
        }
        if (suit == "spades")
        {
            soundController.PlayOneShot(Resources.Load<AudioClip>("Audio/food_rock"));
        }
        if (suit == "clubs")
        {
            soundController.PlayOneShot(Resources.Load<AudioClip>("Audio/food_fruit"));
        }


    }
}
