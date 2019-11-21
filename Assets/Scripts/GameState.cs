using UnityEngine;
using System.IO;
[System.Serializable]

public class GameState
{
    public string[] wastePile;
    public string[] deck;
    public string[] matches;
    public string[] foundation1;
    public string[] foundation2;
    public string[] foundation3;
    public string[] foundation4;
    public string[] reactor1;
    public string[] reactor2;
    public string[] reactor3;
    public string[] reactor4;
    public int score;
    public int actions;
    public int difficulty; // 1 = Easy, 2 = Medium, 3 = Hard
}
