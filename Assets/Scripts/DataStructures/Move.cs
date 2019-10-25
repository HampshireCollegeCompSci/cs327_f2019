using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public GameObject card;
    public GameObject origin;
    public string moveType;
    public bool nextCardWasHidden;

    public Move(string moveType, GameObject card, GameObject origin, bool nextCardWasHidden)
    {
        this.card = card;
        this.origin = origin;
        this.moveType = moveType;
        this.nextCardWasHidden = nextCardWasHidden;
    }
}
