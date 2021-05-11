using System.Collections.Generic;

[System.Serializable]
public class GameState
{
    public List<string> wastePile;
    public List<string> deck;
    public List<string> matches;
    public List<StringListWrapper> foundations;
    public List<StringListWrapper> reactors;
    public List<AltMove> moveLog;
    public int score;
    public byte consecutiveMatches;
    public int moveCounter;
    public int actions;
    public string difficulty;
}
