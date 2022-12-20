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
