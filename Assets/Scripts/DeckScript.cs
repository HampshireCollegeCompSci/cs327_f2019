using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckScript : MonoBehaviour
{
    public GameObject utils;
    private GameObject wastePile;

    public List<GameObject> cardList;
    public bool shuffleOnDeckReset = true;
    public bool dealOnDeckReset = true;

    void Start()
    {
        // we really shouldn't set up the game adding data to cards and cards to foundations in this script
        cardList = Shuffle(cardList);
    }

    public void setCardPositions()
    {
        // cards should have hitboxes so placing them as hidden on top of the deck will need to be accounted for somewhere else
        // or, don't have them on screen and have a placeholder card back that disappears when the deck is empty
        return;
    }

    public void Clicked()
    {
        if (cardList.Count != 0) // can the deck can be drawn from
        {
            Deal();
        }
        else //we need to repopulate the deck
        {
            if (wastePile.GetComponent<WastepileScript>().GetCardList().Count == 0) // is it not possible to repopulate the deck?
            {
                return;
            }

            /*if (utils.NextCycle()) // trigger next cycle and see if the game ends
            {
                return;
            }*/

            DeckReset();
        }
    }

    public void DeckReset()
    {
        // get all the cards from the wastepile
        List<GameObject> wasteCardList = wastePile.GetComponent<WastepileScript>().GetCardList();

        // move all the cards from waste to deck, preserves reveal order
        for (int i = wasteCardList.Count - 1; i > -1; i--)
        {
            wasteCardList[i].GetComponent<CardScript>().MoveCard(this.gameObject, null);
        }

        if (shuffleOnDeckReset)
        {
            cardList = Shuffle(cardList);
        }

        if (dealOnDeckReset) // auto deal cards
        {
            Deal();
        }
    }

    public void Deal()
    {
        for (int i = 0; i < 3; i++) // try to deal 3 cards
        {
            if (cardList.Count == 0) // are there no more cards in the deck?
            {
                break;
            }

            // move card from deck list top into waste
            cardList[cardList.Count - 1].GetComponent<CardScript>().MoveCard(wastePile, null);
        }
    }

    //Shuffles list using Knuth shuffle aka Fisher-Yates shuffle
    public List<GameObject> Shuffle(List<GameObject> cards)
    {
        System.Random rand = new System.Random();

        for (int i = 0; i < cards.Count; i++)
        {
            int j = rand.Next(i, cards.Count);
            GameObject temp = cards[i];
            cards[i] = cards[j];
            cards[j] = temp;
        }

        return cards;
    }
}
