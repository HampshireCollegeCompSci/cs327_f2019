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
    public int score;
    public int actions;
    public int difficulty; // 1 = Easy, 2 = Medium, 3 = Hard
}
