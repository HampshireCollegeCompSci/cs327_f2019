using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultsScript : MonoBehaviour
{
    public GameObject highScore;
    public GameObject leastMoves;
    public GameObject moves;

    public GameObject spaceBaby;

    // Start is called before the first frame update
    void Start()
    {
        int lastHighScore = PlayerPrefs.GetInt(Config.config.difficulty + "HighScore");
        int lastLeastMoves = PlayerPrefs.GetInt(Config.config.difficulty + "Moves");

        if (Config.config.score > lastHighScore)
            highScore.GetComponent<Text>().text = "NEW " + Config.config.difficulty + " HIGH SCORE " + Config.config.score;
        else
            highScore.GetComponent<Text>().text = "HIGH SCORE " + lastHighScore;

        if (Config.config.MoveCounter < lastLeastMoves)
            leastMoves.GetComponent<Text>().text = "NEW " + Config.config.difficulty + " LEAST MOVES: " + PlayerPrefs.GetInt(Config.config.difficulty + "Moves");
        else
            leastMoves.GetComponent<Text>().text = "LEAST MOVES: " + lastLeastMoves;

        moves.GetComponent<Text>().text = "MOVES: " + Config.config.MoveCounter;

        if (Config.config.gameWin)
            spaceBaby.GetComponent<SpaceBabyController>().BabyWin(Config.config.matchCounter);
        else
            spaceBaby.GetComponent<SpaceBabyController>().BabyLose();

        UtilsScript.global.SetHighScores();
    }
}
