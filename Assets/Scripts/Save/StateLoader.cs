using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StateLoader : MonoBehaviour
{
    // Singleton instance.
    public static StateLoader Instance;

    private List<SaveMove> saveMoveLog;

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
            t = newMove.containerType,
            i = newMove.containerType switch
            {
                Constants.CardContainerType.Reactor => Array.IndexOf(UtilsScript.Instance.reactors, newMove.origin),
                Constants.CardContainerType.Foundation => Array.IndexOf(UtilsScript.Instance.foundations, newMove.origin),
                _ => 0,
            },
            m = newMove.moveType,
            h = Convert.ToByte(newMove.nextCardWasHidden),
            a = Convert.ToByte(newMove.isAction),
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

        GameState<int> gameState = new();

        //save foundations
        for (int i = 0; i < UtilsScript.Instance.foundationScripts.Length; i++)
        {
            foreach (GameObject card in UtilsScript.Instance.foundationScripts[i].CardList)
            {
                CardScript cardScript = card.GetComponent<CardScript>();
                if (cardScript.Hidden)
                {
                    gameState.foundations[i].hidden.Add(cardScript.CardID);
                }
                else
                {
                    gameState.foundations[i].unhidden.Add(cardScript.CardID);
                }
            }
        }

        //save reactors
        for (int i = 0; i < UtilsScript.Instance.reactorScripts.Length; i++)
        {
            gameState.reactors[i].cards = ConvertCardListToStringList(UtilsScript.Instance.reactorScripts[i].CardList);
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
        gameState.difficulty = Config.Instance.CurrentDifficulty.Name;

        //saving to json, when in editor save it in human readable format
        File.WriteAllText(SaveFile.GetPath(), JsonUtility.ToJson(gameState, Application.isEditor));

        //UnityEditor.AssetDatabase.Refresh();
    }

    public void LoadSaveState()
    {
        Debug.Log("loading save state");

        // load the save file from the save path and unpack it
        string jsonTextFile = File.ReadAllText(SaveFile.GetPath());
        GameState<int> saveState = JsonUtility.FromJson<GameState<int>>(jsonTextFile);
        UnpackGameState(saveState);
    }

    public void LoadTutorialState(string fileName)
    {
        Debug.Log($"loading tutorial state: {fileName}");
        string filePath = Constants.Tutorial.tutorialResourcePath + fileName;

        // load the asset from resources and unpack it
        string jsonTextFile = Resources.Load<TextAsset>(filePath).ToString();
        GameState<string> tutorialState = JsonUtility.FromJson<GameState<string>>(jsonTextFile);
        UnpackGameState(tutorialState, isTutorial: true);
    }

    private void UnpackGameState<T>(GameState<T> state, bool isTutorial = false)
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
            throw new NullReferenceException("there are no cards in the load pile when starting to load the game");
        }

        if (!isTutorial)
        {
            SetUpMoveLog(state.moveLog, LoadPileScript.Instance.CardList);
        }

        //set up foundations
        for (int i = 0; i < state.foundations.Length; i++)
        {
            SetUpLocationWithCards(state.foundations[i].hidden, Constants.CardContainerType.Foundation, UtilsScript.Instance.foundations[i], isHidden: true);
            SetUpLocationWithCards(state.foundations[i].unhidden, Constants.CardContainerType.Foundation, UtilsScript.Instance.foundations[i]);
        }

        //set up reactors
        for (int i = 0; i < state.reactors.Length; i++)
        {
            SetUpLocationWithCards(state.reactors[i].cards, Constants.CardContainerType.Reactor, UtilsScript.Instance.reactors[i]);
        }

        //set up wastepile
        SetUpLocationWithCards(state.wastePile, Constants.CardContainerType.WastePile, WastepileScript.Instance.gameObject);

        //set up matches
        SetUpLocationWithCards(state.matches, Constants.CardContainerType.MatchedPile, MatchedPileScript.Instance.gameObject);

        //set up deck
        if (isTutorial)
        {
            // during the tutorial the deck order doesn't matter
            int cardCount = LoadPileScript.Instance.CardList.Count;
            while (cardCount != 0)
            {
                // move from top down for efficiency, LoadPileScript.Remove() takes advantage of this
                LoadPileScript.Instance.CardList[^1].GetComponent<CardScript>().MoveCard(Constants.CardContainerType.Deck, DeckScript.Instance.gameObject, false, false, false);
                cardCount--;
            }
        }
        else
        {
            SetUpLocationWithCards(state.deck, Constants.CardContainerType.Deck, DeckScript.Instance.gameObject);
        }

        // if the game state has missing cards they will be leftover in the load pile cardlist
        if (LoadPileScript.Instance.CardList.Count != 0)
        {
            Debug.LogError("there are cards still in the load pile after loading the game");
            throw new NullReferenceException("there are cards still in the load pile after loading the game");
        }

        foreach (ReactorScript reactorScript in UtilsScript.Instance.reactorScripts)
        {
            reactorScript.SetReactorScore();
        }

        UtilsScript.Instance.UpdateActions(state.actions, startingGame: true);
        DeckScript.Instance.UpdateDeckCounter();
    }

    private List<int> ConvertCardListToStringList(List<GameObject> cardList)
    {
        List<int> newCardList = new(cardList.Count);
        foreach (GameObject card in cardList)
        {
            newCardList.Add(card.GetComponent<CardScript>().CardID);
        }
        return newCardList;
    }

    private void SetUpMoveLog(List<SaveMove> moves, List<GameObject> cardList)
    {
        saveMoveLog = moves;

        Stack<Move> newMoveLog = new();

        // going through all the saved moves and recreate them
        foreach (SaveMove saveMove in moves)
        {
            Move newMove = new()
            {
                card = cardList[saveMove.c - 1],
                containerType = saveMove.t,
                origin = saveMove.t switch
                {
                    Constants.CardContainerType.Reactor => UtilsScript.Instance.reactors[saveMove.i],
                    Constants.CardContainerType.Foundation => UtilsScript.Instance.foundations[saveMove.i],
                    Constants.CardContainerType.Deck => DeckScript.Instance.gameObject,
                    Constants.CardContainerType.WastePile => WastepileScript.Instance.gameObject,
                    Constants.CardContainerType.MatchedPile => MatchedPileScript.Instance.gameObject,
                    _ => throw new System.ArgumentException($"{saveMove.t} is not a valid saved origin")
                },
                moveType = saveMove.m,
                nextCardWasHidden = System.Convert.ToBoolean(saveMove.h),
                isAction = System.Convert.ToBoolean(saveMove.a),
                remainingActions = saveMove.r,
                score = saveMove.s,
                moveNum = saveMove.n
            };

            newMoveLog.Push(newMove);
        }

        UndoScript.Instance.SetMoveLog(newMoveLog);
    }

    private void SetUpLocationWithCards<T>(List<T> cardList, Constants.CardContainerType newContainer, GameObject newLocation, bool isHidden = false)
    {
        foreach (T cardID in cardList)
        {
            GameObject card = cardID switch
            {
                int ID => LoadPileScript.Instance.CardList.Find(card => card.GetComponent<CardScript>().CardID == ID),
                string name => LoadPileScript.Instance.CardList.Find(card => card.GetComponent<CardScript>().name == name),
                _ => null
            };

            // int ID => LoadPileScript.Instance.CardList.BinarySearch(ID, Comparer<GameObject>.Create((x, y) =>
            // x.GetComponent<CardScript>().CardID.CompareTo(y.GetComponent<CardScript>().CardID))),

            if (card == null)
            {
                throw new KeyNotFoundException($"the card \"{cardID}\" was not found");
            }

            CardScript cardScript = card.GetComponent<CardScript>();
            cardScript.MoveCard(newContainer, newLocation, doLog: false, isAction: false);
            if (isHidden)
            {
                cardScript.Hidden = true;
            }
        }
    }
}
