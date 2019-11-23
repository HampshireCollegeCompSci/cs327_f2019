using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StateLoader : MonoBehaviour
{
    Config config = Config.config;

    public void writeState(/*string path*/)
    {
        GameState gameState = new GameState();
        //save foundations
        int i = 0;
        int x = 0;
        foreach (GameObject foundation in Config.config.foundationList)
        {
            string[] tempArray = new string[foundation.GetComponent<FoundationScript>().cardList.Count];
            foreach (GameObject token in foundation.GetComponent<FoundationScript>().cardList)
            {
                tempArray.SetValue(token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString(), x);
                x++;
            }
            print(tempArray);
            gameState.foundations[i].SetValue(tempArray, i);
            i++;
        }
        //save reactors
        i = 0;
        x = 0;
        foreach (GameObject reactor in Config.config.reactors)
        {
            string[] tempArray = new string[reactor.GetComponent<ReactorScript>().cardList.Count];
            foreach (GameObject token in reactor.GetComponent<ReactorScript>().cardList)
            {
                tempArray.SetValue(token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString(), x);
                x++;
            }
            gameState.reactors[i].SetValue(tempArray, i);
            i++;
        }
        //save wastepile
        i = 0;
        string[] wastePileArray = new string[Config.config.wastePile.GetComponent<WastepileScript>().cardList.Count];
        foreach (GameObject token in Config.config.wastePile.GetComponent<WastepileScript>().cardList)
        {
            wastePileArray.SetValue(token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString(), i);
            i++;
        }
        gameState.wastePile = wastePileArray;
        //save deck
        i = 0;
        string[] deckArray = new string[Config.config.deck.GetComponent<DeckScript>().cardList.Count];
        foreach (GameObject token in Config.config.deck.GetComponent<DeckScript>().cardList)
        {
            deckArray.SetValue(token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString(), i);
            i++;
        }
        gameState.deck = deckArray;
        //save matches
        i = 0;
        string[] matchArray = new string[Config.config.matches.GetComponent<MatchedPileScript>().cardList.Count];
        foreach (GameObject token in Config.config.matches.GetComponent<MatchedPileScript>().cardList)
        {
            matchArray.SetValue(token.GetComponent<CardScript>().cardSuit + "_" + token.GetComponent<CardScript>().cardNum.ToString(), i);
            i++;
        }
        gameState.matches = matchArray;
        //save undo
        gameState.moveLog = UndoScript.undoScript.moveLog;
        //save other data
        gameState.score = Config.config.score;
        gameState.actions = Config.config.actions;
        gameState.difficulty = Config.config.difficulty;

        string json = JsonUtility.ToJson(gameState);
        print(json);

        File.WriteAllText("GameStates/testState.json", json);
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
