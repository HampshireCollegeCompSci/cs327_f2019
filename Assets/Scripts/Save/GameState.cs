using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
[System.Serializable]
public class GameState
{
    public List<string> wastePile;
    public List<string> deck;
    public List<string> matches;
    public List<StringListWrapper> foundations;
    public List<StringListWrapper> reactors;
    public Stack<AltMove> moveLog;
    public int score;
    public int actions;
    public string difficulty;
}
