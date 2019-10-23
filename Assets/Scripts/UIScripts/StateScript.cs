using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateScript : MonoBehaviour
{
    public Text gameState;
    public Text gameScore;

    private void Start()
    {
        WinOrLose(Config.config.gameWin);
    }

    public void WinOrLose(bool win)
    {

        if (win)
        {
            gameState.text = "You Won!";
            ShowScore(Config.config.score);

        }

        else
        {
            gameState.text = "You Lost!";
            gameScore.text = "";
        }
    }

    public void ShowScore(int score)
    {
        gameScore.text = "score:" + score;
    }
}
