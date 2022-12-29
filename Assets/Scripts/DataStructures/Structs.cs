using System;
using UnityEngine;

public readonly struct Difficulty
{
    public Difficulty(string name, int reactorLimit, int moveLimit)
    {
        Name = name;
        ReactorLimit = reactorLimit;
        MoveLimit = moveLimit;
    }

    public readonly string Name;

    public readonly int ReactorLimit;

    public readonly int MoveLimit;
}

[Serializable]
public struct Card
{
    public Card(Suit suit, Rank rank)
    {
        this.suit = suit;
        this.rank = rank;
    }

    [SerializeField]
    private Suit suit;
    public Suit Suit => suit;

    [SerializeField]
    private Rank rank;
    public Rank Rank => rank;

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

[Serializable]
public struct HighLightColor
{
    public HighLightColor(Color color)
    {
        this.color = color;
        glowColor = color;
        glowColor.a = 0.3f;
    }

    [SerializeField]
    private Color color;
    public Color Color => color;

    public readonly Color glowColor;
}