using System;
using UnityEngine;

[Serializable]
public class GameValues
{
    public bool enableCheat;

    public int foundationStartingSize;
    public int cardsToDeal;

    public float relativeCardScale;
    public float matchExplosionScale;

    public float draggedCardScale;
    public float draggedCardOffset;

    public int turnAlertSmallThreshold;
    public int turnAlertThreshold;

    public int matchPoints;
    public int scoreMultiplier;
    public bool enableBonusPoints;
    public int emptyReactorPoints;
    public int perfectGamePoints;

    public int wastepileAnimationSpeedSlow;
    public int wastepileAnimationSpeedFast;
    public int cardsToReactorspeed;

    public float musicFadeInDurationSec;
    public float musicFadeOutDurationSec;
    public float musicFadeOutSlowDurationSec;

    public int musicDefaultVolume;
    public int soundEffectsDefaultVolume;
    public bool vibrationEnabledDefault;
    public bool foodSuitsEnabledDefault;

    public float fadeOutButtonsSpeed;
    public int zoomFactor;
    public float panAndZoomSpeed;
    public float startGameFadeInSpeed;
    public float endGameFadeOutSpeed;
    public float summaryTransitionSpeed;

    public float reactorMeltDownSpeed;

    public Difficulty[] difficulties;

    public float selectedCardOpacity;

    public Color cardObstructedColor;

    public Color matchHighlightColor;
    public Color moveHighlightColor;
    public Color overHighlightColor;
    public Color[] highlightColors;

    public Color pointColor;

    public Color tutorialObjectHighlightColor;

    public Color fadeDarkColor;

    public Color fadeLightColor;

    public MenuText menuText;

    [Serializable]
    public class MenuText
    {
        public string[] menuButtons;
        public string[] levelButtons;
        public string backButton;
        public string loading;
        public string[] pauseButtons;
        public string[] summaryButtons;
        public string[] scoreActionLabels;
        public string[] gameState;
    }

    public static class Colors
    {
        public static readonly Color gameOverWin = Color.cyan;
        public static readonly Color gameOverLose = Color.red;
    }

    public static class Text
    {
        public static readonly string gameOverWin = "YOU WON!";
        public static readonly string gameOverLose = "YOU LOST!";
    }

    public static class AlertLevels
    {
        // for the action counter
        public static readonly AlertLevel none = new(new Color(0.725f, 0.725f, 0.725f), Color.white);
        public static readonly AlertLevel low = new(new Color(0.941f, 0.706f, 0.055f), new Color(0.6f, 0.45f, 0.039f));
        public static readonly AlertLevel high = new(new Color(0.835f, 0.2f, 0.098f), new Color(0.56f, 0.141f, 0.11f));
    }

    public static class AnimationDurataions
    {
        // all durations are in seconds

        // main menu
        public const float logoDelay = 2;
        public const float buttonFadeOut = 0.5f;

        // screen fades 
        public const float startGameFadeIn = 1; // fades out the game startup logos
        public const float gameplayFadeIn = 1; // fades in the gameplay scene
        public const float gameOverFade = 1f; // fades in the game over pop-up
        public const float gameEndFade = 1.5f; // fades out of the gameplay scene to summary
        public const float summaryFadeIn = 1f; // fades into the summary scene
        public const float playAgainFadeOut = 2; // fades out of the summary scene to gameplay

        // gameplay

        public const float cardHologramFadeIn = 2; // fades in the cards holograms
        public const float cardsToReactor = 0.6f; // movement of the cards to reactor during a nextcycle
        // match effect
        public const float comboPointsFadeIn = 0.5f; // fade in and scale up the point text
        public const float comboWait = 0.5f; // wait a bit
        public const float comboFadeOut = 1; // fade out and scale up the point text and food combo object

        public const float alertFade = 1; // action counter's alert siren

        public const float gameSummaryBabyFade = 2; // fade in, then out baby win transitioning in the summary scene
    }

    public static class FadeColors
    {
        // the colors that will be faded from and to during screen fades
        public static readonly Color blackA0 = new(0, 0, 0, 0);
        public static readonly Color blackA1 = new(0, 0, 0, 1);

        public static readonly Color grayA0 = new(0.6f, 0.6f, 0.6f, 0);
        public static readonly Color grayA1 = new(0.6f, 0.6f, 0.6f, 1);

        public static readonly FadeColorPair blackFadeOut = new(blackA1, blackA0);
        public static readonly FadeColorPair backFadeIn = new(blackA0, blackA1);
        
        public static readonly FadeColorPair grayFadeIn = new(grayA0, grayA1);
        public static readonly FadeColorPair grayFadeOut = new(grayA1, grayA0);
    }
}

[Serializable]
public struct Difficulty
{
    [SerializeField]
    private string name;
    public string Name { get => name; }
    [SerializeField]
    private int reactorLimit;
    public int ReactorLimit { get => reactorLimit; }
    [SerializeField]
    private int moveLimit;
    public int MoveLimit { get => moveLimit; }
}

[Serializable]
public struct Card
{
    public Card(Suit suit, Rank rank)
    {
        _suit = suit;
        _rank = rank;
    }

    [SerializeField]
    private Suit _suit;
    public Suit Suit { get => _suit; }

    [SerializeField]
    private Rank _rank;
    public Rank Rank { get => _rank; }

    public static bool CanMatch(Card card1, Card card2)
    {
        return card1.Rank.Equals(card2.Rank) && Suit.IsComplementary(card1.Suit, card2.Suit);
    }
}

[Serializable]
public struct Suit
{
    public Suit(string name, int index, Color color)
    {
        _name = name;
        _index = index;
        _color = color;
    }

    [SerializeField]
    private string _name;
    public string Name { get => _name; }

    [SerializeField]
    private int _index;
    public int Index { get => _index; }

    [SerializeField]
    private Color _color;
    public Color Color { get => _color; }

    public static bool IsComplementary(Suit suit1, Suit suit2)
    {
        return suit1.Index == GetComplementaryIndex(suit2);
    }

    public static int GetComplementaryIndex(Suit suit)
    {
        return suit.Index switch
        {
            0 => 1,
            1 => 0,
            2 => 3,
            3 => 2,
            _ => throw new ArgumentException()
        };
    }
}

[Serializable]
public struct Rank
{
    public Rank(string name, int value, int reactorValue)
    {
        _name = name;
        _value = value;
        _reactorValue = reactorValue;
    }

    [SerializeField]
    private string _name;
    public string Name { get => _name; }

    [SerializeField]
    private int _value;
    public int Value { get => _value; }

    [SerializeField]
    private int _reactorValue;
    public int ReactorValue { get => _reactorValue; }
}

public readonly struct AlertLevel
{
    public AlertLevel(Color lightColor, Color screenColor)
    {
        this.lightColor = lightColor;
        this.screenColor = screenColor;
    }

    public readonly Color lightColor;
    public readonly Color screenColor;
}

public readonly struct FadeColorPair
{
    public FadeColorPair(Color startColor, Color endColor)
    {
        this.startColor = startColor;
        this.endColor = endColor;
    }

    public readonly Color startColor;
    public readonly Color endColor;
}
