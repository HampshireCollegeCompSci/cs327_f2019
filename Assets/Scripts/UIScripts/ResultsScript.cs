using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ResultsScript : MonoBehaviour
{
    public Text stateText, difficultyText;

    public Text currentScoreText, currentScoreStatText;
    public Text oldHighScoreText, oldHighScoreStatText;

    public Text currentMovesText, currentMovesStatText;
    public Text oldLeastMovesText, oldLeastMovesStatText;

    // Start is called before the first frame update
    void Start()
    {
        SetScoreBoard();
    }

    private void SetScoreBoard()
    {
        // game won or lost text
        if (Config.Instance.gameWin)
        {
            stateText.text = Config.GameValues.gameStateTxtEnglish[0];
        }
        else
        {
            stateText.text = Config.GameValues.gameStateTxtEnglish[1];
        }

        difficultyText.text = Config.Instance.currentDifficulty.ToUpper();

        SetScore();
        SetMoves();
    }

    private void SetScore()
    {
        string highScoreKey = PlayerPrefKeys.GetHighScoreKey(Config.Instance.currentDifficulty);

        // Set the current score
        int currentScoreNum = Config.Instance.score;
        currentScoreStatText.text = currentScoreNum.ToString();

        if (PlayerPrefs.HasKey(highScoreKey))
        {
            // Set the old high score text
            int oldHighScoreStatNum = PlayerPrefs.GetInt(highScoreKey);
            oldHighScoreStatText.text = oldHighScoreStatNum.ToString();

            // Check for a new high score and update if need be
            if (currentScoreNum > oldHighScoreStatNum)
            {
                UpdateHigh(currentScoreText, currentScoreStatText, highScoreKey, currentScoreNum);
            }
        }
        else 
        {
            // Set the text to indicate no saved high scores
            oldHighScoreStatText.fontSize = oldHighScoreText.fontSize;
            oldHighScoreStatText.text = "NONE";

            UpdateHigh(currentScoreText, currentScoreStatText, highScoreKey, currentScoreNum);
        }
    }

    private void SetMoves()
    {
        string leastMovesKey = PlayerPrefKeys.GetLeastMovesKey(Config.Instance.currentDifficulty);

        // Set the current moves
        int currentMovesNum = Config.Instance.moveCounter;
        currentMovesStatText.text = currentMovesNum.ToString();

        if (PlayerPrefs.HasKey(leastMovesKey))
        {
            // Set the old least moves text
            int oldLeastMovesNum = PlayerPrefs.GetInt(leastMovesKey);
            oldLeastMovesStatText.text = oldLeastMovesNum.ToString();

            // If game won, check for a new least moves and update if need be
            if (Config.Instance.gameWin && currentMovesNum < oldLeastMovesNum)
            {
                UpdateHigh(currentMovesText, currentMovesStatText, leastMovesKey, currentMovesNum);
            }
        }
        else
        {
            // Set the text to indicate no saved least moves
            oldLeastMovesStatText.fontSize = oldLeastMovesText.fontSize;
            oldLeastMovesStatText.text = "NONE";

            if (Config.Instance.gameWin)
            {
                UpdateHigh(currentMovesText, currentMovesStatText, leastMovesKey, currentMovesNum);
            }
        }
    }

    private void UpdateHigh(Text title, Text stat, string key, int update)
    {
        title.color = Color.cyan;
        stat.color = Color.cyan;
        PlayerPrefs.SetInt(key, update);
    }
}
