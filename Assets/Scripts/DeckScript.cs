using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.IO;

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
    public List<Sprite> holograms;
    public List<GameObject> cardList;

    private Image buttonImage;
    public List<Sprite> buttonAnimation;
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

        utils.UpdateActionCounter(0, true);

        cardList = new List<GameObject>();
        if ((File.Exists("Assets/Resources/GameStates/testState.json") && Application.isEditor) || Config.config.tutorialOn)
        {
            StateLoader.saveSystem.unpackState(StateLoader.saveSystem.gameState);
            utils.UpdateScore(0);
            print("Loading save mode 1");
        }
        else if (File.Exists(Application.persistentDataPath + "/testState.json") && !Application.isEditor)
        {
            StateLoader.saveSystem.unpackState(StateLoader.saveSystem.gameState);
            utils.UpdateScore(0);
            print("Loading save mode 2");
        }
        else
        {
            print("New Game");
            InstantiateCards(this.gameObject);
            importSeed = false;
            Shuffle();
            SetUpFoundations();
            Deal(false);
        }
    }

    // sets up card list
    public void InstantiateCards(GameObject target)
    {
        GameObject newCard;
        CardScript newCardScript;

        // order: club ace, 2, 3... 10, jack, queen, king, spades... hearts... diamonds
        int cardIndex = 0; // 1 - 52
        for (int suit = 0; suit < 4; suit++) // order: club, spades, hearts, diamonds
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
                    if (num < 10)
                    {
                        newCardScript.hologramFood.GetComponent<SpriteRenderer>().sprite = holograms[0];
                    }
                    else
                    {
                        newCardScript.hologramFood.GetComponent<SpriteRenderer>().sprite = holograms[num - 9];
                    }
                }
                else if (suit == 1)
                {
                    newCardScript.cardSuit = "spades";
                    if (num < 10)
                    {
                        newCardScript.hologramFood.GetComponent<SpriteRenderer>().sprite = holograms[5];
                    }
                    else
                    {
                        newCardScript.hologramFood.GetComponent<SpriteRenderer>().sprite = holograms[num - 4];
                    }
                }
                else if (suit == 2)
                {
                    newCardScript.cardSuit = "hearts";
                    if (num < 10)
                    {
                        newCardScript.hologramFood.GetComponent<SpriteRenderer>().sprite = holograms[10];
                    }
                    else
                    {
                        newCardScript.hologramFood.GetComponent<SpriteRenderer>().sprite = holograms[num + 1];
                    }
                }
                else if (suit == 3)
                {
                    newCardScript.cardSuit = "diamonds";
                    if (num < 10)
                    {
                        newCardScript.hologramFood.GetComponent<SpriteRenderer>().sprite = holograms[15];
                    }
                    else
                    {
                        newCardScript.hologramFood.GetComponent<SpriteRenderer>().sprite = holograms[num + 6];
                    }
                }

                newCardScript.number.GetComponent<SpriteRenderer>().sprite = sprites[cardIndex];
                newCardScript.SetVisibility(true);
                newCardScript.container = target;
                if (target.CompareTag("Deck"))
                {
                    AddCard(newCard);
                }
                else if (target.CompareTag("LoadPile"))
                {
                    target.GetComponent<LoadPileScript>().AddCard(newCard);
                }
                
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
            cardList[0].SetActive(true);
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

        deckCounter.fontSize = 50;
        deckCounter.text = cardList.Count.ToString();
    }

    public void RemoveCard(GameObject card, bool checkHolo = false)
    {
        cardList.Remove(card);
        card.SetActive(true);

        if (cardList.Count != 0)
        {
            deckCounter.fontSize = 50;
            deckCounter.text = cardList.Count.ToString();
        }
    }

    // user wants to deal cards, other things might need to be done before that
    public void ProcessAction(GameObject input)
    {
        utils.DeselectCards();

        if (utils.IsInputStopped()) // the deck button directly calls ProcessAction
        {
            return;
        }

        if (cardList.Count != 0) // can the deck can be drawn from
        {
            soundController.DeckDeal();
            Deal();

            //Animator dealAnim = gameObject.GetComponent<Animator>();
            //dealAnim.enabled = true;
            //dealAnim.Play("ConveyorButtonAnim");

            StartCoroutine(ButtonDown());
        }
        else // we need to try repopulating the deck
        {
            if (wastePileScript.GetCardList().Count <= Config.config.cardsToDeal) // is it not possible to repopulate the deck?
            {
                return;
            }

            DeckReset();
            StartCoroutine(ButtonDown());
        }
    }

    IEnumerator ButtonDown()
    {
        for (int i = 1; i < buttonAnimation.Count; i++)
        {
            buttonImage.sprite = buttonAnimation[i];
            yield return new WaitForSeconds(0.08f);
        }
    }

    public void StartButtonUp()
    {
        StartCoroutine(ButtonUp());
    }

    IEnumerator ButtonUp()
    {
        for (int i = buttonAnimation.Count - 2; i > 0; i--)
        {
            buttonImage.sprite = buttonAnimation[i];
            yield return new WaitForSeconds(0.08f);
        }
    }

    // moves all of the top foundation cards into their appropriate reactors
    public void StartNextCycle(bool manuallyTriggered = false)
    {
        if (manuallyTriggered && utils.IsInputStopped()) // stops 2 NextCycles from happening at once
        {
            return;
        }

        utils.SetInputStopped(true);
        StartCoroutine(NextCycle());
    }

    IEnumerator NextCycle()
    {
        utils.baby.GetComponent<SpaceBabyController>().BabyActionCounterSound();

        FoundationScript currentFoundation;
        GameObject topFoundationCard;
        CardScript topCardScript;
        for (int f = 0; f < foundations.Count; f++)
        {
            currentFoundation = foundations[f].GetComponent<FoundationScript>();
            if (currentFoundation.cardList.Count != 0)
            {
                topFoundationCard = currentFoundation.cardList[0];
                topCardScript = topFoundationCard.GetComponent<CardScript>();
                for (int r = 0; r < reactors.Count; r++)
                {
                    if (topCardScript.cardSuit == reactors[r].GetComponent<ReactorScript>().suit)
                    {
                        topCardScript.HideHologram();
                        topFoundationCard.GetComponent<SpriteRenderer>().sortingLayerName = "SelectedCards";
                        topCardScript.number.GetComponent<SpriteRenderer>().sortingLayerName = "SelectedCards";
                        Vector3 target = reactors[r].transform.position;

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
                        topCardScript.MoveCard(reactors[r], isCycle: true);

                        if (Config.config.gameOver)
                        {
                            yield break;
                        }

                        break;
                    }
                }
            }
        }

        utils.SetInputStopped(false);
        utils.UpdateActionCounter(0, true);
        utils.CheckGameOver();
    }



    // moves all wastePile cards into the deck
    public void DeckReset()
    {
        wastePileScript.DeckReset();
        soundController.DeckReshuffle();
    }

    // deals cards
    public void Deal(bool doLog = true)
    {
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
            wastePileScript.AddCards(toMoveList, doLog);
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
