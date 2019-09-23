using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckScript : MonoBehaviour
{
    public GameObject utils;
    private GameObject wastePile;
    private List<GameObject> foundations;
    private List<GameObject> reactors;

    public Sprite cardBackSprite;
    public Sprite placeHolderSprite;
    public List<GameObject> cardList;
    public bool shuffleOnDeckReset = true;
    public bool dealOnDeckReset = true;
    public int foundationStartSize = 7;

    void Start()
    {
        // we really shouldn't set up the game adding data to cards and cards to foundations in this script

        // setting up card list
        // order: club ace, 2, 3... 10, jack, queen, king, diamonds... hearts... spades
        int cardIndex;
        for (int suit = 0; suit < 4; suit++)
        {
            for (int num = 1; num < 14; num++)
            {
                cardIndex = suit * 13 + num;

                if (num > 10)
                {
                    cardList[cardIndex].GetComponent<CardScript>().cardVal = 10;
                }
                else
                {
                    cardList[cardIndex].GetComponent<CardScript>().cardVal = num;
                }

                cardList[cardIndex].GetComponent<CardScript>().cardNum = num;

                if (suit == 0)
                {
                    cardList[cardIndex].GetComponent<CardScript>().cardSuit = "clubs";
                }
                else if (suit == 1)
                {
                    cardList[cardIndex].GetComponent<CardScript>().cardSuit = "diamonds";
                }
                else if (suit == 2)
                {
                    cardList[cardIndex].GetComponent<CardScript>().cardSuit = "hearts";
                }
                else if (suit == 3)
                {
                    cardList[cardIndex].GetComponent<CardScript>().cardSuit = "spades";
                }

                // the only cards that will ever be hidden are the foundation non top cards
                // the decks cards are up for debate as their location is not specified yet
                cardList[cardIndex].GetComponent<CardScript>().hidden = false;
                cardList[cardIndex].GetComponent<CardScript>().apearSelected = false;
                //cardList[cardIndex].GetComponent<CardScript>().currentLocation = this.gameObject;
            }
        }

        cardList = Shuffle(cardList);

        // moving cards into foundations
        foreach (GameObject foundation in foundations)
        {
            for (int n = 0; n < foundationStartSize; n++)
            {
                // set to hidden as they might be unhidden
                cardList[0].GetComponent<CardScript>().hidden = true;
                // MoveCard() should be removing the card from its current cardList so taking index 0 should work
                cardList[0].GetComponent<CardScript>().MoveCard(foundation);
            }

            // revealing the top card of the foundation
            foundation.GetComponent<FoundationScript>().cardList[foundationStartSize - 1].GetComponent<CardScript>().hidden = false;
        }

        gameObject.GetComponent<SpriteRenderer>().sprite = cardBackSprite;
        Deal();
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
        else // we can repopulate the deck
        {
            if (wastePile.GetComponent<WastepileScript>().GetCardList().Count == 0) // is it not possible to repopulate the deck?
            {
                return;
            }

            NextCycle();
            gameObject.GetComponent<SpriteRenderer>().sprite = cardBackSprite;
            DeckReset();
        }
    }

    public void NextCycle()
    {
        foreach (GameObject foundation in foundations)
        {
            // get the list of cards in a foundation
            List<GameObject> foundationCardList = foundation.GetComponent<FoundationScript>().cardList;
            if (foundationCardList.Count != 0) // is it not empty?
            {
                GameObject topFoundationCard = foundationCardList[foundationCardList.Count - 1];

                // trackers for first time reactor suits
                GameObject emptyReactor = null;
                bool placed = false;
                foreach (GameObject reactor in reactors)
                {
                    // get the reactor's card list
                    List<GameObject> reactorCardList = reactor.GetComponent<ReactorScript>().cardList;
                    if (reactorCardList.Count == 0) // is this reactor empty?
                    {
                        if (emptyReactor == null) // is this the first empty reactor found for this card?
                        {
                            // save for possible use
                            emptyReactor = reactor;
                        }
                    }
                    // otherwise see if this top card's suit matches the reactor cards suit
                    else if (topFoundationCard.GetComponent<CardScript>().cardSuit == reactorCardList[0].GetComponent<CardScript>().cardSuit)
                    {
                        topFoundationCard.GetComponent<CardScript>().MoveCard(reactor);
                        placed = true;
                        break;
                    }
                }
                if (placed == false) // is this a brand new suit for the reactors?
                {
                    if (emptyReactor != null) // reactors with the same suit may not fulfill this
                    {
                        // place this top card into the first empty reactor that was found
                        topFoundationCard.GetComponent<CardScript>().MoveCard(emptyReactor);
                    }
                }
            }
        }
    }

    public void DeckReset()
    {
        // get all the cards from the wastepile
        List<GameObject> wasteCardList = wastePile.GetComponent<WastepileScript>().GetCardList();

        // move all the cards from waste to deck, preserves reveal order
        for (int i = wasteCardList.Count - 1; i > -1; i--)
        {
            wasteCardList[i].GetComponent<CardScript>().MoveCard(gameObject);
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
                gameObject.GetComponent<SpriteRenderer>().sprite = placeHolderSprite;
                break;
            }

            // move card from deck list top into waste
            cardList[cardList.Count - 1].GetComponent<CardScript>().MoveCard(wastePile);
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
