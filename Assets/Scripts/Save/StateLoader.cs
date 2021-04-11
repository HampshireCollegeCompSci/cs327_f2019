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

        //create unsorted full deck.
        GameObject LoadPile = Config.config.loadPile;
        LoadPileScript LoadPileList = LoadPile.GetComponent<LoadPileScript>();
        if (!isTutorial)
        {
            DeckScript.deckScript.InstantiateCards(Config.config.deck);
            while (Config.config.deck.GetComponent<DeckScript>().cardList.Count > 0)
            {
                Config.config.deck.GetComponent<DeckScript>().cardList[0].GetComponent<CardScript>().MoveCard(LoadPile, false, false, false);
            }
        }

        //set up foundations
        int i = 0;
        foreach (StringListWrapper lw in state.foundations)
        {
            foreach (string s in lw.stringList)
            {
                string[] segments = s.Split('_');
                string suite = segments[0];
                string number = segments[1];
                string hiddenState = segments[2];
                foreach (GameObject token in LoadPileList.cardList)
                {
                    if (token.GetComponent<CardScript>().cardNum.ToString() == number && token.GetComponent<CardScript>().cardSuit == suite)
                    {
                        token.GetComponent<CardScript>().MoveCard(Config.config.foundations[i], false, false, false);
                        if (hiddenState == "True")
                        {
                            token.GetComponent<CardScript>().SetFoundationVisibility(false);
                        }
                        break;
                    }
                }
            }
            i++;
        }

        //set up reactors
        i = 0;
        foreach (StringListWrapper lw in state.reactors)
        {
            foreach (string s in lw.stringList)
            {
                string[] segments = s.Split('_');
                string suite = segments[0];
                string number = segments[1];
                string hiddenState = segments[2];
                foreach (GameObject token in LoadPileList.cardList)
                {
                    if (token.GetComponent<CardScript>().cardNum.ToString() == number && token.GetComponent<CardScript>().cardSuit == suite)
                    {
                        token.GetComponent<CardScript>().MoveCard(Config.config.reactors[i], false, false, false);
                        if (hiddenState == "True")
                        {
                            token.GetComponent<CardScript>().SetFoundationVisibility(false);
                        }
                        break;
                    }
                }
            }
            i++;
        }

        //set up wastepile
        foreach (string s in state.wastePile)
        {
            string[] segments = s.Split('_');
            string suite = segments[0];
            string number = segments[1];
            string hiddenState = segments[2];
            foreach (GameObject token in LoadPileList.cardList)
            {
                if (token.GetComponent<CardScript>().cardNum.ToString() == number && token.GetComponent<CardScript>().cardSuit == suite)
                {
                    token.GetComponent<CardScript>().MoveCard(Config.config.wastePile, false, false, false);
                    if (hiddenState == "True")
                    {
                        token.GetComponent<CardScript>().SetFoundationVisibility(false);
                    }
                    break;
                }
            }
        }

        //set up matches
        foreach (string s in state.matches)
        {
            string[] segments = s.Split('_');
            string suite = segments[0];
            string number = segments[1];
            string hiddenState = segments[2];
            foreach (GameObject token in LoadPileList.cardList)
            {
                if (token.GetComponent<CardScript>().cardNum.ToString() == number && token.GetComponent<CardScript>().cardSuit == suite)
                {
                    token.GetComponent<CardScript>().MoveCard(Config.config.matches, false, false, false);
                    if (hiddenState == "True")
                    {
                        token.GetComponent<CardScript>().SetFoundationVisibility(false);
                    }
                    break;
                }
            }
        }

        //set up deck
        foreach (string s in state.deck)
        {
            string[] segments = s.Split('_');
            string suite = segments[0];
            string number = segments[1];
            string hiddenState = segments[2];

            foreach (GameObject token in LoadPileList.cardList)
            {
                if (token.GetComponent<CardScript>().cardNum.ToString() == number && token.GetComponent<CardScript>().cardSuit == suite)
                {
                    token.GetComponent<CardScript>().MoveCard(Config.config.deck, false, false, false);
                    break;
                }
            }
        }

        //set up undo log
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");
        Move tempMove;
        foreach (AltMove a in state.moveLog)
        {
            tempMove = new Move();
            string[] segments = a.cardName.Split('_');
            string suite = segments[0];
            string number = segments[1];
            //string hiddenState = segments[2];
            foreach (GameObject card in cards)
            {
                if (card.GetComponent<CardScript>().cardSuit == suite && card.GetComponent<CardScript>().cardNum.ToString() == number)
                {
                    tempMove.card = card;
                }
            }
            tempMove.origin = GameObject.Find(a.originName);
            tempMove.isAction = a.isAction;
            tempMove.moveType = a.moveType;
            tempMove.nextCardWasHidden = a.nextCardWasHidden;
            tempMove.remainingActions = a.remainingActions;
            tempMove.score = a.score;
            tempMove.moveNum = a.moveNum;
            UndoScript.undoScript.moveLog.Push(tempMove);
        }

        //set up simple variables
        //print("actions taken: " + state.actions);
        //print("max actions: " + Config.config.actionMax);
        Config.config.difficulty = state.difficulty;
        Config.config.score = state.score;
        Config.config.consecutiveMatches = state.consecutiveMatches;
        Config.config.moveCounter = state.moveCounter;
        UtilsScript.global.UpdateActions(state.actions, startingGame: true);
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
