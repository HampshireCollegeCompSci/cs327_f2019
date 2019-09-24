using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DeckScript : MonoBehaviour
{
    public UtilsScript utils;
    public GameObject wastePile;
    public List<GameObject> foundations;
    public List<GameObject> reactors;
    
    public Sprite cardBackSprite;
    public Sprite placeHolderSprite;

    public GameObject myPrefab;
    public List<Sprite> sprites;
    public List<GameObject> cardList;

    public bool shuffleOnDeckReset = false;
    public bool dealOnDeckReset = true;
    public int foundationStartSize = 7;

    void Start()
    {
        myPrefab = (GameObject)Resources.Load("Prefabs/Card", typeof(GameObject));
        utils = UtilsScript.global;

        // we really shouldn't set up the game adding data to cards and cards to foundations in this script
        InstantiateCards();
        Shuffle();
        SetUpFoundations();
        
        // let user know deck has cards and deal some out
        this.gameObject.GetComponent<SpriteRenderer>().sprite = cardBackSprite;
        Deal();
    }

     // sets up card list
    public void InstantiateCards()
    {
        cardList = new List<GameObject>();
        for (int i = 0; i < 52; i++)
        {
            cardList.Add(Instantiate(myPrefab));
        }

        // order: club ace, 2, 3... 10, jack, queen, king, diamonds... hearts... spades
        int cardIndex = 0; // 1 - 52
        for (int suit = 0; suit < 4; suit++) // order: club, diamonds, hearts, spades
        {
            for (int num = 1; num < 14; num++) // card num: 1 - 13
            { 
                if (num > 10) // all face cards have a value of 10
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

                cardList[cardIndex].GetComponent<CardScript>().cardBackSprite = sprites[0];
                cardList[cardIndex].GetComponent<CardScript>().cardFrontSprite = sprites[cardIndex];
                cardList[cardIndex].GetComponent<CardScript>().hidden = true;
                cardList[cardIndex].GetComponent<CardScript>().apearSelected = false;
                cardList[cardIndex].GetComponent<CardScript>().container = this.gameObject;

                cardIndex += 1;
            }
        }

        SetCardPositions();
    }

    // moves cards into foundations
    public void SetUpFoundations()
    {
        for (int i = 0; i < foundations.Count; i++)
        {
            for (int n = 0; n < foundationStartSize - 1; n++)
            {
                // set to hidden as they might be unhidden
                cardList[0].GetComponent<CardScript>().hidden = true;
                // MoveCard() should be removing the card from its current cardList so taking index 0 should work
                cardList[0].GetComponent<CardScript>().MoveCard(foundations[i]);
            }

            // adding and revealing the top card of the foundation
            cardList[0].GetComponent<CardScript>().hidden = false;
            cardList[0].GetComponent<CardScript>().MoveCard(foundations[i]);
        }
    }

    RemoveCard(GameObject card)
    {
        cardList.Remove(card);
    }

    // cards shouldn't be clickable nor viewable, only the deck's sprite should be
    public void SetCardPositions()
    {
        // stagger the cards
        float offset = 0;
        for (int i = cardList.Count - 1; i > -1; i--) // index 0 is on the top
        {
            cardList[i].transform.parent = this.gameObject.transform;
            cardList[i].transform.localPosition = new Vector3(offset, offset, 0.01f + offset);
            offset += 0.01f;
        }
    }

    // user wants to deal cards, other things might need to be done before that
    public void Clicked(GameObject input)
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
            this.gameObject.GetComponent<SpriteRenderer>().sprite = cardBackSprite;
            DeckReset();
        }
    }

    // moves all of the top foundation cards into their appropriate reactors
    public void NextCycle()
    {
        for (int f = 0; f < foundations.Count; f++)
        {
            // get the list of cards in a foundation
            List<GameObject> foundationCardList = foundations[f].GetComponent<FoundationScript>().cardList;
            if (foundationCardList.Count != 0) // is it not empty?
            {
                GameObject topFoundationCard = foundationCardList[0];

                // trackers for first time reactor suits
                GameObject emptyReactor = null;
                bool placed = false;
                for (int r = 0; r < reactors.Count; r++)
                {
                    // get the reactor's card list
                    List<GameObject> reactorCardList = reactors[r].GetComponent<ReactorScript>().cardList;
                    if (reactorCardList.Count == 0) // is this reactor empty?
                    {
                        if (emptyReactor == null) // is this the first empty reactor found for this card?
                        {
                            // save for possible use
                            emptyReactor = reactors[r];
                        }
                    }
                    // otherwise see if this top card's suit matches the reactor cards suit
                    else if (topFoundationCard.GetComponent<CardScript>().cardSuit == reactorCardList[0].GetComponent<CardScript>().cardSuit)
                    {
                        topFoundationCard.GetComponent<CardScript>().MoveCard(reactors[r]);
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

    // moves all wastePile cards into the deck
    public void DeckReset()
    {
        // the top of the deck is index 0
        // get all the cards from the wastepile
        List<GameObject> wasteCardList = wastePile.GetComponent<WastepileScript>().GetCardList();

        // move all the cards from waste to deck, preserves reveal order
        // if there are any cards in the deck's cardList before they will be on the bottom after
        for (int i = 0; i < wasteCardList.Count; i++)
        {
            wasteCardList[i].GetComponent<CardScript>().MoveCard(this.gameObject);
        }

        if (shuffleOnDeckReset)
        {
            Shuffle();
        }

        if (dealOnDeckReset) // auto deal cards
        {
            Deal();
        }
    }

    // deals cards, changes sprite if deck is empty
    public void Deal()
    {
        for (int i = 0; i < 3; i++) // try to deal 3 cards
        {
            if (cardList.Count == 0) // are there no more cards in the deck?
            {
                this.gameObject.GetComponent<SpriteRenderer>().sprite = placeHolderSprite;
                break;
            }

            // move card from deck list top into waste
            cardList[0].GetComponent<CardScript>().MoveCard(wastePile);
        }
    }

    //Shuffles cardList using Knuth shuffle aka Fisher-Yates shuffle
    public void Shuffle()
    {
        System.Random rand = new System.Random();

        for (int i = 0; i < cardList.Count; i++)
        {
            int j = rand.Next(i, cardList.Count);
            GameObject temp = cardList[i];
            cardList[i] = cardList[j];
            cardList[j] = temp;
        }
    }
}
