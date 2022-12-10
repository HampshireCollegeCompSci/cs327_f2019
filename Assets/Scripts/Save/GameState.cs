using System;
using System.Collections.Generic;

[Serializable]
public class GameState<T>
{
    public string difficulty;
    public int moveCounter;
    public int actions;
    public int score;
    public byte consecutiveMatches;
    public ReactorCards[] reactors;
    public FoundationCards[] foundations;
    public List<T> wastePile;
    public List<T> deck;
    public List<T> matches;
    public List<SaveMove> moveLog;

    [Serializable]
    public class ReactorCards
    {
        public List<T> cards;

        public ReactorCards()
        {
            cards = new List<T>();
        }
    }

    [Serializable]
    public class FoundationCards
    {
        public List<T> unhidden;
        public List<T> hidden;

        public FoundationCards()
        {
            unhidden = new List<T>();
            hidden= new List<T>();
        }
    }

    public GameState()
    {
        reactors = new ReactorCards[4];
        for (int i = 0; i < reactors.Length; i++)
        {
            reactors[i] = new ReactorCards();
        }
        foundations = new FoundationCards[4];
        for (int i = 0; i < foundations.Length; i++)
        {
            foundations[i] = new FoundationCards();
        }
    }
}

[Serializable]
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
