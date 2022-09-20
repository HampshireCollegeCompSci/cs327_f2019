using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StateLoader : MonoBehaviour
{
    // Singleton instance.
    public static StateLoader Instance;

    private List<SaveMove> saveMoveLog;
    private List<GameObject> origins;
    private int originsLength;

    // Initialize the singleton instance.
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            throw new System.Exception("two of these scripts should not exist at the same time");
        }
    }

    private void Start()
    {
        saveMoveLog = new();
        origins = new()
        {
            DeckScript.Instance.gameObject,
            WastepileScript.Instance.gameObject,
            MatchedPileScript.Instance.gameObject
        };
        origins.AddRange(UtilsScript.Instance.foundations);
        origins.AddRange(UtilsScript.Instance.reactors);

        originsLength = origins.Count;
    }

    public void ClearSaveMoveLog()
    {
        saveMoveLog.Clear();
    }

    public void AddMove(Move newMove)
    {
        SaveMove newSaveMove = new()
        {
            c = newMove.card.GetComponent<CardScript>().CardID,
            o = GetOriginIndex(newMove.origin),
            m = newMove.moveType,
            h = System.Convert.ToByte(newMove.nextCardWasHidden),
            a = System.Convert.ToByte(newMove.isAction),
            r = newMove.remainingActions,
            s = newMove.score,
            n = newMove.moveNum
        };

        saveMoveLog.Add(newSaveMove);
    }

    public void RemoveMove()
    {
        saveMoveLog.RemoveAt(saveMoveLog.Count - 1);
    }

    public void WriteState(/*string path*/)
    {
        if (Config.Instance.tutorialOn) return;

        Debug.Log("writing state");

        GameState gameState = new()
        {
            foundations = new(),
            reactors = new()
        };

        //save foundations
        foreach (FoundationScript foundationScript in UtilsScript.Instance.foundationScripts)
        {
            FoundationCards foundationCards = new()
            {
                hidden = new(),
                unhidden = new()
            };

            CardScript cardScript;
            for (int i = foundationScript.CardList.Count - 1; i >= 0; i--)
            {
                cardScript = foundationScript.CardList[i].GetComponent<CardScript>();
                if (cardScript.Hidden)
                {
                    foundationCards.hidden.Add(cardScript.CardID);
                }
                else
                {
                    foundationCards.unhidden.Add(cardScript.CardID);
                }
            }

            gameState.foundations.Add(foundationCards);
        }

        //save reactors
        foreach (ReactorScript reactorScript in UtilsScript.Instance.reactorScripts)
        {
            ReactorCards cardList = new()
            {
                cards = ConvertCardListToStringList(reactorScript.CardList)
            };
            gameState.reactors.Add(cardList);
        }

        //save wastepile
        gameState.wastePile = ConvertCardListToStringList(WastepileScript.Instance.CardList);

        //save deck
        gameState.deck = ConvertCardListToStringList(DeckScript.Instance.CardList);

        //save matches
        gameState.matches = ConvertCardListToStringList(MatchedPileScript.Instance.CardList);

        //save undo
        gameState.moveLog = saveMoveLog;

        //save other data
        gameState.score = Config.Instance.score;
        gameState.consecutiveMatches = Config.Instance.consecutiveMatches;
        gameState.moveCounter = Config.Instance.moveCounter;
        gameState.actions = Config.Instance.actions;
        gameState.difficulty = Config.Instance.currentDifficulty;

        //saving to json, when in editor save it in human readable format
        File.WriteAllText(SaveState.GetFilePath(), JsonUtility.ToJson(gameState, Constants.inEditor));

        //UnityEditor.AssetDatabase.Refresh();
    }

    public void LoadSaveState()
    {
        Debug.Log("loading save state");

        // load the save file from the save path and unpack it
        string jsonTextFile = File.ReadAllText(SaveState.GetFilePath());
        GameState gameState = JsonUtility.FromJson<GameState>(jsonTextFile);
        UnpackGameState(gameState);
    }

    public void LoadTutorialState(string fileName)
    {
        Debug.Log($"loading tutorial state: {fileName}");
        string filePath = Constants.tutorialResourcePath + fileName;

        // load the asset from resources and unpack it
        string jsonTextFile = Resources.Load<TextAsset>(filePath).ToString();
        TutorialState tutorialState = JsonUtility.FromJson<TutorialState>(jsonTextFile);
        UnpackTutorialState(tutorialState);
    }

    private int GetOriginIndex(GameObject origin)
    {
        for (int originIndex = 0; originIndex < originsLength; originIndex++)
        {
            if (origins[originIndex] == origin)
            {
                return originIndex;
            }
        }

        throw new System.Exception($"the origin \"{origin.name}\" was not found");
    }

    private void UnpackGameState(GameState state)
    {
        Debug.Log($"unpacking state");

        //set up simple variables
        Config.Instance.SetDifficulty(state.difficulty);
        ScoreScript.Instance.SetScore(state.score);
        Config.Instance.consecutiveMatches = state.consecutiveMatches;
        Config.Instance.moveCounter = state.moveCounter;
        // more is done at the end

        // if the tutorial isn't being loaded then we need to setup the move log
        if (LoadPileScript.Instance.CardList.Count == 0)
        {
            Debug.LogError("there are no cards in the load pile when starting to load the game");
            throw new System.Exception("there are no cards in the load pile when starting to load the game");
        }

        SetUpMoveLog(state.moveLog, LoadPileScript.Instance.CardList);

        // sharing the index variable for the foundations and reactors
        int index;

        //set up foundations
        index = 0;
        foreach (FoundationCards foundationCards in state.foundations)
        {
            SetUpLocationWithCards(foundationCards.hidden, UtilsScript.Instance.foundations[index], isHidden: true);
            SetUpLocationWithCards(foundationCards.unhidden, UtilsScript.Instance.foundations[index]);
            index++;
        }

        //set up reactors
        index = 0;
        foreach (ReactorCards cardList in state.reactors)
        {
            SetUpLocationWithCards(cardList.cards, UtilsScript.Instance.reactors[index]);
            index++;
        }

        //set up wastepile
        SetUpLocationWithCards(state.wastePile, WastepileScript.Instance.gameObject);

        //set up matches
        SetUpLocationWithCards(state.matches, MatchedPileScript.Instance.gameObject);

        //set up deck
        SetUpLocationWithCards(state.deck, DeckScript.Instance.gameObject);

        // if the game state has missing cards they will be leftover in the load pile cardlist
        if (LoadPileScript.Instance.CardList.Count != 0)
        {
            Debug.LogError("there are cards still in the load pile after loading the game");
            throw new System.Exception("there are cards still in the load pile after loading the game");
        }

        foreach (ReactorScript reactorScript in UtilsScript.Instance.reactorScripts)
        {
            reactorScript.SetReactorScore();
        }

        UtilsScript.Instance.UpdateActions(state.actions, startingGame: true);
        DeckScript.Instance.UpdateDeckCounter();
    }

    private void UnpackTutorialState(TutorialState state)
    {
        Debug.Log($"unpacking tutorial state");

        //set up simple variables
        Config.Instance.SetDifficulty(state.difficulty);
        ScoreScript.Instance.SetScore(state.score);
        Config.Instance.consecutiveMatches = state.consecutiveMatches;
        Config.Instance.moveCounter = state.moveCounter;
        // more is done at the end

        // if the tutorial isn't being loaded then we need to setup the move log
        if (LoadPileScript.Instance.CardList.Count == 0)
        {
            throw new System.Exception("there are no cards in the load pile");
        }

        // sharing the index variable for the foundations and reactors
        int index;

        //set up foundations
        index = 0;
        foreach (TutorialFoundationCards foundationCards in state.foundations)
        {
            SetUpLocationWithCards(foundationCards.hidden, UtilsScript.Instance.foundations[index], isHidden: true);
            SetUpLocationWithCards(foundationCards.unhidden, UtilsScript.Instance.foundations[index]);
            index++;
        }

        //set up reactors
        index = 0;
        foreach (TutorialReactorCards cardList in state.reactors)
        {
            SetUpLocationWithCards(cardList.cards, UtilsScript.Instance.reactors[index]);
            index++;
        }

        //set up wastepile
        SetUpLocationWithCards(state.wastePile, WastepileScript.Instance.gameObject);

        //set up matches
        SetUpLocationWithCards(state.matches, MatchedPileScript.Instance.gameObject);

        // during the tutorial the deck order doesn't matter
        int cardCount = LoadPileScript.Instance.CardList.Count;
        while (cardCount != 0)
        {
            LoadPileScript.Instance.CardList[0].GetComponent<CardScript>().MoveCard(DeckScript.Instance.gameObject, false, false, false);
            cardCount--;
        }

        foreach (ReactorScript reactorScript in UtilsScript.Instance.reactorScripts)
        {
            reactorScript.SetReactorScore();
        }

        UtilsScript.Instance.UpdateActions(state.actions, startingGame: true);
        DeckScript.Instance.UpdateDeckCounter();
    }

    private List<byte> ConvertCardListToStringList(List<GameObject> cardList)
    {
        List<byte> newCardList = new();
        // go backwards through the list as the top cards are at index 0
        for (int i = cardList.Count - 1; i >= 0; i--)
        {
            newCardList.Add(cardList[i].GetComponent<CardScript>().CardID);
        }

        return newCardList;
    }

    private void SetUpMoveLog(List<SaveMove> moves, List<GameObject> cardList)
    {
        saveMoveLog = moves;

        Stack<Move> newMoveLog = new();

        // going backwards through all the saved moves and recreate them
        foreach (SaveMove saveMove in moves)
        {
            Move newMove = new();

            // looking for the card that the move is associated with
            foreach (GameObject card in cardList)
            {
                if (card.GetComponent<CardScript>().CardID == saveMove.c)
                {
                    newMove.card = card;
                    break;
                }
            }

            newMove.origin = origins[saveMove.o];

            if (newMove.card == null)
            {
                throw new System.NullReferenceException($"card \"{saveMove.c}\" was not found");
            }
            if (newMove.origin == null)
            {
                throw new System.NullReferenceException($"origin \"{saveMove.o}\" was not found");
            }

            // other variables
            newMove.moveType = saveMove.m;
            newMove.nextCardWasHidden = System.Convert.ToBoolean(saveMove.h);
            newMove.isAction = System.Convert.ToBoolean(saveMove.a);
            newMove.remainingActions = saveMove.r;
            newMove.score = saveMove.s;
            newMove.moveNum = saveMove.n;

            newMoveLog.Push(newMove);
        }

        UndoScript.Instance.SetMoveLog(newMoveLog);
    }

    private void SetUpLocationWithCards(List<byte> cardList, GameObject newLocation, bool isHidden = false)
    {
        // seting up the new location with cards using the given commands, and cards
        bool foundCard;
        CardScript cardScriptRef;

        foreach (byte cardID in cardList)
        {
            // looking for the card that needs to be moved
            foundCard = false;
            foreach (GameObject card in LoadPileScript.Instance.CardList)
            {
                cardScriptRef = card.GetComponent<CardScript>();
                if (cardScriptRef.CardID == cardID)
                {
                    cardScriptRef.MoveCard(newLocation, doLog: false, isAction: false);
                    if (isHidden)
                    {
                        cardScriptRef.Hidden = true;
                    }

                    foundCard = true;
                    break;
                }
            }

            if (!foundCard)
            {
                throw new System.NullReferenceException($"the card \"{cardID}\" was not found");
            }
        }
    }

    private void SetUpLocationWithCards(List<string> cardList, GameObject newLocation, bool isHidden = false)
    {
        // seting up the new location with cards using the given commands, and cards
        bool foundCard;
        CardScript cardScriptRef;

        foreach (string cardName in cardList)
        {
            // looking for the card that needs to be moved
            foundCard = false;
            foreach (GameObject card in LoadPileScript.Instance.CardList)
            {
                if (card.name == cardName)
                {
                    cardScriptRef = card.GetComponent<CardScript>();

                    cardScriptRef.MoveCard(newLocation, doLog: false, isAction: false);
                    if (isHidden)
                    {
                        cardScriptRef.Hidden = true;
                    }

                    foundCard = true;
                    break;
                }
            }

            if (!foundCard)
            {
                throw new System.NullReferenceException($"the card \"{cardName}\" was not found");
            }
        }
    }
}
