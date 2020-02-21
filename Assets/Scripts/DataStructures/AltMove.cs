using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AltMove
{
    public string cardName;
    public string originName;
    public string moveType;
    public bool nextCardWasHidden;
    public bool isAction;
    public int remainingActions;
    public int moveNum;
}