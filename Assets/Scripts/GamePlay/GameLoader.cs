using System;
using System.Collections.Generic;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    public GameObject cardPrefab;

    public GameObject[] topSuitObjects, bottomSuitObjects;
    public Sprite[] allSuitSprites;
    private Sprite[] suitSpritesToUse;

    public Sprite[] holograms;
    public Sprite[] combinedHolograms;


    // Singleton instance.
    public static GameLoader Instance = null;

    private void Awake()
    {
        // Initialize the singleton instance.
        if (Instance == null)
        {
            Instance = this;

            // suit sprites
            suitSpritesToUse = GetSuitSprites();
            SetRectorSuitSprites(suitSpritesToUse);
        }
        else if (Instance != this)
        {
            throw new Exception("two of these scripts should not exist at the same time");
        }
    }

    public void LoadGame()
    {
        // Figure out what kinda game to start
        if (Config.Instance.tutorialOn)
        {
            LoadTutorial(Constants.tutorialStateStartFileName, gameStart: true);
        }
        else if (Config.Instance.continuing)
        {
            Debug.Log("loading saved game");
            MoveCardsToLoadPile(GetNewCards());
            StateLoader.Instance.LoadSaveState();
        }
        else
        {
            StartNewGame(GetNewCards());
        }
    }

    public void LoadTutorial(string fileName, bool gameStart = false)
    {
        Debug.Log("loading tutorial");
        if (gameStart)
        {
            MoveCardsToLoadPile(GetNewCards());
        }
        else
        {
            MoveCardsToLoadPile(GetAllCards());
        }

        StateLoader.Instance.LoadTutorialState(fileName);
    }

    public void RestartGame()
    {
        Config.Instance.gamePaused = true;

        List<GameObject> cards = GetAllCards();
        MoveCardsToLoadPile(cards);
        StartNewGame(cards);

        Config.Instance.gamePaused = false;
    }

    private List<GameObject> GetNewCards()
    {
        List<GameObject> newCards = new List<GameObject>();

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
                GameObject newCard = Instantiate(cardPrefab);
                CardScript newCardScript = newCard.GetComponent<CardScript>();

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
                newCardScript.suit = Constants.suits[suit];

                // setting up the in-game appearance of the card's rank color
                newCardScript.rankObject.GetComponent<TextMesh>().color = rankColor;

                // setting up the in-game appearance of the card's suit
                newCardScript.suitObject.GetComponent<SpriteRenderer>().sprite = suitSpritesToUse[suit];

                newCard.name = $"Card: {rank}, {Constants.suits[suit]}";
                newCards.Add(newCard);
                cardIndex += 1;
            }
        }

        return newCards;
    }

    private List<GameObject> GetAllCards()
    {
        List<GameObject> cards = new List<GameObject>();

        foreach (FoundationScript foundationScript in UtilsScript.Instance.foundationScripts)
        {
            cards.AddRange(foundationScript.cardList);
        }
        foreach (ReactorScript reactorScript in UtilsScript.Instance.reactorScripts)
        {
            cards.AddRange(reactorScript.cardList);
        }
        cards.AddRange(DeckScript.Instance.cardList);
        cards.AddRange(WastepileScript.Instance.cardList);
        cards.AddRange(MatchedPileScript.Instance.cardList);

        return cards;
    }

    private void StartNewGame(List<GameObject> cards)
    {
        // the game difficultuy should already be set to what is desired for things to work properly

        // remove old stuff
        UndoScript.Instance.moveLog.Clear();
        SaveState.Delete();

        // reset game values
        Config.Instance.consecutiveMatches = 0;
        Config.Instance.moveCounter = 0;
        Config.Instance.gameOver = false;
        Config.Instance.gameWin = false;

        // these are updated visually as well
        Config.Instance.score = 0;
        UtilsScript.Instance.UpdateScore(0, setAsValue: true);

        Config.Instance.actions = 0;
        UtilsScript.Instance.UpdateActions(0, startingGame: true);

        foreach (ReactorScript reactorScript in UtilsScript.Instance.reactorScripts)
        {
            reactorScript.SetReactorScore(0);
            reactorScript.AlertOff();
        }

        ActionCountScript.Instance.TurnSirenOff();

        cards = ShuffleCards(cards);
        cards = SetUpFoundations(cards);
        MoveCardsToDeck(cards);

        DeckScript.Instance.Deal(false);
    }

    private List<GameObject> ShuffleCards(List<GameObject> cards)
    {
        System.Random rand = new System.Random();
        int count = cards.Count;
        int length = count - 1;
        int j;
        GameObject temp;
        for (int i = 0; i < length; i++)
        {
            j = rand.Next(i, count);
            temp = cards[i];
            cards[i] = cards[j];
            cards[j] = temp;
        }
        return cards;
    }

    private List<GameObject> SetUpFoundations(List<GameObject> cards)
    {
        CardScript currentCardScript;
        foreach (FoundationScript foundationScript in UtilsScript.Instance.foundationScripts)
        {
            for (int i = 0; i < Config.GameValues.foundationStartingSize - 1; i++)
            {
                currentCardScript = cards[0].GetComponent<CardScript>();
                currentCardScript.MoveCard(foundationScript.gameObject, doLog: false, showHolo: false);
                currentCardScript.SetFoundationVisibility(false);
                cards.RemoveAt(0);
            }

            // adding and revealing the top card of the foundation
            currentCardScript = cards[0].GetComponent<CardScript>();
            currentCardScript.MoveCard(foundationScript.gameObject, doLog: false);
            cards.RemoveAt(0);
        }

        // for testing out max foundation stack size
        //for (int i = 0; i < 12; i++)
        //{
        //    currentCardScript = cards[0].GetComponent<CardScript>();
        //    currentCardScript.MoveCard(UtilsScript.Instance.foundations[0], doLog: false);
        //    cards.RemoveAt(0);
        //}

        return cards;
    }

    private void MoveCardsToDeck(List<GameObject> cards)
    {
        foreach (GameObject card in cards)
        {
            card.GetComponent<CardScript>().MoveCard(DeckScript.Instance.gameObject, doLog: false);
        }
    }

    private void MoveCardsToLoadPile(List<GameObject> cards)
    {
        foreach (GameObject card in cards)
        {
            card.GetComponent<CardScript>().MoveCard(LoadPileScript.Instance.gameObject, doLog: false, isAction: false);
        }
    }

    private Sprite[] GetSuitSprites()
    {
        // getting user setting
        bool isOn;
        if (System.Boolean.TryParse(PlayerPrefs.GetString(Constants.foodSuitsEnabledKey), out isOn))
        { }
        else
        {
            // unable to parse
            isOn = false;
        }

        // the food sprites start at index 0, classic at 4
        int suitSpritesIndex = isOn ? 0 : 4;

        // getting a subset list of suit sprites to use for the token/cards
        Sprite[] suitSpritesSubset = new Sprite[4];
        Array.Copy(allSuitSprites, suitSpritesIndex, suitSpritesSubset, 0, 4);
        return suitSpritesSubset;
    }

    private void SetRectorSuitSprites(Sprite[] suitSpritesSubset)
    {
        if (suitSpritesSubset.Length != 4)
        {
            throw new IndexOutOfRangeException("there needs to be exacty 4 suits");
        }

        // setting up the reactor suit images
        for (int i = 0; i < 4; i++)
        {
            topSuitObjects[i].GetComponent<SpriteRenderer>().sprite = suitSpritesSubset[i];
            bottomSuitObjects[i].GetComponent<SpriteRenderer>().sprite = suitSpritesSubset[i];
        }
    }
}