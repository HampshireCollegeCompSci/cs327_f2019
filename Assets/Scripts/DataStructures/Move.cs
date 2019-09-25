using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    GameObject card;
    GameObject stackFrom;
    GameObject stackTo;

    public Move(GameObject Card, GameObject StackFrom, GameObject StackTo)
    {
        card = Card;
        stackFrom = StackFrom;
        stackTo = StackTo;
    }
}
