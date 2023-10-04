using System;
using UnityEngine;

[Serializable]
public struct Card
{
    public Card(Suit suit, Rank rank)
    {
        this.suit = suit;
        this.rank = rank;
        id = rank.Value + suit.Index * 13;
    }

    [SerializeField]
    private Suit suit;
    public Suit Suit => suit;

    [SerializeField]
    private Rank rank;
    public Rank Rank => rank;

    [SerializeField]
    private int id;
    public int ID => id;

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
        this.name = name;
        this.index = index;
        this.color = color;
    }

    [SerializeField]
    private string name;
    public string Name { get => name; }

    [SerializeField]
    private int index;
    public int Index { get => index; }

    [SerializeField]
    private Color color;
    public Color Color { get => color; }

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
        this.name = name;
        this.value = value;
        this.reactorValue = reactorValue;
    }

    [SerializeField]
    private string name;
    public string Name => name;

    [SerializeField]
    private int value;
    public int Value => value;

    [SerializeField]
    private int reactorValue;
    public int ReactorValue => reactorValue;
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

[Serializable]
#pragma warning disable CS0660, CS0661
public struct HighLightColor
#pragma warning restore CS0660, CS0661
{
    [SerializeField]
    private Color color;
    public Color Color => color;
    public readonly Color GlowColor;
    public readonly Color HoloColor;
    public readonly Color ScreenColor;
    public readonly Constants.ColorLevel ColorLevel;

    private const float glowAlpha = 0.6f;
    private const float screenDiv = 1.6f;

    public HighLightColor(string hexColor, Constants.ColorLevel colorLevel)
    {
        if (ColorUtility.TryParseHtmlString(hexColor, out Color newCol))
        {
            this.color = newCol;
            GlowColor = color;
            GlowColor.a = glowAlpha;
            HoloColor = color;
            HoloColor.a = GameValues.Colors.cardHologramAlpha;
            ScreenColor = color / screenDiv;
            ScreenColor.a = 1;
            this.ColorLevel = colorLevel;
        }
        else
        {
            throw new ArgumentException($"the hex of \"{hexColor}\" could not be parsed");
        }
    }

    public HighLightColor(Color color, Constants.ColorLevel colorLevel)
    {
        this.color = color;
        GlowColor = color;
        GlowColor.a = glowAlpha;
        HoloColor = color;
        HoloColor.a = GameValues.Colors.cardHologramAlpha;
        ScreenColor = color / screenDiv;
        ScreenColor.a = 1;
        this.ColorLevel = colorLevel;
    }

    public static bool operator ==(HighLightColor left, HighLightColor right)
    {
        return left.ColorLevel == right.ColorLevel;
    }

    public static bool operator !=(HighLightColor left, HighLightColor right)
    {
        return left.ColorLevel != right.ColorLevel;
    }
}

public readonly struct ColorMode
{
    public readonly string Name;
    public readonly HighLightColor Match, Move, Over, Notify;

    public ColorMode(string name,
                     HighLightColor match,
                     HighLightColor move,
                     HighLightColor over,
                     HighLightColor notify)
    {
        this.Name = name;
        this.Match = match;
        this.Move = move;
        this.Over = over;
        this.Notify = notify;
    }
}