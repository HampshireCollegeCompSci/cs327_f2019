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
        {
            gameState.text = Config.config.gameStateTxtEnglish[0].ToUpper();
        }

        else
        {
            gameState.text = Config.config.gameStateTxtEnglish[1].ToUpper();
        }
    }


}
