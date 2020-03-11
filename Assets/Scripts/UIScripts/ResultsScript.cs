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

        if (Config.config.score > lastHighScore)
            highScore.GetComponent<Text>().text = "NEW HIGH SCORE " + Config.config.score;
        else
            highScore.GetComponent<Text>().text = "HIGH SCORE " + lastHighScore;

        leastMoves.GetComponent<Text>().text = "LEAST MOVES: " + PlayerPrefs.GetInt(Config.config.difficulty + "Moves");
        moves.GetComponent<Text>().text = "MOVES: " + Config.config.MoveCounter;

        if (Config.config.gameWin)
            spaceBaby.GetComponent<SpaceBabyController>().BabyWin(0);
        else
            spaceBaby.GetComponent<SpaceBabyController>().BabyLose();
    }
}
