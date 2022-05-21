using System.Collections.Generic;

[System.Serializable]
public class GameState
{
    public List<byte> wastePile;
    public List<byte> deck;
    public List<byte> matches;
    public List<FoundationCards> foundations;
    public List<ReactorCards> reactors;
    public List<SaveMove> moveLog;
    public int score;
    public byte consecutiveMatches;
    public int moveCounter;
    public int actions;
    public string difficulty;
}

[System.Serializable]
public class FoundationCards
{
    public List<byte> hidden;
    public List<byte> unhidden;
}

[System.Serializable]
public class ReactorCards
{
    public List<byte> cards;
}

[System.Serializable]
public class SaveMove
{
    public byte c;
    public int o;
    public byte m;
    public byte h;
    public byte a;
    public int r;
    public int s;
    public int n;
}
