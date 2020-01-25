using System.Collections;
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

    public void writeState(/*string path*/)
    {
        gameState = new GameState() {
            wastePile = new List<string>(),
            deck = new List<string>(),
            matches = new List<string>(),
            foundations = new List<StringListWrapper>(),
            reactors = new List<StringListWrapper>(),
            moveLog = new List<AltMove>(),
            score = 0,
            actions = 0,
            difficulty = ""
        };
        //save foundations
        foreach (GameObject foundation in Config.config.foundationList)
        {
            List<string> tempList = new List<string>();
            foreach (GameObject token in foundation.GetComponent<FoundationScript>().cardList)
            {
                tempList.Insert(0,token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString() 
                    + "_" + token.GetComponent<CardScript>().isHidden().ToString());
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
                    + "_" + token.GetComponent<CardScript>().isHidden().ToString());
            }
            gameState.reactors.Add(new StringListWrapper() { stringList = tempList });
        }
        //save wastepile
        List<string> wastePileList = new List<string>();
        foreach (GameObject token in Config.config.wastePile.GetComponent<WastepileScript>().cardList)
        {
            wastePileList.Insert(0, token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString()
                + "_" + token.GetComponent<CardScript>().isHidden().ToString());
        }
        gameState.wastePile = wastePileList;
        //save deck
        List<string> deckList = new List<string>();
        foreach (GameObject token in Config.config.deck.GetComponent<DeckScript>().cardList)
        {
            deckList.Insert(0, token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString()
                + "_" + token.GetComponent<CardScript>().isHidden().ToString());
        }
        gameState.deck = deckList;
        //save matches
        List<string> matchList = new List<string>();
        foreach (GameObject token in Config.config.matches.GetComponent<MatchedPileScript>().cardList)
        {
            matchList.Insert(0, token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString()
                + "_" + token.GetComponent<CardScript>().isHidden().ToString());
        }
        gameState.matches = matchList;
        //save undo
        List<AltMove> altMoveLog = new List<AltMove>();
        Stack<AltMove> tempMoveLog = new Stack<AltMove>();
        foreach (Move move in Config.config.moveLog)
        {
            tempMoveLog.Push(new AltMove() {
                cardName = move.card.GetComponent<CardScript>().cardSuit + "_" + move.card.GetComponent<CardScript>().cardNum.ToString(),
                originName = move.origin.name,
                moveType = move.moveType,
                nextCardWasHidden = move.nextCardWasHidden,
                isAction = move.isAction,
                remainingActions = move.remainingActions
            });
        }
        int altMoveLogSize = tempMoveLog.Count;
        for (int i = 0; i < altMoveLogSize; i++)
        {
            altMoveLog.Add(tempMoveLog.Pop());
        }
        gameState.moveLog = altMoveLog;
        //save other data
        gameState.score = Config.config.score;
        gameState.actions = Config.config.actions;
        gameState.difficulty = Config.config.difficulty;

        string json = JsonUtility.ToJson(gameState, true);
        if (Application.isEditor)
        {
            File.WriteAllText("Assets/Resources/GameStates/testState.json", json);
        }
        else
        {
            File.WriteAllText("Cosmia_Data/Resources/testState.json", json);
        }
        //UnityEditor.AssetDatabase.Refresh();
    }

    public void loadState()
    {
        string path;
        if (Application.isEditor)
        {
            path = "GameStates/testState";
        }
        else
        {
            path = "Cosmia_Data/Resources/testState.json";
        }
        //load the json into a GameState
        gameState = CreateFromJSON(path);
    }

    public void tutorialState(string path = "GameStates/tutorialState")
    {
        //load the json into a GameState
        gameState = CreateFromJSON(path);
    }

    public void unpackState(GameState state)
    {
        //create unsorted full deck.
        GameObject LoadPile = Config.config.loadPile;
        LoadPileScript LoadPileList = LoadPile.GetComponent<LoadPileScript>();
        DeckScript.deckScript.InstantiateCards(Config.config.deck);
        while (Config.config.deck.GetComponent<DeckScript>().cardList.Count > 0)
        {
            Config.config.deck.GetComponent<DeckScript>().cardList[0].GetComponent<CardScript>().MoveCard(LoadPile, false, false, false);
        }
        print(state);
        //set up simple variables
        print("actions taken: " + state.actions);
        print("max actions: " + Config.config.actionMax);
        UtilsScript.global.UpdateActionCounter(state.actions, true);
        Config.config.score = state.score;
        Config.config.difficulty = state.difficulty;
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
                        token.GetComponent<CardScript>().MoveCard(Config.config.foundationList[i], false, false, false);
                        if (hiddenState == "True")
                        {
                            token.GetComponent<CardScript>().SetVisibility(false);
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
                            token.GetComponent<CardScript>().SetVisibility(false);
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
                        token.GetComponent<CardScript>().SetVisibility(false);
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
                        token.GetComponent<CardScript>().SetVisibility(false);
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
        foreach (AltMove a in state.moveLog)
        {
            Move tempMove = new Move();
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
            UndoScript.undoScript.moveLog.Push(tempMove);
        }
    }
       
    public static GameState CreateFromJSON(string path)
    {
        if (Application.isEditor)
        {
            var jsonTextFile = Resources.Load<TextAsset>(path);
            return JsonUtility.FromJson<GameState>(jsonTextFile.ToString());
        } else
        {
            var jsonTextFile = File.ReadAllText(path);
            return JsonUtility.FromJson<GameState>(jsonTextFile);
        }
    }
}
