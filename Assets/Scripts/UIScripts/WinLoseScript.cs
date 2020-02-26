using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinLoseScript : MonoBehaviour
{
    void Start()
    {
        if (Config.config.gameWin)
        {
            gameObject.SetActive(true);
        }
    }
}
