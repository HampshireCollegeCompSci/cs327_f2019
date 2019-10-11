using UnityEngine;
using System.IO;
[System.Serializable]
public class GameInfo
{

    //reactorLimit = int
    //(A four element array)

    //startingStack = int
    //(How many cards are dealt to each starting stack.A value of 7 results in 6 face down and 1 face up)

    //cardsToDeal = int
    //(How many cards should we deal at a time?)

    //wastepileCardsToShow
    //(how many cards to fully show on top of the wastepile)

    //nonTopXOffset = float
    //(adjusts how compressed the hidden cards in the wastepile are)

    //cardsToDeal = int
    //(how many cards to deal per click on the deck)

    public int reactorLimit;
    public int foundationStartingSize;
    public int wastepileCardsToShow;
    public float nonTopXOffset;
    public int cardsToDeal;

}