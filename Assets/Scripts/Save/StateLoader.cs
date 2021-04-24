﻿using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StateLoader : MonoBehaviour
{
    public static StateLoader saveSystem;
    public GameState gameState;

    private void Awake()
    {
        if (saveSystem == null)
        {
            DontDestroyOnLoad(gameObject); //makes instance persist across scenes
            saveSystem = this;
        }
        else if (saveSystem != this)
        {
            Destroy(gameObject); //deletes copies of global which do not need to exist, so right version is used to get info from
        }
    }

    public void WriteState(/*string path*/)
    {
        Debug.Log("writing state");

        gameState = new GameState() {
            foundations = new List<StringListWrapper>(),
            reactors = new List<StringListWrapper>()
        };

        //save foundations
        foreach (GameObject foundation in Config.config.foundations)
        {
            gameState.foundations.Add(new StringListWrapper() {
                stringList = ConvertCardListToStringList(foundation.GetComponent<FoundationScript>().cardList)
            });
        }

        //save reactors
        foreach (GameObject reactor in Config.config.reactors)
        {
            gameState.reactors.Add(new StringListWrapper() {
                stringList = ConvertCardListToStringList(reactor.GetComponent<ReactorScript>().cardList)
            });
        }

        //save wastepile
        gameState.wastePile = ConvertCardListToStringList(Config.config.wastePile.GetComponent<WastepileScript>().cardList);

        //save deck
        gameState.deck = ConvertCardListToStringList(Config.config.deck.GetComponent<DeckScript>().cardList);

        //save matches
        gameState.matches = ConvertCardListToStringList(Config.config.matches.GetComponent<MatchedPileScript>().cardList);

        //save undo
        CardScript cardScriptRef;
        List<AltMove> saveMoveLog = new List<AltMove>();
        foreach (Move move in UndoScript.undoScript.moveLog)
        {
            cardScriptRef = move.card.GetComponent<CardScript>();

            saveMoveLog.Add(new AltMove() {
                cardName = $"{cardScriptRef.cardSuit}_{cardScriptRef.cardNum}",
                originName = move.origin.name,
                moveType = move.moveType,
                nextCardWasHidden = move.nextCardWasHidden,
                isAction = move.isAction,
                remainingActions = move.remainingActions,
                score = move.score,
                moveNum = move.moveNum
            });
        }

        gameState.moveLog = saveMoveLog;

        //save other data
        gameState.score = Config.config.score;
        gameState.consecutiveMatches = Config.config.consecutiveMatches;
        gameState.moveCounter = Config.config.moveCounter;
        gameState.actions = Config.config.actions;
        gameState.difficulty = Config.config.difficulty;

        //saving to json
        string json;
        if (Application.isEditor)
        {
            json = JsonUtility.ToJson(gameState, true);
            File.WriteAllText("Assets/Resources/GameStates/testState.json", json);
        }
        else
        {
            json = JsonUtility.ToJson(gameState);
            File.WriteAllText(Application.persistentDataPath + "/testState.json", json);
        }

        //UnityEditor.AssetDatabase.Refresh();
    }

    private static List<string> ConvertCardListToStringList(List<GameObject> cardList)
    {
        List<string> stringList = new List<string>();
        CardScript cardScriptRef;
        
        // go backwards through the list as the top cards are at index 0
        for (int i = cardList.Count - 1; i != -1; i--)
        {
            cardScriptRef = cardList[i].GetComponent<CardScript>();
            stringList.Add($"{cardScriptRef.cardSuit}_{cardScriptRef.cardNum}_{cardScriptRef.IsHidden}");
        }

        return stringList;
    }

    public void LoadState()
    {
        Debug.Log("loading state");

        string path;
        if (Application.isEditor)
        {
            path = "GameStates/testState";
        }
        else
        {
            path = Application.persistentDataPath + "/testState.json";
        }
        //load the json into a GameState
        gameState = CreateFromJSON(path);
    }

    public void LoadTutorialState(string fileName)
    {
        Debug.Log("loading tutorial state");

        //load the json into a GameState
        gameState = CreateFromJSON($"Tutorial/{fileName}", true);
    }

    public void UnpackState(GameState state, bool isTutorial)
    {
        Debug.Log($"unpacking state, isTutorial: {isTutorial}");

        LoadPileScript loadPileScript = Config.config.loadPile.GetComponent<LoadPileScript>();

        // if the tutorial isn't being loaded then we need to make new cards and setup the move log
        if (!isTutorial)
        {
            DeckScript.deckScript.InstantiateCards(addToLoadPile: true);
            SetUpMoveLog(state.moveLog, loadPileScript.cardList);
        }

        // sharing the index variable for the foundations and reactors
        int index;

        //set up foundations
        index = 0;
        foreach (StringListWrapper lw in state.foundations)
        {
            SetUpLocationWithCards(lw.stringList, loadPileScript.cardList, Config.config.foundations[index]);
            index++;
        }

        //set up reactors
        index = 0;
        foreach (StringListWrapper lw in state.reactors)
        {
            SetUpLocationWithCards(lw.stringList, loadPileScript.cardList, Config.config.reactors[index]);
            index++;
        }

        //set up wastepile
        SetUpLocationWithCards(state.wastePile, loadPileScript.cardList, Config.config.wastePile);

        //set up matches
        SetUpLocationWithCards(state.matches, loadPileScript.cardList, Config.config.matches);

        //set up deck
        if (!isTutorial)
        {
            SetUpLocationWithCards(state.deck, loadPileScript.cardList, Config.config.deck);
        }
        else
        {
            // during the tutorial the deck order doesn't matter
            int cardCount = loadPileScript.cardList.Count;
            while (cardCount != 0)
            {
                loadPileScript.cardList[0].GetComponent<CardScript>().MoveCard(Config.config.deck, false, false, false);
                cardCount--;
            }
        }

        //set up simple variables
        Config.config.difficulty = state.difficulty;
        Config.config.score = state.score;
        Config.config.consecutiveMatches = state.consecutiveMatches;
        Config.config.moveCounter = state.moveCounter;
        UtilsScript.global.UpdateActions(state.actions, startingGame: true);
    }

    private static void SetUpMoveLog(List<AltMove> moves, List<GameObject> cardList)
    {
        //set up undo log
        
        // creating a list of card containing objects to reference from later
        List<GameObject> origins = new List<GameObject>()
        {
            Config.config.deck,
            Config.config.wastePile,
            Config.config.matches
        };
        origins.AddRange(Config.config.foundations);
        origins.AddRange(Config.config.reactors);

        // variables that will be used repeatedly when setting up the move log
        Move tempMove;
        string[] segments;
        string suite;
        int number;

        CardScript cardScriptRef;

        // going backwards through all the saved moves and recreate them
        for (int i = moves.Count - 1; i != -1; i--)
        {
            tempMove = new Move();

            // extracting move info relating to the specific card
            segments = moves[i].cardName.Split('_');
            suite = segments[0];
            number = System.Int32.Parse(segments[1]);

            // looking for the card that the move is associated with
            foreach (GameObject card in cardList)
            {
                cardScriptRef = card.GetComponent<CardScript>();
                if (cardScriptRef.cardNum == number && cardScriptRef.cardSuit == suite)
                {
                    tempMove.card = card;
                    break;
                }
            }

            // looking for the move's card containg object
            foreach (GameObject origin in origins)
            {
                if (origin.name == moves[i].originName)
                {
                    tempMove.origin = origin;
                    break;
                }
            }

            // other variables
            tempMove.isAction = moves[i].isAction;
            tempMove.moveType = moves[i].moveType;
            tempMove.nextCardWasHidden = moves[i].nextCardWasHidden;
            tempMove.remainingActions = moves[i].remainingActions;
            tempMove.score = moves[i].score;
            tempMove.moveNum = moves[i].moveNum;

            // making the move official
            UndoScript.undoScript.moveLog.Push(tempMove);
        }
    }

    private static void SetUpLocationWithCards(List<string> stringList, List<GameObject> cardList, GameObject newLocation)
    {
        // seting up the new location with cards using the given commands, and cards

        // variables that will be used repeatedly when setting up the location
        string[] segments;
        string suite;
        int number;
        bool hiddenState;

        CardScript cardScriptRef;

        foreach (string s in stringList)
        {
            // extracting info relating to the specific card
            segments = s.Split('_');
            suite = segments[0];
            number = System.Int32.Parse(segments[1]);
            hiddenState = bool.Parse(segments[2]);

            // looking for the card that needs to be moved
            foreach (GameObject card in cardList)
            {
                cardScriptRef = card.GetComponent<CardScript>();
                if (cardScriptRef.cardNum == number && cardScriptRef.cardSuit == suite)
                {
                    cardScriptRef.MoveCard(newLocation, false, false, false);
                    if (hiddenState)
                    {
                        cardScriptRef.SetFoundationVisibility(false);
                    }

                    break;
                }
            }
        }
    }
       
    public static GameState CreateFromJSON(string path, bool tutorial = false)
    {
        Debug.Log("creating gamestate from path: " + path);

        if (tutorial)
        {
            var jsonTextFile = Resources.Load<TextAsset>(path);
            return JsonUtility.FromJson<GameState>(jsonTextFile.ToString());
        }
        else if (Application.isEditor)
        {
            var jsonTextFile = Resources.Load<TextAsset>(path);
            return JsonUtility.FromJson<GameState>(jsonTextFile.ToString());
        }
        else
        {
            var jsonTextFile = File.ReadAllText(path);
            return JsonUtility.FromJson<GameState>(jsonTextFile);
        }
    }
}
