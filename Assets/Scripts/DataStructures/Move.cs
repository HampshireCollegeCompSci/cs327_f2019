using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Move
{
    public GameObject card;
    public GameObject origin;
    public string moveType;
    public bool nextCardWasHidden;
    public bool isAction;
    public int remainingActions;
}
