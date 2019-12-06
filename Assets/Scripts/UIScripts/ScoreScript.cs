using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreScript : MonoBehaviour
{
    public Text gameScore;

    void Start()
    {
        UpdateScore();
    }

    public void UpdateScore()
    {
        gameScore.text = Config.config.score.ToString();
    }
}
