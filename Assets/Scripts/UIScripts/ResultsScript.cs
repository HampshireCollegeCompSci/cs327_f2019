using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultsScript : MonoBehaviour
{
    public GameObject highScore;
    public GameObject leastMoves;
    public GameObject moves;
    // Start is called before the first frame update
    void Start()
    {
        highScore.GetComponent<Text>().text = "High Score: " + PlayerPrefs.GetInt(Config.config.difficulty + "HighScore");
        leastMoves.GetComponent<Text>().text = "Least Moves: " + PlayerPrefs.GetInt(Config.config.difficulty + "Moves");
        moves.GetComponent<Text>().text = "Moves: " + Config.config.moves;
    }
}
