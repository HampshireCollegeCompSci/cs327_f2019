[System.Serializable]
public class GameInfo
{
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
    public string[] difficulties;
    public int[] reactorLimits;
    public int[] moveLimits;
    public float selectedCardOpacity;
    public float[] cardObstructedColor;
    public float[] cardMoveHighlightColor;
    public float[] cardMatchHighlightColor;
    public float[] pointColor;
    public int turnAlertSmallThreshold;
    public int turnAlertThreshold;
    public int scoreMultiplier;

    public string[] menuButtonsTxtEnglish;
    public string[] levelButtonsTxtEnglish;
    public string backButtonTxtEnglish;
    public string loadingTxtEnglish;
    public string[] pauseButtonsTxtEnglish;
    public string[] summaryButtonsTxtEnglish;
    public string[] scoreActionLabelsTxtEnglish;
    public string[] gameStateTxtEnglish;
}
