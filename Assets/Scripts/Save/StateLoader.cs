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
            moveLog = new Stack<AltMove>(),
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
                tempList.Add(token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString());
            }
            gameState.foundations.Add(new StringListWrapper() { stringList = tempList });
        }
        //save reactors
        foreach (GameObject reactor in Config.config.reactors)
        {
            List<string> tempList = new List<string>();
            foreach (GameObject token in reactor.GetComponent<ReactorScript>().cardList)
            {
                tempList.Add(token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString());
            }
            gameState.reactors.Add(new StringListWrapper() { stringList = tempList });
        }
        //save wastepile
        List<string> wastePileList = new List<string>();
        foreach (GameObject token in Config.config.wastePile.GetComponent<WastepileScript>().cardList)
        {
            wastePileList.Add(token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString());
        }
        gameState.wastePile = wastePileList;
        //save deck
        List<string> deckList = new List<string>();
        foreach (GameObject token in Config.config.deck.GetComponent<DeckScript>().cardList)
        {
            deckList.Add(token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString());
        }
        gameState.deck = deckList;
        //save matches
        List<string> matchList = new List<string>();
        foreach (GameObject token in Config.config.matches.GetComponent<MatchedPileScript>().cardList)
        {
            matchList.Add(token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString());
        }
        gameState.matches = matchList;
        //save undo
        Stack<AltMove> altMoveLog = new Stack<AltMove>();
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
            altMoveLog.Push(tempMoveLog.Pop());
        }
        gameState.moveLog = altMoveLog;
        //save other data
        gameState.score = Config.config.score;
        gameState.actions = Config.config.actions;
        gameState.difficulty = Config.config.difficulty;

        string json = JsonUtility.ToJson(gameState, true);

        File.WriteAllText("Assets/Resources/GameStates/testState.json", json);
    }

    public void loadState(string path)
    {
        //load the json into a GameState
        GameState state = CreateFromJSON(path);

        //create unsorted full deck.
        DeckScript.deckScript.InstantiateCards(GameObject.Find("DeckButton"));
        
        //set up simple variables
        config.actions = state.actions;
        config.score = state.score;

        //set up reactors
        /*
        for (int i = 0; i < state.foundation1.Length; i++)
        {

        }
        */
        //set up deck
    }

    public static GameState CreateFromJSON(string path)
    {
        var jsonTextFile = Resources.Load<TextAsset>(path);
        return JsonUtility.FromJson<GameState>(jsonTextFile.ToString());
    }
}
