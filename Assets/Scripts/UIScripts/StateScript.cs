using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateScript : MonoBehaviour
{
    public Text gameState;

    private void Start()
    {
        WinOrLose(Config.config.gameWin);

    }

    public void WinOrLose(bool win)
    {

        if (win)
            gameState.text = "You Won!";

        else
            gameState.text = "You Lost!";

        gameState.alignment = TextAnchor.MiddleCenter;
    }
}
