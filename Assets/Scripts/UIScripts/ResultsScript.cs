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
            true => Config.GameValues.menuText.gameState[0],
            false => Config.GameValues.menuText.gameState[1]
        };
        difficultyText.text = Config.Instance.CurrentDifficulty.Name.ToUpper();

        SetScore();
        SetMoves();
    }

    private void SetScore()
    {
        // Set the current score
        int currentScoreNum = Config.Instance.score;
        currentScoreStatText.text = currentScoreNum.ToString();

        int oldHighScoreNum = PersistentSettings.GetHighScore(Config.Instance.CurrentDifficulty);
        if (oldHighScoreNum == 0)
        {
            // Set the text to indicate no saved high scores
            oldHighScoreStatText.fontSize = oldHighScoreText.fontSize;
            oldHighScoreStatText.text = "NONE";
        }
        else
        {
            oldHighScoreStatText.text = oldHighScoreNum.ToString();
        }

        // Check for a new high score and update if need be
        if (currentScoreNum > oldHighScoreNum)
        {
            UpdateHigh(currentScoreText, currentScoreStatText, oldHighScoreText, oldHighScoreStatText);
            PersistentSettings.SetHighScore(Config.Instance.CurrentDifficulty, currentScoreNum);
        }
    }

    private void SetMoves()
    {
        // Set the current moves
        int currentMovesNum = Config.Instance.moveCounter;
        currentMovesStatText.text = currentMovesNum.ToString();

        int oldLeastMovesNum = PersistentSettings.GetLeastMoves(Config.Instance.CurrentDifficulty);
        if (oldLeastMovesNum == 0)
        {
            // Set the text to indicate no saved least moves
            oldLeastMovesStatText.fontSize = oldLeastMovesText.fontSize;
            oldLeastMovesStatText.text = "NONE";
        }
        else
        {
            oldLeastMovesStatText.text = oldLeastMovesNum.ToString();
        }

        // If game won, check for a new least moves and update if need be
        if (Config.Instance.gameWin && (oldLeastMovesNum == 0 || currentMovesNum < oldLeastMovesNum))
        {
            UpdateHigh(currentMovesText, currentMovesStatText, oldLeastMovesText, oldLeastMovesStatText);
            PersistentSettings.SetLeastMoves(Config.Instance.CurrentDifficulty, currentMovesNum);
        }
    }

    private void UpdateHigh(Text newTitle, Text newStat, Text oldTitle, Text oldStat)
    {
        newTitle.color = Color.cyan;
        newStat.color = Color.cyan;

        oldTitle.color = Color.red;
        oldStat.color = Color.red;
    }
}
