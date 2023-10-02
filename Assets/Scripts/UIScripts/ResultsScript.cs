using System;
using UnityEngine;
using UnityEngine.UI;

public class ResultsScript : MonoBehaviour
{
    [SerializeField]
    private Text stateText, difficultyText,
        currentScoreText, currentScoreStatText,
        oldHighScoreText, oldHighScoreStatText,
        currentMovesText, currentMovesStatText,
        oldLeastMovesText, oldLeastMovesStatText,
        currentTimeText, currentTimeStatText,
        oldTimeText, oldTimeStatText;

    // Start is called before the first frame update
    void Start()
    {
        SetScoreBoard();
    }

    private void SetScoreBoard()
    {
        // game won or lost text
        stateText.text = Actions.GameWin ? GameValues.Text.gameWon : GameValues.Text.gameLost;
        difficultyText.text = Config.Instance.CurrentDifficulty.Name;

        SetScore();
        SetMoves();
        SetTime();
    }

    private void SetScore()
    {
        // Set the current score
        int currentScoreNum = Actions.Score;
        currentScoreStatText.text = Actions.Score.ToString();

        int oldHighScoreNum = Config.Instance.oldStats.HighScore;
        if (oldHighScoreNum == 0)
        {
            oldHighScoreText.color = GameValues.Colors.whiteAlphaLow;
            oldHighScoreStatText.text = GameValues.Text.noValue;
        }
        else
        {
            oldHighScoreStatText.text = oldHighScoreNum.ToString();
        }

        // Check for a new high score and update if need be
        if (currentScoreNum > oldHighScoreNum)
        {
            UpdateHigh(currentScoreText, currentScoreStatText, oldHighScoreText, oldHighScoreStatText);
        }
    }

    private void SetMoves()
    {
        // Set the current moves
        int currentMovesNum = Actions.MoveCounter;
        currentMovesStatText.text = currentMovesNum.ToString();

        int oldLeastMovesNum = Config.Instance.oldStats.LeastMoves;
        if (oldLeastMovesNum == 0)
        {
            oldLeastMovesText.color = GameValues.Colors.whiteAlphaLow;
            oldLeastMovesStatText.text = GameValues.Text.noValue;
        }
        else
        {
            oldLeastMovesStatText.text= oldLeastMovesNum.ToString();
        }

        // If game won, check for a new least moves and update if need be
        if (Actions.GameWin && (oldLeastMovesNum == 0 || currentMovesNum < oldLeastMovesNum))
        {
            UpdateHigh(currentMovesText, currentMovesStatText, oldLeastMovesText, oldLeastMovesStatText);
        }
    }

    private void SetTime()
    {
        TimeSpan currentTime = Timer.GetTimeSpan();
        currentTimeStatText.text = Timer.GetTimeSpan().ToString(Constants.Time.format);

        TimeSpan oldTimerNum = Config.Instance.oldStats.FastestTime;
        bool noOldTime = oldTimerNum.Equals(TimeSpan.Zero);
        if (noOldTime)
        {
            oldTimeText.color = GameValues.Colors.whiteAlphaLow;
            oldTimeStatText.text = GameValues.Text.noValue;
        }
        else
        {
            oldTimeStatText.text = oldTimerNum.ToString(Constants.Time.format);
        }

        if (!Actions.GameWin) return;

        if (noOldTime || currentTime.CompareTo(oldTimerNum) < 0)
        {
            UpdateHigh(currentTimeText, currentTimeStatText, oldTimeText, oldTimeStatText);
        }
    }

    private void UpdateHigh(Text newTitle, Text newStat, Text oldTitle, Text oldStat)
    {
        newTitle.color = Config.Instance.CurrentColorMode.Match.Color;
        newStat.color = Config.Instance.CurrentColorMode.Match.Color;

        Color oldColor = Config.Instance.CurrentColorMode.Over.Color;
        if (oldTitle.color.a != 1)
        {
            oldColor.a = oldTitle.color.a;
        }
        oldTitle.color = oldColor;
        oldStat.color = oldColor;
    }
}
