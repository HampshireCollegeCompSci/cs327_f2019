using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultsScript : MonoBehaviour
{
    public Text gameState;
    public Text gameScore;

    private void Start()
    {
        WinOrLose(Config.config.gameWin, GameObject.Find("Utils").GetComponent<UtilsScript>().score);

    }

    public void WinOrLose(bool win, float score)
    {

        if (win)
            gameState.text = "You Won!";

        else
            gameState.text = "You Lost!";

        gameState.alignment = TextAnchor.MiddleCenter;
        gameScore.text = "score " + score;
    }
}
