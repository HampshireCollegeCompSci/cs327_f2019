using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreScript : MonoBehaviour
{
    public Text gameScore;

    public void Start()
    {
        // for the pause and end game scores to load
        if (SceneManager.GetActiveScene().name != "SummaryScene")
        {
            UpdatePauseScore();
        }
        else
        {
            UpdateScore();
        }


    }

    public void UpdateScore()
    {
        gameScore.text = "Score " + Config.config.score.ToString();
    }

    public void UpdatePauseScore()
    {
        gameScore.text = Config.config.score.ToString();
    }
}
