using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchCancel : MonoBehaviour
{
    void Update()
    {
        if (Config.config.gamePaused)
        {
            Destroy(gameObject);
        }
    }
}
