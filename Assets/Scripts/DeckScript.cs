﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class DeckScript : MonoBehaviour
{
    public static DeckScript deckScript;

    public UtilsScript utils;
    public GameObject wastePile;
    private WastepileScript wastePileScript;
    public List<GameObject> foundations;
    public List<GameObject> reactors;

    public GameObject myPrefab;
    public List<Sprite> sprites;
    public List<GameObject> cardList;

    public Text deckCounter;

    public SoundController soundController;

    // public bool shuffleOnDeckReset = false;
    public bool dealOnDeckReset = true;

    public bool importSeed;
    public int shuffleSeed;


    private void Awake()
    {
        if (deckScript == null)
        {
            DontDestroyOnLoad(gameObject); //makes instance persist across scenes
            deckScript = this;
        }
        else if (deckScript != this)
        {
            Destroy(gameObject); //deletes copies of global which do not need to exist, so right version is used to get info from
        }
    }
    void Start()
    {
        utils = UtilsScript.global;
        wastePileScript = wastePile.GetComponent<WastepileScript>();

        cardList = new List<GameObject>();
        InstantiateCards(this.gameObject);
        importSeed = false;
        Shuffle();
        SetUpFoundations();
        Deal(false);

    }

    // sets up card list
    public void InstantiateCards(GameObject target)
    {
        GameObject newCard;
        CardScript newCardScript;
        
        // order: club ace, 2, 3... 10, jack, queen, king, diamonds... hearts... spades
        int cardIndex = 0; // 1 - 52
        for (int suit = 0; suit < 4; suit++) // order: club, diamonds, hearts, spades
        {
            for (int num = 1; num < 14; num++) // card num: 1 - 13
            {
                newCard = Instantiate(myPrefab);
                newCardScript = newCard.GetComponent<CardScript>();
                if (num > 10) // all face cards have a value of 10
                {
                    newCardScript.cardVal = 10;
                }
                else
                {
                    newCardScript.cardVal = num;
                }

                newCardScript.cardNum = num;

                if (suit == 0)
                {
                    newCardScript.cardSuit = "clubs";
                }
                else if (suit == 1)
                {
                    newCardScript.cardSuit = "spades";
                }
                else if (suit == 2)
                {
                    newCardScript.cardSuit = "hearts";
                }
                else if (suit == 3)
                {
                    newCardScript.cardSuit = "diamonds";
                }

                newCardScript.cardFrontSprite = sprites[cardIndex];
                newCardScript.SetVisibility(true);
                newCardScript.container = target;
                AddCard(newCard);
                
                cardIndex += 1;
            }
        }
    }

    // moves cards into foundations
    public void SetUpFoundations()
    {
        for (int i = 0; i < foundations.Count; i++)
        {
            for (int n = 0; n < Config.config.foundationStartSize - 1; n++)
            {
                cardList[0].GetComponent<CardScript>().SetVisibility(false);
                cardList[0].GetComponent<CardScript>().MoveCard(foundations[i], doLog: false, addUpdateHolo: false);
            }

            // adding and revealing the top card of the foundation
            cardList[0].GetComponent<CardScript>().SetVisibility(true);
            cardList[0].gameObject.GetComponent<CardScript>().ShowHologram();
            cardList[0].GetComponent<CardScript>().MoveCard(foundations[i], doLog: false, addUpdateHolo: false);
        }
    }

    public void AddCard(GameObject card, bool checkHolo = true)
    {
        card.GetComponent<CardScript>().HideHologram();
        cardList.Insert(0, card);
        card.transform.SetParent(gameObject.transform);
        card.transform.localPosition = Vector3.zero;
        card.SetActive(false);
        deckCounter.text = cardList.Count.ToString();
    }

    public void RemoveCard(GameObject card, bool checkHolo = false)
    {
        cardList.Remove(card);
        card.SetActive(true);
        deckCounter.text = cardList.Count.ToString();
    }

    // user wants to deal cards, other things might need to be done before that
    public void ProcessAction(GameObject input)
    {
        utils.DeselectCards();

        if (cardList.Count != 0) // can the deck can be drawn from
        {
            soundController.DeckDeal();
            Deal();
        }
        else // we need to try repopulating the deck
        {
            if (wastePileScript.GetCardList().Count == 0) // is it not possible to repopulate the deck?
            {
                return;
            }

            DeckReset();
        }
    }

    // moves all of the top foundation cards into their appropriate reactors
    public void NextCycle()
    {
        for (int f = 0; f < foundations.Count; f++)
        {
            if (foundations[f].GetComponent<FoundationScript>().cardList.Count != 0)
            {
                GameObject topFoundationCard = foundations[f].GetComponent<FoundationScript>().cardList[0];
                for (int r = 0; r < reactors.Count; r++)
                {
                    if (topFoundationCard.GetComponent<CardScript>().cardSuit == reactors[r].GetComponent<ReactorScript>().suit)
                    {
                        topFoundationCard.GetComponent<CardScript>().MoveCard(reactors[r], isCycle: true);
                        break;
                    }
                }
            }
        }
    }

    // moves all wastePile cards into the deck
    public void DeckReset()
    {
        wastePileScript.DeckReset();
        soundController.DeckReshuffle();
    }

    // deals cards
    public void Deal(bool log = true)
    {
        if (wastePileScript.isScrolling())
        {
            return;
        }

        List<GameObject> toMoveList = new List<GameObject>();
        for (int i = 0; i < Config.config.cardsToDeal; i++) // try to deal set number of cards
        {
            if (cardList.Count <= i) // are there no more cards in the deck?
            {
                break;
            }

            toMoveList.Add(cardList[i]);
        }

        if (toMoveList.Count != 0)
        {
            wastePileScript.AddCards(toMoveList);
        }
    }

    public void ImportShuffleSeed(int seed)
    {
        shuffleSeed = seed;
        importSeed = true;
    }

    //Shuffles cardList using Knuth shuffle aka Fisher-Yates shuffle
    public void Shuffle()
    {
        if (!importSeed)
        {
            System.Random rand1 = new System.Random();
            shuffleSeed = rand1.Next();
        }
        else
        {
            importSeed = false;
        }

        System.Random rand2 = new System.Random(shuffleSeed);

        for (int i = 0; i < cardList.Count; i++)
        {
            int j = rand2.Next(i, cardList.Count);
            GameObject temp = cardList[i];
            cardList[i] = cardList[j];
            cardList[j] = temp;
        }
    }
}
