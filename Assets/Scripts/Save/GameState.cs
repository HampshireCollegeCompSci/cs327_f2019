using System;
using System.Collections.Generic;

[Serializable]
public class GameState<T>
{
    public string difficulty;
    public string timer;
    public int moveCounter;
    public int moveTracker;
    public int actions;
    public int score;
    public int consecutiveMatches;
    public ReactorCards[] reactors;
    public FoundationCards[] foundations;
    public List<T> wastePile;
    public List<T> deck;
    public List<T> matches;
    public List<SaveMove> moveLog;
    public List<Achievement> achievements;

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
            hidden = new List<T>();
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
    // using single character variable names to reduce save file size
    public byte c; // cardID
    public Constants.CardContainerType t;
    public int i; // card container index
    public Constants.LogMoveType m;
    public byte h; // next card was hidden
    public byte a; // was action
    public int r; // remaining actions
    public int s; // score
    public int n; // move number
}