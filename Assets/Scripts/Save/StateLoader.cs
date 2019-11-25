using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StateLoader : MonoBehaviour
{
    Config config = Config.config;
    GameState gameState;

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
                tempList.Insert(0,token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString());
            }
            gameState.foundations.Add(new StringListWrapper() { stringList = tempList });
        }
        //save reactors
        foreach (GameObject reactor in Config.config.reactors)
        {
            List<string> tempList = new List<string>();
            foreach (GameObject token in reactor.GetComponent<ReactorScript>().cardList)
            {
                tempList.Insert(0, token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString());
            }
            gameState.reactors.Add(new StringListWrapper() { stringList = tempList });
        }
        //save wastepile
        List<string> wastePileList = new List<string>();
        foreach (GameObject token in Config.config.wastePile.GetComponent<WastepileScript>().cardList)
        {
            wastePileList.Insert(0, token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString());
        }
        gameState.wastePile = wastePileList;
        //save deck
        List<string> deckList = new List<string>();
        foreach (GameObject token in Config.config.deck.GetComponent<DeckScript>().cardList)
        {
            deckList.Insert(0, token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString());
        }
        gameState.deck = deckList;
        //save matches
        List<string> matchList = new List<string>();
        foreach (GameObject token in Config.config.matches.GetComponent<MatchedPileScript>().cardList)
        {
            matchList.Insert(0, token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString());
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

        File.WriteAllText("Assets/Resources/GameStates/testState.json", json);
    }

    public void loadState(string path = "Assets/Resources/GameStates/testState.json")
    {
        //load the json into a GameState
        GameState state = CreateFromJSON(path);

        //create unsorted full deck.
        DeckScript.deckScript.InstantiateCards(GameObject.Find("DeckButton"));
        
        //set up simple variables
        config.actions = state.actions;
        config.score = state.score;
        config.difficulty = state.difficulty;
        //set up foundations
        int i = 0;
        foreach (StringListWrapper lw in state.foundations)
        {
            foreach (string s in lw.stringList)
            {
                string[] halves = s.Split('_');
                string suite = halves[0];
                string number = halves[1];
                foreach (GameObject token in GameObject.Find("DeckButton").GetComponent<DeckScript>().cardList)
                {
                    if (token.GetComponent<CardScript>().cardNum.ToString() == number && token.GetComponent<CardScript>().cardSuit == suite)
                    {
                        token.GetComponent<CardScript>().MoveCard(config.foundationList[i]);
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
                string[] halves = s.Split('_');
                string suite = halves[0];
                string number = halves[1];
                foreach (GameObject token in GameObject.Find("DeckButton").GetComponent<DeckScript>().cardList)
                {
                    if (token.GetComponent<CardScript>().cardNum.ToString() == number && token.GetComponent<CardScript>().cardSuit == suite)
                    {
                        token.GetComponent<CardScript>().MoveCard(config.reactors[i]);
                        break;
                    }
                }
            }
            i++;
        }
        //set up wastepile
        foreach (string s in state.wastePile)
        {
            string[] halves = s.Split('_');
            string suite = halves[0];
            string number = halves[1];
            foreach (GameObject token in GameObject.Find("DeckButton").GetComponent<DeckScript>().cardList)
            {
                if (token.GetComponent<CardScript>().cardNum.ToString() == number && token.GetComponent<CardScript>().cardSuit == suite)
                {
                    token.GetComponent<CardScript>().MoveCard(config.wastePile);
                    break;
                }
            }
        }
        //set up matches
        foreach (string s in state.matches)
        {
            string[] halves = s.Split('_');
            string suite = halves[0];
            string number = halves[1];
            foreach (GameObject token in GameObject.Find("DeckButton").GetComponent<DeckScript>().cardList)
            {
                if (token.GetComponent<CardScript>().cardNum.ToString() == number && token.GetComponent<CardScript>().cardSuit == suite)
                {
                    token.GetComponent<CardScript>().MoveCard(config.matches);
                    break;
                }
            }
        }
        //set up deck
        List<GameObject> tempList = GameObject.Find("DeckButton").GetComponent<DeckScript>().cardList;
        foreach (string s in state.deck)
        {
            string[] halves = s.Split('_');
            string suite = halves[0];
            string number = halves[1];
            foreach (GameObject token in tempList)
            {
                if (token.GetComponent<CardScript>().cardNum.ToString() == number && token.GetComponent<CardScript>().cardSuit == suite)
                {
                    token.GetComponent<CardScript>().MoveCard(config.matches);
                    break;
                }
            }
        }
        //set up undo log

    }
    

    public static GameState CreateFromJSON(string path)
    {
        var jsonTextFile = Resources.Load<TextAsset>(path);
        return JsonUtility.FromJson<GameState>(jsonTextFile.ToString());
    }
}
