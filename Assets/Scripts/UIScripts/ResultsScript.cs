using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ResultsScript : MonoBehaviour
{
    public Text state;

    public Text scoreStat;
    public Text highScore, highScoreStat;

    public Text leastMoves, leastMovesStat;
    public Text movesStat;

    public SpaceBabyController spaceBaby;
    public Sprite spaceShipDebris;
    public GameObject spaceShip;

    // Start is called before the first frame update
    void Start()
    {
        // state
        if (Config.config.gameWin)
            state.text = Config.config.gameStateTxtEnglish[0];
        else
            state.text = Config.config.gameStateTxtEnglish[1];

        // score
        scoreStat.text = Config.config.score.ToString();

        // high score
        highScore.text = Config.config.currentDifficulty + " HIGH SCORE";

        // moves
        movesStat.text = Config.config.moveCounter.ToString();

        // spacebaby
        if (Config.config.gameWin)
        {
            spaceBaby.BabyWin(Config.config.matchCounter);
        }
        else
        {
            spaceBaby.BabyLose();
            SpaceShipExplode();
        }

        SetHighScore();
        SetLeastMoves();
    }

    /// <summary>
    /// Checks to see if the current score is better than the recorded best for the current difficulty.
    /// If so, saves it and indicates in game that it is new.
    /// </summary>
    private void SetHighScore()
    {
        string highScoreKey = PlayerPrefKeys.GetHighScoreKey(Config.config.currentDifficulty);
        int currentScore = Config.config.score;
        if (!PlayerPrefs.HasKey(highScoreKey) || currentScore > PlayerPrefs.GetInt(highScoreKey))
        {
            PlayerPrefs.SetInt(highScoreKey, currentScore);
            highScore.color = Color.cyan;
            highScoreStat.color = Color.cyan;
            highScoreStat.text = currentScore.ToString();
        }
        else
        {
            highScoreStat.text = PlayerPrefs.GetInt(highScoreKey).ToString();
        }
    }

    /// <summary>
    /// Checks to see if the game was won and the current move count is less than the recorded best for the current difficulty.
    /// If so, saves it and indicates in game that it is new.
    /// </summary>
    private void SetLeastMoves()
    {
        string leastMovesKey = PlayerPrefKeys.GetLeastMovesKey(Config.config.currentDifficulty);
        if (Config.config.gameWin)
        {
            int currentMoves = Config.config.moveCounter;
            if (!PlayerPrefs.HasKey(leastMovesKey) || currentMoves < PlayerPrefs.GetInt(leastMovesKey))
            {
                PlayerPrefs.SetInt(leastMovesKey, currentMoves);
                leastMoves.color = Color.cyan;
                leastMovesStat.color = Color.cyan;
                leastMovesStat.text = currentMoves.ToString();
            }
            else
            {
                leastMovesStat.text = PlayerPrefs.GetInt(leastMovesKey).ToString();
            }
        }
        else if (PlayerPrefs.HasKey(leastMovesKey))
        {
            leastMovesStat.text = PlayerPrefs.GetInt(leastMovesKey).ToString();
        }
        else
        {
            leastMovesStat.text = "NULL";
        }
    }

    private bool exploded = false;
    /// <summary>
    /// Starts the Explode Coroutine if the space ship has already not been exploded.
    /// </summary>
    public void SpaceShipExplode()
    {
        if (exploded)
            return;

        exploded = true;
        StartCoroutine(Explode());
    }

    /// <summary>
    /// Waits for a few seconds before playing an explosion sound and changing the space ship's sprite to debris.
    /// </summary>
    /// <returns></returns>
    IEnumerator Explode()
    {
        yield return new WaitForSeconds(2);
        Config.config.GetComponent<SoundController>().ExplosionSound();
        spaceShip.GetComponent<Image>().sprite = spaceShipDebris;
        spaceShip.transform.localScale = new Vector3(2, 2, 1);
    }
}
