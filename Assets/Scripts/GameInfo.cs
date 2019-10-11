using UnityEngine;
using System.IO;
[System.Serializable]
public class GameInfo
{

    //reactorLimit = [int,int,int,int]
    //(A four element array)

    //startingStack = [int,int,int,int]
    //(How many cards are dealt to each starting stack.A value of 7 results in 6 face down and 1 face up)

    //cardsToDeal = int
    //(How many cards should we deal at a time?)

    public int[] reactorLimit;
    public int[] foundationStartingSize;
    public int cardsToWastePilePerClick;
    public int nonTopXOffset;

}