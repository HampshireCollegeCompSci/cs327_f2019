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

    public GameObject myPrefab;
    public Sprite[] sprites;
    public Sprite[] holograms;
    public Sprite[] combinedHolograms;

    public List<GameObject> cardList;
    private String[] suits;

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

    public void DeckStart()
    {
        utils = UtilsScript.global;
        wastePileScript = wastePile.GetComponent<WastepileScript>();
        buttonImage = gameObject.GetComponent<Image>();

        cardList = new List<GameObject>();
        if ((File.Exists("Assets/Resources/GameStates/testState.json") && Application.isEditor) || Config.config.tutorialOn)
        {
            StateLoader.saveSystem.unpackState(StateLoader.saveSystem.gameState);
            utils.UpdateScore(0);
            //print("Loading save mode 1");
        }
        else if (File.Exists(Application.persistentDataPath + "/testState.json") && !Application.isEditor)
        {
            StateLoader.saveSystem.unpackState(StateLoader.saveSystem.gameState);
            utils.UpdateScore(0);
            //print("Loading save mode 2");
        }
        else
        {
            //print("New Game");
            InstantiateCards(this.gameObject);
            Shuffle();
            SetUpFoundations();
            Deal(false);
            utils.UpdateActions(0, true);
        }
    }

    // sets up card list
    public void InstantiateCards(GameObject target)
    {
        suits = new String[4] { "clubs", "spades", "hearts", "diamonds" };

        GameObject newCard;
        CardScript newCardScript;

        // order: club ace, 2, 3... 10, jack, queen, king, spades... hearts... diamonds
        int cardIndex = 0; // 1 - 52
        int hFSIndex;
        int num;
        for (int suit = 0; suit < 4; suit++) // order: club, spades, hearts, diamonds
        {
            hFSIndex = suit * 5;
            for (num = 1; num < 14; num++) // card num: 1 - 13
            {
                newCard = Instantiate(myPrefab);
                newCardScript = newCard.GetComponent<CardScript>();

                if (num < 10)
                {
                    newCardScript.cardVal = num;
                    newCardScript.hologramFoodSprite = holograms[hFSIndex];

                    if (suit < 2)
                        newCardScript.hologramComboSprite = combinedHolograms[0];
                    else
                        newCardScript.hologramComboSprite = combinedHolograms[5];
                }
                else
                {
                    // all face cards have a value of 10
                    newCardScript.cardVal = 10;
                    newCardScript.hologramFoodSprite = holograms[num - (9 - hFSIndex)];
                    
                    if (suit < 2)
                        newCardScript.hologramComboSprite = combinedHolograms[num - 9];
                    else
                        newCardScript.hologramComboSprite = combinedHolograms[num - 4];
                }

                newCardScript.cardNum = num;
                newCardScript.cardSuit = suits[suit];

                newCardScript.number.GetComponent<SpriteRenderer>().sprite = sprites[cardIndex];
                newCardScript.SetVisibility(true);
                newCardScript.container = target;
                
                if (target.CompareTag("Deck"))
                    AddCard(newCard);
                else if (target.CompareTag("LoadPile"))
                    target.GetComponent<LoadPileScript>().AddCard(newCard);
                
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
                currentCardScript.MoveCard(foundation, doLog: false, doSave: false);
                currentCardScript.SetVisibility(false);
            }

            // adding and revealing the top card of the foundation
            currentCardScript = cardList[0].GetComponent<CardScript>();
            currentCardScript.SetVisibility(true);
            currentCardScript.ShowHologram();
            currentCardScript.MoveCard(foundation, doLog: false, doSave: false);
        }
    }

    public void AddCard(GameObject card)
    {
        cardList.Insert(0, card);
        card.transform.SetParent(gameObject.transform);
        card.transform.localPosition = Vector3.zero;
        card.GetComponent<CardScript>().HideHologram();
        card.GetComponent<SpriteRenderer>().enabled = false;
        card.GetComponent<BoxCollider2D>().enabled = false;
        UpdateDeckCounter();
    }

    public void RemoveCard(GameObject card)
    {
        cardList.Remove(card);
        card.GetComponent<SpriteRenderer>().enabled = true;
        card.GetComponent<BoxCollider2D>().enabled = true;
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
            utils.SetInputStopped(true);
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
                        topCardScript.number.GetComponent<SpriteRenderer>().sortingLayerName = "SelectedCards";

                        Vector3 target = reactor.transform.position;
                        target.y += -0.6f + reactor.GetComponent<ReactorScript>().cardList.Count * 0.45f;

                        if (currentFoundation.cardList.Count > 1)
                        {
                            CardScript nextTopFoundationCard = currentFoundation.cardList[1].GetComponent<CardScript>();
                            if (nextTopFoundationCard.isHidden())
                            {
                                nextTopFoundationCard.SetVisibility(true);
                                nextTopFoundationCard.ShowHologram();
                                nextTopFoundationCard.hidden = true; // for undo to work right
                            }
                        }

                        while (topFoundationCard.transform.position != target)
                        {   
                            topFoundationCard.transform.position = Vector3.MoveTowards(topFoundationCard.transform.position, target,
                                Time.deltaTime * Config.config.cardsToReactorspeed);
                            yield return null;
                        }

                        topFoundationCard.GetComponent<SpriteRenderer>().sortingLayerName = "Gameplay";
                        topCardScript.number.GetComponent<SpriteRenderer>().sortingLayerName = "Gameplay";
                        soundController.CardToReactorSound();
                        topCardScript.MoveCard(reactor, isCycle: true);

                        if (Config.config.gameOver)
                            yield break;

                        break;
                    }
                }
            }
        }

        utils.SetInputStopped(false);
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
