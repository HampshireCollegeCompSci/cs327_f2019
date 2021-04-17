using System.Collections.Generic;
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
            wastePile = new List<string>(),
            deck = new List<string>(),
            matches = new List<string>(),
            foundations = new List<StringListWrapper>(),
            reactors = new List<StringListWrapper>(),
            moveLog = new List<AltMove>(),
            score = 0,
            consecutiveMatches = 0,
            moveCounter = 0,
            actions = 0,
            difficulty = ""
        };

        //save foundations
        foreach (GameObject foundation in Config.config.foundations)
        {
            List<string> tempList = new List<string>();
            foreach (GameObject token in foundation.GetComponent<FoundationScript>().cardList)
            {
                tempList.Insert(0,token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString() 
                    + "_" + token.GetComponent<CardScript>().IsHidden.ToString());
            }
            gameState.foundations.Add(new StringListWrapper() { stringList = tempList });
        }

        //save reactors
        foreach (GameObject reactor in Config.config.reactors)
        {
            List<string> tempList = new List<string>();
            foreach (GameObject token in reactor.GetComponent<ReactorScript>().cardList)
            {
                tempList.Insert(0, token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString()
                    + "_" + token.GetComponent<CardScript>().IsHidden.ToString());
            }
            gameState.reactors.Add(new StringListWrapper() { stringList = tempList });
        }

        //save wastepile
        List<string> wastePileList = new List<string>();
        foreach (GameObject token in Config.config.wastePile.GetComponent<WastepileScript>().cardList)
        {
            wastePileList.Insert(0, token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString()
                + "_" + token.GetComponent<CardScript>().IsHidden.ToString());
        }
        gameState.wastePile = wastePileList;

        //save deck
        List<string> deckList = new List<string>();
        foreach (GameObject token in Config.config.deck.GetComponent<DeckScript>().cardList)
        {
            deckList.Insert(0, token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString()
                + "_" + token.GetComponent<CardScript>().IsHidden.ToString());
        }
        gameState.deck = deckList;

        //save matches
        List<string> matchList = new List<string>();
        foreach (GameObject token in Config.config.matches.GetComponent<MatchedPileScript>().cardList)
        {
            matchList.Insert(0, token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString()
                + "_" + token.GetComponent<CardScript>().IsHidden.ToString());
        }
        gameState.matches = matchList;

        //save undo
        Stack<AltMove> tempMoveLog = new Stack<AltMove>();
        foreach (Move move in UndoScript.undoScript.moveLog)
        {
            /*print(move.card.GetComponent<CardScript>().cardSuit + "_" + move.card.GetComponent<CardScript>().cardNum.ToString());
            print(move.origin.name);
            print(move.moveType);
            print(move.nextCardWasHidden);
            print(move.isAction);
            print(move.remainingActions);*/
            tempMoveLog.Push(new AltMove() {
                cardName = move.card.GetComponent<CardScript>().cardSuit + "_" + move.card.GetComponent<CardScript>().cardNum.ToString(),
                originName = move.origin.name,
                moveType = move.moveType,
                nextCardWasHidden = move.nextCardWasHidden,
                isAction = move.isAction,
                remainingActions = move.remainingActions,
                score = move.score,
                moveNum = move.moveNum
            });
        }

        List<AltMove> altMoveLog = new List<AltMove>();
        int altMoveLogSize = tempMoveLog.Count;
        for (int i = 0; i < altMoveLogSize; i++)
        {
            altMoveLog.Add(tempMoveLog.Pop());
        }
        gameState.moveLog = altMoveLog;

        //save other data
        gameState.score = Config.config.score;
        gameState.consecutiveMatches = Config.config.consecutiveMatches;
        gameState.moveCounter = Config.config.moveCounter;
        gameState.actions = Config.config.actions;
        gameState.difficulty = Config.config.difficulty;

        //saving to json
        string json = JsonUtility.ToJson(gameState, true);
        if (Application.isEditor)
        {
            File.WriteAllText("Assets/Resources/GameStates/testState.json", json);
        }
        else
        {
            File.WriteAllText(Application.persistentDataPath + "/testState.json", json);
        }

        //UnityEditor.AssetDatabase.Refresh();
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

    public void LoadTutorialState(string path)
    {
        Debug.Log("loading tutorial state");

        //load the json into a GameState
        gameState = CreateFromJSON(path, true);
    }

    public void UnpackState(GameState state, bool isTutorial)
    {
        Debug.Log("unpacking state");

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

        // going through all the saved moves and recreating them
        foreach (AltMove move in moves)
        {
            tempMove = new Move();

            // extracting move info relating to the specific card
            segments = move.cardName.Split('_');
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
                if (origin.name == move.originName)
                {
                    tempMove.origin = origin;
                    break;
                }
            }

            // other variables
            tempMove.isAction = move.isAction;
            tempMove.moveType = move.moveType;
            tempMove.nextCardWasHidden = move.nextCardWasHidden;
            tempMove.remainingActions = move.remainingActions;
            tempMove.score = move.score;
            tempMove.moveNum = move.moveNum;

            // making the move official
            UndoScript.undoScript.moveLog.Push(tempMove);
        }
    }

    private static void SetUpLocationWithCards(List<string> tokenStrings, List<GameObject> cardList, GameObject newLocation)
    {
        // seting up the new location with cards using the given commands, and cards

        // variables that will be used repeatedly when setting up the location
        string[] segments;
        string suite;
        int number;
        bool hiddenState;

        CardScript cardScriptRef;

        foreach (string s in tokenStrings)
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
