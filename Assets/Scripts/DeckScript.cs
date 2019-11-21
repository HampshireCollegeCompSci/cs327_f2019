using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class DeckScript : MonoBehaviour
{
    public static DeckScript deckScript;

    public UtilsScript utils;
    public GameObject wastePile;
    public List<GameObject> foundations;
    public List<GameObject> reactors;

    public GameObject myPrefab;
    public List<Sprite> sprites;
    public List<GameObject> cardList;

    public Text deckCounter;

    public SoundController soundController;

    // public bool shuffleOnDeckReset = false;
    public bool dealOnDeckReset = true;
    public int foundationStartSize;

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
        //myPrefab = (GameObject)Resources.Load("Prefabs/Card", typeof(GameObject));
        //myPrefab.GetComponent<BoxCollider2D>().size = new Vector2Int(1, 1);
        //myPrefab.GetComponent<BoxCollider2D>().offset = new Vector2Int(0, 0);

        utils = UtilsScript.global;

        InstantiateCards(gameObject);
        importSeed = false;
        Shuffle();
        SetUpFoundations();
        Deal(false);
        SetCardPositions();

        deckCounter.text = cardList.Count.ToString();
    }

    // sets up card list
    public void InstantiateCards(GameObject target)
    {
        cardList = new List<GameObject>();
        for (int i = 0; i < 52; i++)
        {
            GameObject newCard = Instantiate(myPrefab);
            //newCard.transform.localScale = new Vector3(0.15f, 0.15f, 1);
            AddCard(newCard);
            //cardList.Add(Instantiate(myPrefab));
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

                cardList[cardIndex].GetComponent<CardScript>().cardFrontSprite = sprites[cardIndex];
                cardList[cardIndex].GetComponent<CardScript>().hidden = true;
                cardList[cardIndex].GetComponent<CardScript>().appearSelected = false;
                cardList[cardIndex].GetComponent<CardScript>().container = target;

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
                cardList[0].SetActive(true);
                // set to hidden as they might be unhidden
                cardList[0].GetComponent<CardScript>().hidden = true;
                // MoveCard() should be removing the card from its current cardList so taking index 0 should work
                cardList[0].GetComponent<CardScript>().MoveCard(foundations[i], false);
            }

            // adding and revealing the top card of the foundation
            cardList[0].SetActive(true);
            cardList[0].GetComponent<CardScript>().hidden = false;
            cardList[0].GetComponent<CardScript>().MoveCard(foundations[i], false);
            foundations[i].GetComponent<FoundationScript>().CheckTopCard();
        }
    }

    public void AddCard(GameObject card)
    {
        cardList.Insert(0, card);
        card.transform.SetParent(gameObject.transform);
        card.transform.localPosition = Vector3.zero;
        card.SetActive(false);
    }

    public void RemoveCard(GameObject card)
    {
        cardList.Remove(card);
    }

    // top card is cardList[0]
    public void SetCardPositions()
    { }

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
            if (wastePile.GetComponent<WastepileScript>().GetCardList().Count == 0) // is it not possible to repopulate the deck?
            {
                return;
            }

            soundController.DeckReshuffle();
            //NextCycle();
            DeckReset();
        }

        deckCounter.text = cardList.Count.ToString();
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
                    //  does this top card's suit match the reactor's suit
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
        // get all the cards from the wastepile
        List<GameObject> wasteCardList = wastePile.GetComponent<WastepileScript>().GetCardList();

        // move all the cards from waste to deck, preserves reveal order
        // if there are any cards in the deck's cardList before they will be on the bottom after
        while (wasteCardList.Count > 0)
        {
            wasteCardList[0].GetComponent<CardScript>().MoveCard(this.gameObject, false);
            cardList[0].GetComponent<CardScript>().hidden = true;
            cardList[0].GetComponent<CardScript>().SetCardAppearance();
        }

        // if (shuffleOnDeckReset) // seed saving for this not implemented yet
        // {
        //     Shuffle();
        // }

        if (dealOnDeckReset) // auto deal cards
        {
            Deal();
        }
    }

    // deals cards
    public void Deal(bool log = true)
    {
        List<GameObject> toMoveList = new List<GameObject>();
        for (int i = 0; i < Config.config.cardsToDeal; i++) // try to deal set number of cards
        {
            if (cardList.Count == 0) // are there no more cards in the deck?
            {
                break;
            }

            // reveal card and move from deck list top into waste

            cardList[0].SetActive(true);
            cardList[0].GetComponent<CardScript>().hidden = false;
            cardList[0].GetComponent<CardScript>().SetCardAppearance();

            //toMoveList.Add(cardList[0]);
            //cardList.RemoveAt(0);

            if (log)
            {
                cardList[0].GetComponent<CardScript>().MoveCard(wastePile);
            }
            else
            {
                cardList[0].GetComponent<CardScript>().MoveCard(wastePile, false);
            }
        }

        //if (toMoveList.Count != 0)
        //{
        //    wastePile.GetComponent<WastepileScript>().AddCards(toMoveList);
        //    Config.config.actions += 1; //adds to the action count
        //}

        Config.config.actions += 1; //adds to the action count
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
