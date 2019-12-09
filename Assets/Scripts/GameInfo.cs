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

    public int foundationStartingSize;
    public byte wastepileAnimationSpeedSlow;
    public byte wastepileAnimationSpeedFast;
    public float draggedTokenOffset;
    public byte cardsToDeal;
    public byte cardsToReactorspeed;
    public float relativeCardScale;
    public int turnsTillReset;
    public int matchPoints;
    public int emptyReactorPoints;
    public int perfectGamePoints;
    public int delayToShowGameSummary;
    public int easyReactorLimit;
    public int mediumReactorLimit;
    public int hardReactorLimit;
    public int easyMoveCount;
    public int mediumMoveCount;
    public int hardMoveCount;
    public float selectedCardOpacity;
    public float[] cardHighlightColor;
    public string[] gameStateTxtEnglish;
    public string[] menuSceneButtonsTxtEnglish;
    public string loadingSceneTxtEnglish;
    public string[] levelSceneButtonsTxtEnglish;
    public string[] pauseSceneButtonsTxtEnglish;
    public string[] summarySceneButtonsTxtEnglish;
    public int turnAlertSmallThreshold;
    public int turnAlertThreshold;
}