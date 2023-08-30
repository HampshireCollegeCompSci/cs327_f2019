using System;
using TreeEditor;
using UnityEngine;

public class Stats
{
    private string timesWonKey, highScoreKey, leastMovesKey, highestComboKey, fastestTimeKey;

    public Stats(string name)
    {
        timesWonKey = $"{name} Times Won";
        highScoreKey = $"{name} High Score";
        leastMovesKey = $"{name} Least Moves";
        highestComboKey = $"{name} Highest Combo";
        fastestTimeKey = $"{name} Fastest Time";

        _timesWon = PlayerPrefs.GetInt(timesWonKey, 0);
        _highScore = PlayerPrefs.GetInt(highScoreKey, 0);
        _leastMoves = PlayerPrefs.GetInt(leastMovesKey, 0);
        _highestCombo = PlayerPrefs.GetInt(highestComboKey, 0);

        string timeSpanString = PlayerPrefs.GetString(fastestTimeKey, TimeSpan.Zero.ToString());
        bool status = TimeSpan.TryParse(timeSpanString, out TimeSpan result);
        _fastestTime = status ? result : TimeSpan.Zero;
    }

    private int _timesWon;
    public int TimesWon
    {
        get => _timesWon;
        set
        {
            _timesWon = value;
            PlayerPrefs.SetInt(timesWonKey, value);
        }
    }

    private int _highScore;
    public int HighScore
    {
        get => _highScore;
        set
        {
            _highScore = value;
            PlayerPrefs.SetInt(highScoreKey, value);
        }
    }

    private int _leastMoves;
    public int LeastMoves
    {
        get => _leastMoves;
        set
        {
            _leastMoves = value;
            PlayerPrefs.SetInt(leastMovesKey, value);
        }
    }

    private int _highestCombo;
    public int HighestCombo
    {
        get => _highestCombo;
        set
        {
            _highestCombo = value;
            PlayerPrefs.SetInt(highestComboKey, value);
        }
    }

    private TimeSpan _fastestTime;
    public TimeSpan FastestTime
    {
        get => _fastestTime;
        set
        {
            _fastestTime = value;
            PlayerPrefs.SetString(fastestTimeKey, value.ToString());
        }
    }

    public Stats ShallowCopy()
    {
        return (Stats)MemberwiseClone();
    }

    public static void SaveResults(bool didWin)
    {
        Stats currentStats = Config.Instance.CurrentDifficulty.Stats;

        if (Actions.Score > currentStats.HighScore)
        {
            currentStats.HighScore = Actions.Score;
        }

        if (!didWin) return;
        currentStats.TimesWon++;

        if (Actions.MoveCounter < currentStats.LeastMoves)
        {
            currentStats.LeastMoves = Actions.MoveCounter;
        }

        TimeSpan currentTime = Timer.GetTimeSpan();
        if (currentStats.FastestTime.Equals(TimeSpan.Zero) ||
            currentTime.CompareTo(currentStats.FastestTime) < 0)
        {
            currentStats.FastestTime = currentTime;
        }
    }

    public static void TryUpdateHighestCombo()
    {
        Stats currentStats = Config.Instance.CurrentDifficulty.Stats;
        if (Actions.ConsecutiveMatches > currentStats.HighestCombo)
        {
            currentStats.HighestCombo = Actions.ConsecutiveMatches;
        }
    }
}
