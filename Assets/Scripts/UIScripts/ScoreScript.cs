using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreScript : MonoBehaviour
{
    public Text gameScore;

    public void Start()
    {
        // for the pause and end game scores to load
        UpdateScore();
    }

    public void UpdateScore()
    {
        gameScore.text = Config.config.score.ToString();
    }
}
