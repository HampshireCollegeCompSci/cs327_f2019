using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StateLoader : MonoBehaviour
{
    Config config = Config.config;

    public void writeState()
    {

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
        for (int i = 0; i < state.foundation1.Length; i++)
        {

        }

        //set up deck
    }

    public static GameState CreateFromJSON(string path)
    {
        var jsonTextFile = Resources.Load<TextAsset>(path);
        return JsonUtility.FromJson<GameState>(jsonTextFile.ToString());
    }
}
