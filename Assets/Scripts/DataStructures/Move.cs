using UnityEngine;

public class Move
{
    public GameObject card;
    public Constants.CardContainerType containerType;
    public GameObject origin;
    public Constants.LogMoveType moveType;
    public bool nextCardWasHidden;
    public bool isAction;
    public int remainingActions;
    public int score;
    public int moveNum;
}
