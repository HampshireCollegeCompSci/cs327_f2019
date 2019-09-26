using UnityEngine;

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
    public int[] startingStack;
    public int cardsToDeal;
    public int cardsToFlipOver;
    public int wastePileCardsToShow;
    

    public static GameInfo CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<GameInfo>(jsonString);
    }
   
}