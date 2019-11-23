using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Move
{
    public GameObject card;
    public GameObject origin;
    public string moveType;
    public bool nextCardWasHidden;
    public bool isAction;
    public int remainingActions;
}

[System.Serializable]
public class AltMove
{
    public string cardName;
    public string originName;
    public string moveType;
    public bool nextCardWasHidden;
    public bool isAction;
    public int remainingActions;
}
