using UnityEngine;
using UnityEngine.UI;

public class ResultsScript : MonoBehaviour
{
    [SerializeField]
    private Text stateText, difficultyText,
        currentScoreText, currentScoreStatText,
        oldHighScoreText, oldHighScoreStatText,
        currentMovesText, currentMovesStatText,
        oldLeastMovesText, oldLeastMovesStatText;

    // Start is called before the first frame update
    void Start()
    {
        SetScoreBoard();
    }

    private void SetScoreBoard()
    {
        // game won or lost text
        stateText.text = Config.Instance.gameWin switch
        {
            true => Config.GameValues.gameStateTxtEnglish[0],
            false => Config.GameValues.gameStateTxtEnglish[1]
        };
        difficultyText.text = Config.Instance.currentDifficulty.ToUpper();

        SetScore();
        SetMoves();
    }

    private void SetScore()
    {
        string highScoreKey = PersistentSettings.GetHighScoreKey(Config.Instance.currentDifficulty);

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
                UpdateHigh(currentScoreText, currentScoreStatText, oldHighScoreText, oldHighScoreStatText, highScoreKey, currentScoreNum);
            }
        }
        else
        {
            // Set the text to indicate no saved high scores
            oldHighScoreStatText.fontSize = oldHighScoreText.fontSize;
            oldHighScoreStatText.text = "NONE";

            UpdateHigh(currentScoreText, currentScoreStatText, oldHighScoreText, oldHighScoreStatText, highScoreKey, currentScoreNum);
        }
    }

    private void SetMoves()
    {
        string leastMovesKey = PersistentSettings.GetLeastMovesKey(Config.Instance.currentDifficulty);

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
                UpdateHigh(currentMovesText, currentMovesStatText, oldLeastMovesText, oldLeastMovesStatText, leastMovesKey, currentMovesNum);
            }
        }
        else
        {
            // Set the text to indicate no saved least moves
            oldLeastMovesStatText.fontSize = oldLeastMovesText.fontSize;
            oldLeastMovesStatText.text = "NONE";

            if (Config.Instance.gameWin)
            {
                UpdateHigh(currentMovesText, currentMovesStatText, oldLeastMovesText, oldLeastMovesStatText, leastMovesKey, currentMovesNum);
            }
        }
    }

    private void UpdateHigh(Text newTitle, Text newStat, Text oldTitle, Text oldStat, string key, int update)
    {
        newTitle.color = Color.cyan;
        newStat.color = Color.cyan;

        oldTitle.color = Color.red;
        oldStat.color = Color.red;

        PlayerPrefs.SetInt(key, update);
    }
}
