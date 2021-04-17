using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.IO;

public class DeckScript : MonoBehaviour
{
    public static DeckScript deckScript;

    private UtilsScript utils;
    public GameObject wastePile;
    private WastepileScript wastePileScript;

    public GameObject cardPrefab;
    private Sprite[] suitSprites;
    public Sprite[] holograms;
    public Sprite[] combinedHolograms;

    public List<GameObject> cardList;

    private Image buttonImage;
    public Sprite[] buttonAnimation;
    public Text deckCounter;

    public SoundController soundController;

    private void Awake()
    {
        if (deckScript == null)
        {
            //DontDestroyOnLoad(gameObject); //makes instance persist across scenes
            deckScript = this;
        }
        else if (deckScript != this)
        {
            Destroy(gameObject); //deletes copies of global which do not need to exist, so right version is used to get info from
        }
    }

    public void DeckStart(Sprite[] suitSprites)
    {
        utils = UtilsScript.global;
        wastePileScript = wastePile.GetComponent<WastepileScript>();
        buttonImage = gameObject.GetComponent<Image>();

        this.suitSprites = suitSprites;
        cardList = new List<GameObject>();
        if ((File.Exists("Assets/Resources/GameStates/testState.json") && Application.isEditor) || Config.config.tutorialOn)
        {
            StateLoader.saveSystem.UnpackState(StateLoader.saveSystem.gameState, false);
            utils.UpdateScore(0);
            //print("Loading save mode 1");
        }
        else if (File.Exists(Application.persistentDataPath + "/testState.json") && !Application.isEditor)
        {
            StateLoader.saveSystem.UnpackState(StateLoader.saveSystem.gameState, false);
            utils.UpdateScore(0);
            //print("Loading save mode 2");
        }
        else
        {
            //print("New Game");
            InstantiateCards();
            Shuffle();
            SetUpFoundations();
            Deal(false);
            utils.UpdateActions(0, startingGame: true);
        }
    }

    // sets up card list
    public void InstantiateCards(bool addToLoadPile = false)
    {
        LoadPileScript lps = Config.config.loadPile.GetComponent<LoadPileScript>();

        string[] suitStrings = new string[] { "spades", "clubs", "diamonds", "hearts" };

        GameObject newCard;
        CardScript newCardScript;

        // order: spade ace, 2, 3... 10, jack, queen, king, clubs... diamonds... hearts
        int cardIndex = 0; // 1 - 52
        int hFSIndex; // used for assigning holograms
        int rank;
        Color rankColor = Color.black;
        for (int suit = 0; suit < 4; suit++) // order: spades, clubs, diamonds, hearts
        {
            if (suit == 2)
                rankColor = Color.red;

            hFSIndex = suit * 5;

            for (rank = 1; rank < 14; rank++) // card num: 1 - 13
            {
                newCard = Instantiate(cardPrefab);
                newCardScript = newCard.GetComponent<CardScript>();

                // setting up the cards reactor value, in-game appearance, and hologram sprites
                if (rank < 10)
                {
                    // reactor value
                    newCardScript.cardVal = rank;

                    // in-game appearance of the card's rank
                    if (rank == 1)
                        newCardScript.rankObject.GetComponent<TextMesh>().text = "A";
                    else
                        newCardScript.rankObject.GetComponent<TextMesh>().text = rank.ToString();

                    // basic hologram shown
                    newCardScript.hologramFoodSprite = holograms[hFSIndex];

                    // hologram shown during match
                    if (suit < 2)
                        newCardScript.hologramComboSprite = combinedHolograms[0];
                    else
                        newCardScript.hologramComboSprite = combinedHolograms[5];
                }
                else
                {
                    // reactor value, all face cards have a value of 10
                    newCardScript.cardVal = 10;

                    // in-game appearance of the card's rank
                    if (rank == 10)
                        newCardScript.rankObject.GetComponent<TextMesh>().text = "10";
                    else if (rank == 11)
                        newCardScript.rankObject.GetComponent<TextMesh>().text = "J";
                    else if (rank == 12)
                        newCardScript.rankObject.GetComponent<TextMesh>().text = "Q";
                    else
                        newCardScript.rankObject.GetComponent<TextMesh>().text = "K";

                    // all cards >10 have fancy holograms, this is a complex way of assigning them
                    newCardScript.hologramFoodSprite = holograms[rank - (9 - hFSIndex)];

                    // cards >10 have fancy holograms for matching as well
                    if (suit < 2)
                        newCardScript.hologramComboSprite = combinedHolograms[rank - 9];
                    else
                        newCardScript.hologramComboSprite = combinedHolograms[rank - 4];
                }

                // setting up the cards internal rank and suit
                newCardScript.cardNum = rank;
                newCardScript.cardSuit = suitStrings[suit];

                // setting up the text renderer's sorting layer and order because you can't do it via Unity's Inspector
                newCardScript.rankObject.GetComponent<MeshRenderer>().sortingLayerName = "Gameplay";
                newCardScript.rankObject.GetComponent<MeshRenderer>().sortingOrder = 1;

                // setting up the in-game appearance of the card's rank color
                newCardScript.rankObject.GetComponent<TextMesh>().color = rankColor;

                // setting up the in-game appearance of the card's suit
                newCardScript.suitObject.GetComponent<SpriteRenderer>().sprite = suitSprites[suit];

                // moving card to desired location
                if (!addToLoadPile)
                {
                    newCardScript.container = this.gameObject;
                    AddCard(newCard);
                }
                else
                {
                    newCardScript.container = Config.config.loadPile;
                    lps.AddCard(newCard);
                }
                
                cardIndex += 1;
            }
        }
    }

    // moves cards into foundations
    public void SetUpFoundations()
    {
        CardScript currentCardScript;
        foreach (GameObject foundation in Config.config.foundations)
        {
            for (int n = 0; n < Config.config.foundationStartSize - 1; n++)
            {
                currentCardScript = cardList[0].GetComponent<CardScript>();
                currentCardScript.MoveCard(foundation, doLog: false, showHolo: false);
                currentCardScript.SetFoundationVisibility(false);
            }

            // adding and revealing the top card of the foundation
            currentCardScript = cardList[0].GetComponent<CardScript>();
            currentCardScript.MoveCard(foundation, doLog: false);
        }

        // testing purposes: this makes foundation 0 contain the max number of cards a foundation can carry at one
        /*GameObject foundation0 = Config.config.foundations[1];
        for (int i = 0; i < 12; i++)
        {
            currentCardScript = cardList[0].GetComponent<CardScript>();
            currentCardScript.MoveCard(foundation0, doLog: false);
            currentCardScript.SetVisibility(true);
        }*/
    }

    public void AddCard(GameObject card)
    {
        cardList.Insert(0, card);
        card.transform.SetParent(gameObject.transform);
        card.transform.localPosition = Vector3.zero;
        card.GetComponent<CardScript>().SetGameplayVisibility(false);
        UpdateDeckCounter();
    }

    public void RemoveCard(GameObject card)
    {
        cardList.Remove(card);
        card.GetComponent<CardScript>().SetGameplayVisibility(true);
        UpdateDeckCounter();
    }

    public void ProcessAction()
    {
        // is called by the deck button
        // user wants to deal cards, other things might need to be done before that

        // don't allow dealing when other stuff is happening
        if (utils.IsInputStopped())
            return;

        if (cardList.Count != 0) // can the deck can be drawn from
        {
            soundController.DeckDeal();
            Deal();

            StartCoroutine(ButtonDown());
        }
        // if it is possible to repopulate the deck
        else if (wastePileScript.cardList.Count > Config.config.cardsToDeal) 
        {
            DeckReset();
            StartCoroutine(ButtonDown());
        }
    }

    public void Deal(bool doLog = true)
    {
        List<GameObject> toMoveList = new List<GameObject>();
        for (int i = 0; i < Config.config.cardsToDeal; i++) // try to deal set number of cards
        {
            if (cardList.Count <= i) // are there no more cards in the deck?
                break;

            toMoveList.Add(cardList[i]);
        }

        if (toMoveList.Count != 0)
            wastePileScript.AddCards(toMoveList, doLog);
    }

    public void DeckReset()
    {
        // moves all wastePile cards into the deck

        wastePileScript.StartDeckReset();
        soundController.DeckReshuffle();
    }

    IEnumerator ButtonDown()
    {
        foreach (Sprite button in buttonAnimation)
        {
            buttonImage.sprite = button;
            yield return new WaitForSeconds(0.08f);
        }
    }

    public void StartButtonUp()
    {
        StartCoroutine(ButtonUp());
    }

    IEnumerator ButtonUp()
    {
        for (int i = buttonAnimation.Length - 2; i > 0; i--)
        {
            buttonImage.sprite = buttonAnimation[i];
            yield return new WaitForSeconds(0.08f);
        }
    }

    // moves all of the top foundation cards into their appropriate reactors
    public void StartNextCycle(bool manuallyTriggered = false)
    {
        if (!(manuallyTriggered && utils.IsInputStopped())) // stops 2 NextCycles from happening at once
        {
            utils.SetInputStopped(true, nextCycle: true);
            StartCoroutine(NextCycle());
        }
    }

    IEnumerator NextCycle()
    {
        utils.baby.GetComponent<SpaceBabyController>().BabyActionCounterSound();

        FoundationScript currentFoundation;
        GameObject topFoundationCard;
        CardScript topCardScript;

        foreach (GameObject foundation in Config.config.foundations)
        {
            currentFoundation = foundation.GetComponent<FoundationScript>();
            if (currentFoundation.cardList.Count != 0)
            {
                topFoundationCard = currentFoundation.cardList[0];
                topCardScript = topFoundationCard.GetComponent<CardScript>();

                foreach (GameObject reactor in Config.config.reactors)
                {
                    if (topCardScript.cardSuit == reactor.GetComponent<ReactorScript>().suit)
                    {
                        topCardScript.HideHologram();
                        topFoundationCard.GetComponent<SpriteRenderer>().sortingLayerName = "SelectedCards";
                        topCardScript.suitObject.GetComponent<SpriteRenderer>().sortingLayerName = "SelectedCards";
                        topCardScript.rankObject.GetComponent<MeshRenderer>().sortingLayerName = "SelectedCards";

                        Vector3 target = reactor.transform.position;
                        int cardCount = reactor.GetComponent<ReactorScript>().cardList.Count;
                        if (cardCount > 4)
                            cardCount = 4;

                        target.y += -0.8f + cardCount * 0.45f;
                        target.x -= 0.02f;

                        // immediately unhide the next possible top foundation card and start its hologram
                        if (currentFoundation.cardList.Count > 1)
                        {
                            CardScript nextTopFoundationCard = currentFoundation.cardList[1].GetComponent<CardScript>();
                            if (nextTopFoundationCard.IsHidden)
                            {
                                nextTopFoundationCard.SetFoundationVisibility(true, isNotForNextCycle: false);
                                nextTopFoundationCard.ShowHologram();
                            }
                        }

                        while (topFoundationCard.transform.position != target)
                        {   
                            topFoundationCard.transform.position = Vector3.MoveTowards(topFoundationCard.transform.position, target,
                                Time.deltaTime * Config.config.cardsToReactorspeed);
                            yield return null;
                        }

                        topFoundationCard.GetComponent<SpriteRenderer>().sortingLayerName = "Gameplay";
                        topCardScript.suitObject.GetComponent<SpriteRenderer>().sortingLayerName = "Gameplay";
                        topCardScript.rankObject.GetComponent<MeshRenderer>().sortingLayerName = "Gameplay";

                        soundController.CardToReactorSound();
                        topCardScript.MoveCard(reactor, isCycle: true);

                        if (Config.config.gameOver)
                        {
                            Config.config.moveCounter += 1;
                            yield break;
                        }

                        break;
                    }
                }
            }
        }

        utils.SetInputStopped(false, nextCycle: true);
        utils.UpdateActions(0, setAsValue: true);
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

    public void UpdateDeckCounter()
    {
        if (cardList.Count != 0)
        {
            deckCounter.fontSize = 50;
            deckCounter.text = cardList.Count.ToString();
        }
        else
        {
            if (wastePileScript.cardList.Count > Config.config.cardsToDeal)
            {
                deckScript.deckCounter.fontSize = 45;
                deckScript.deckCounter.text = "FLIP";
            }
            else
            {
                deckScript.deckCounter.fontSize = 40;
                deckScript.deckCounter.text = "EMPTY";
            }
        }
    }
}
