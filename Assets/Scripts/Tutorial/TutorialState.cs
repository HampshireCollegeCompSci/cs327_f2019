using System.Collections.Generic;

[System.Serializable]
public class TutorialState
{
    public List<string> wastePile;
    public List<string> deck;
    public List<string> matches;
    public List<TutorialFoundationCards> foundations;
    public List<TutorialReactorCards> reactors;
    public int score;
    public byte consecutiveMatches;
    public int moveCounter;
    public int actions;
    public string difficulty;
}

[System.Serializable]
public class TutorialFoundationCards
{
    public List<string> hidden;
    public List<string> unhidden;
}

[System.Serializable]
public class TutorialReactorCards
{
    public List<string> cards;
}
