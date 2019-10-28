using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreScript : MonoBehaviour
{
    public Text gameScore;
    private int score;

    void Start()
    {
        UpdateScore();
    }

    void Update()
    {
        if (score != Config.config.score)
        {
            UpdateScore();
        }
    }

    public void UpdateScore()
    {
        gameScore.text = Config.config.score.ToString();
        score = Config.config.score;
    }
}
