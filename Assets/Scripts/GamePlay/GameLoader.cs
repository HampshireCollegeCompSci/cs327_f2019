using System;
using System.Collections.Generic;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    // Singleton instance.
    public static GameLoader Instance;

    [SerializeField]
    private GameObject cardPrefab;
    [SerializeField]
    private GameObject[] topSuitObjects, bottomSuitObjects;
    [SerializeField]
    private Sprite[] allSuitSprites, suitSpritesToUse;
    [SerializeField]
    private Sprite[] holograms, combinedHolograms;

    private void Awake()
    {
        // Initialize the singleton instance.
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            throw new Exception("two of these scripts should not exist at the same time");
        }
    }

    public bool LoadGame()
    {
        // suit sprites
        suitSpritesToUse = GetSuitSprites();
        SetRectorSuitSprites(suitSpritesToUse);

        for (int i = 0; i < 4; i++)
        {
            GameInput.Instance.reactorScripts[i].SetReactorSuit(GameValues.GamePlay.suits[i]);
        }

        ResetState();

        // Figure out what kinda game to start
        if (Config.Instance.TutorialOn)
        {
            if (Config.Instance.prettyColors)
            {
                Config.Instance.prettyColors = false;
            }
            LoadTutorial(Constants.Tutorial.tutorialStateStartFileName, gameStart: true);
        }
        else if (Config.Instance.continuing)
        {
            Config.Instance.continuing = false;
            Debug.Log("loading saved game");
            MoveCardsToLoadPile(GetNewCards());

            try
            {
                StateLoader.Instance.LoadSaveState();
            }
            catch (Exception e)
            {
                Debug.LogError("failed to load the save state: " + e);
                SaveFile.Delete();
                return false;
            }
        }
        else
        {
            StartNewGame(GetNewCards());
        }

        if (Config.Instance.prettyColors)
        {
            Config.Instance.prettyColors = false;
        }
        return true;
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
            List<GameObject> cards = GetAllCards();
            MoveCardsToLoadPile(cards);
            foreach (GameObject card in cards)
            {
                card.GetComponent<CardScript>().SetValuesToDefault();
            }
        }

        StateLoader.Instance.LoadTutorialState(fileName);
    }

    public void RestartGame()
    {
        ResetState();
        List<GameObject> cards = GetAllCards();
        MoveCardsToLoadPile(cards);
        foreach (GameObject card in cards)
        {
            card.GetComponent<CardScript>().SetValuesToDefault();
        }
        StartNewGame(cards);
        Timer.StartWatch();
    }

    public void ChangeSuitSprites()
    {
        Debug.Log("changing suit sprites");
        Sprite[] suitSprites = GetSuitSprites();
        foreach (GameObject card in GetAllCards())
        {
            CardScript cs = card.GetComponent<CardScript>();
            cs.SetSuitSprite(suitSprites[cs.Card.Suit.Index]);
        }
        SetRectorSuitSprites(suitSprites);
    }

    private void ResetState()
    {
        Actions.ResetValues();
        ScoreScript.Instance.SetScore(0);
        EndGame.Instance.GameCanEnd = false;
        AchievementsManager.ClearAchievements();
        UndoScript.Instance.ResetValues();
        StateLoader.Instance.ResetValues();
    }

    private List<GameObject> GetNewCards()
    {
        List<GameObject> newCards = new(GameValues.GamePlay.cardCount);

        // order: spade ace, 2, 3... 10, jack, queen, king, clubs... diamonds... hearts
        int hFSIndex = 0; // used for assigning holograms
        foreach (Suit suit in GameValues.GamePlay.suits)
        {
            foreach (Rank rank in GameValues.GamePlay.ranks)
            {
                GameObject newCard = Instantiate(cardPrefab, this.gameObject.transform);

                Sprite hologramFoodSprite, hologramComboSprite;
                // setting up the cards reactor value, in-game appearance, and hologram sprites
                if (rank.Value < 10)
                {
                    hologramFoodSprite = holograms[hFSIndex];
                    hologramComboSprite = suit.Index < 2 ? combinedHolograms[0] : combinedHolograms[5];
                }
                else
                {
                    hFSIndex++;
                    // all cards >10 have fancy holograms, this is a complex way of assigning them
                    hologramFoodSprite = holograms[hFSIndex];
                    hologramComboSprite = suit.Index < 2 ? combinedHolograms[rank.Value - 9] : combinedHolograms[rank.Value - 4];
                }

                newCard.GetComponent<CardScript>().SetUp(new Card(suit, rank), suitSpritesToUse[suit.Index], hologramFoodSprite, hologramComboSprite);
                newCards.Add(newCard);
            }
            hFSIndex++;
        }

        return newCards;
    }

    private List<GameObject> GetAllCards()
    {
        List<GameObject> cards = new(GameValues.GamePlay.cardCount);

        foreach (FoundationScript foundationScript in GameInput.Instance.foundationScripts)
        {
            cards.AddRange(foundationScript.CardList);
        }
        foreach (ReactorScript reactorScript in GameInput.Instance.reactorScripts)
        {
            cards.AddRange(reactorScript.CardList);
        }
        cards.AddRange(DeckScript.Instance.CardList);
        cards.AddRange(WastepileScript.Instance.CardList);
        cards.AddRange(MatchedPileScript.Instance.CardList);

        return cards;
    }

    private void StartNewGame(List<GameObject> cards)
    {
        // the game difficultuy should already be set to what is desired for things to work properly
        SaveFile.Delete();
        Actions.StartNewGameUpdate();
        Timer.timerOffset = TimeSpan.Zero;

        foreach (ReactorScript reactorScript in GameInput.Instance.reactorScripts)
        {
            reactorScript.SetReactorScore(0);
            reactorScript.Alert = false;
        }

        ActionCountScript.Instance.AlertLevel = GameValues.Colors.normal;

        cards = ShuffleCards(cards);
        bool isCheating = Config.Instance.CurrentDifficulty.Equals(Difficulties.difficultyArray[3]);
        cards = isCheating switch
        {
            true => SetUpFoundations(cards, 1),
            false => SetUpFoundations(cards, GameValues.GamePlay.foundationStartingSize - 1)
        };

        MoveCardsToDeck(cards);

        DeckScript.Instance.Deal(false);
    }

    private List<GameObject> ShuffleCards(List<GameObject> cards)
    {
        System.Random rand = new();
        for (int i = 0; i < cards.Count; i++)
        {
            int j = rand.Next(i, cards.Count);
            (cards[j], cards[i]) = (cards[i], cards[j]);
        }
        return cards;
    }

    private List<GameObject> SetUpFoundations(List<GameObject> cards, int hiddenFoundationCards)
    {
        CardScript currentCardScript;
        foreach (FoundationScript foundationScript in GameInput.Instance.foundationScripts)
        {
            for (int i = 0; i < hiddenFoundationCards; i++)
            {
                currentCardScript = cards[^1].GetComponent<CardScript>();
                currentCardScript.MoveCard(Constants.CardContainerType.Foundation, foundationScript.gameObject, doLog: false, showHolo: false);
                currentCardScript.Hidden = true;
                cards.RemoveAt(cards.Count - 1);
            }

            // adding and revealing the top card of the foundation
            currentCardScript = cards[^1].GetComponent<CardScript>();
            currentCardScript.MoveCard(Constants.CardContainerType.Foundation, foundationScript.gameObject, doLog: false);
            cards.RemoveAt(cards.Count - 1);
        }

        // for testing out max foundation stack size
        //for (int i = 0; i < 12; i++)
        //{
        //    currentCardScript = cards[^1].GetComponent<CardScript>();
        //    currentCardScript.MoveCard(UtilsScript.Instance.foundations[0], doLog: false);
        //    cards.RemoveAt(cards.Count - 1);
        //}

        return cards;
    }

    private void MoveCardsToDeck(List<GameObject> cards)
    {
        foreach (GameObject card in cards)
        {
            card.GetComponent<CardScript>().MoveCard(Constants.CardContainerType.Deck, DeckScript.Instance.gameObject, doLog: false);
        }
    }

    private void MoveCardsToLoadPile(List<GameObject> cards)
    {
        foreach (GameObject card in cards)
        {
            card.GetComponent<CardScript>().MoveCard(Constants.CardContainerType.Loadpile, LoadPileScript.Instance.gameObject, doLog: false, isAction: false);
        }
    }

    private Sprite[] GetSuitSprites()
    {
        // the food sprites start at index 0, classic at 4
        int suitSpritesIndex = PersistentSettings.FoodSuitsEnabled ? 0 : 4;

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
