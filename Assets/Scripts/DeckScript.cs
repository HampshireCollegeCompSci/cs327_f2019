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
    
    public Sprite placeHolderSprite;

    public GameObject myPrefab;
    public List<Sprite> sprites;
    public List<GameObject> cardList;

    public bool shuffleOnDeckReset = false;
    public bool dealOnDeckReset = true;
    public int foundationStartSize;

    int indexCounter;
    int positionCounter;

    void Start()
    {
        foundationStartSize = Config.config.foundationStartSize;
        print("foundation start Size in DeckScript: " + foundationStartSize);
        myPrefab = (GameObject)Resources.Load("Prefabs/Card", typeof(GameObject));
        myPrefab.GetComponent<BoxCollider2D>().size = new Vector2Int(1,1);
        myPrefab.GetComponent<BoxCollider2D>().offset = new Vector2Int(0, 0);

        utils = UtilsScript.global;

        // we really shouldn't set up the game adding data to cards and cards to foundations in this script
        InstantiateCards();
        Shuffle();
        SetUpFoundations();
        
        // let user know deck has cards and deal some out
        this.gameObject.GetComponent<SpriteRenderer>().sprite = placeHolderSprite;
        
        Deal();
        SetCardPositions();
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
                    cardList[cardIndex].GetComponent<CardScript>().cardSuit = "spades";
                }
                else if (suit == 2)
                {
                    cardList[cardIndex].GetComponent<CardScript>().cardSuit = "hearts";
                }
                else if (suit == 3)
                {
                    cardList[cardIndex].GetComponent<CardScript>().cardSuit = "diamonds";
                }

                cardList[cardIndex].GetComponent<CardScript>().cardFrontSprite = sprites[cardIndex + 1];
                cardList[cardIndex].GetComponent<CardScript>().hidden = true;
                cardList[cardIndex].GetComponent<CardScript>().appearSelected = false;
                cardList[cardIndex].GetComponent<CardScript>().container = this.gameObject;
               
                cardIndex += 1;
            }
        }
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

    public void RemoveCard(GameObject card)
    {
        cardList.Remove(card);
    }

    // top card is cardList[0]
    public void SetCardPositions()
    {
        indexCounter = cardList.Count - 1;
        positionCounter = 0;

        while (indexCounter > -1)
        {
            cardList[indexCounter].transform.position = gameObject.transform.position + new Vector3(0, -0.03f * positionCounter, -0.1f * positionCounter);

            indexCounter -= 1;
            positionCounter += 1;
        }
    }

    // user wants to deal cards, other things might need to be done before that
    public void ProcessAction(GameObject input)
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
                for (int r = 0; r < reactors.Count; r++)
                {
                    //  does this top card's suit matches the reactor suit
                    if (foundationCardList[0].GetComponent<CardScript>().cardSuit == reactors[r].GetComponent<ReactorScript>().suit)
                    {
                        foundationCardList[0].GetComponent<CardScript>().MoveCard(reactors[r]);
                        break;
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
        while (wasteCardList.Count > 0)
        {
            wasteCardList[0].GetComponent<CardScript>().MoveCard(this.gameObject);
            cardList[0].GetComponent<CardScript>().hidden = true;
            cardList[0].GetComponent<CardScript>().SetCardAppearance();
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

    // deals cards
    public void Deal()
    {
        for (int i = 0; i < Config.config.cardsToDeal; i++) // try to deal 3 cards
        {
            if (cardList.Count == 0) // are there no more cards in the deck?
            {
                break;
            }

            // reveal card and move from deck list top into waste
            cardList[0].GetComponent<CardScript>().hidden = false;
            cardList[0].GetComponent<CardScript>().SetCardAppearance();
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
