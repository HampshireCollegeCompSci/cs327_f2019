using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
[System.Serializable]

public class GameState
{
    public string[] wastePile;
    public string[] deck;
    public string[] matches;
    public string[][] foundations;
    public string[][] reactors;
    public Stack<Move> moveLog;
    public int score;
    public int actions;
    public string difficulty;
}
