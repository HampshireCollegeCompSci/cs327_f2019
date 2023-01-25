﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReactorScript : MonoBehaviour, ICardContainer, IGlow
{
    private const int maxFullReactorCards = 5;
    private const float xOffset = -0.02f;
    private const float startingYOffset = -0.35f;
    private const float largeYOffset = 0.16f;
    private const float smallYOffset = 0.03f;

    [SerializeField]
    private List<GameObject> cardList;

    [SerializeField]
    private Suit suit;
    [SerializeField]
    private Text reactorScore;
    [SerializeField]
    private GameObject suitGlow;

    [SerializeField]
    private bool _glowing;
    [SerializeField]
    private HighLightColor _glowColor;
    [SerializeField]
    private bool _alert;

    private SpriteRenderer suitGlowSR;
    private SpriteRenderer glowSR;
    private BoxCollider2D hitbox;

    void Awake()
    {
        cardList = new(52);
        suitGlowSR = suitGlow.GetComponent<SpriteRenderer>();
        glowSR = this.gameObject.GetComponent<SpriteRenderer>();
        hitbox = this.gameObject.GetComponent<BoxCollider2D>();

        _glowing = false;
        _glowColor = GameValues.Colors.normal;
        _alert = false;
    }

    public List<GameObject> CardList => cardList;

    public bool Glowing
    {
        get => _glowing;
        set
        {
            if (_glowing == value) return;
            _glowing = value;
            hitbox.enabled = value;

            if (Config.Instance.HintsEnabled)
            {
                glowSR.enabled = value;
                suitGlowSR.enabled = value;
            }
        }
    }

    public HighLightColor GlowColor
    {
        get => _glowColor;
        set
        {
            Glowing = true;
            if (_glowColor.Equals(value)) return;
            _glowColor = value;
            glowSR.color = value.GlowColor;
            suitGlowSR.color = value.GlowColor;
        }
    }

    public bool Alert
    {
        get => _alert;
        set
        {
            if (_alert == value) return;
            _alert = value;
            if (value && Config.Instance.HintsEnabled)
            {
                reactorScore.color = Config.Instance.CurrentColorMode.Over.Color;
            }
            else
            {
                reactorScore.color = Color.black;
            }
        }
    }

    public Suit ReactorSuit => suit;

    public void SetReactorSuit(Suit suit)
    {
        this.suit = suit;
    }

    public void AddCard(GameObject card)
    {
        if (cardList.Count != 0)
        {
            cardList[^1].GetComponent<CardScript>().Obstructed = true;
        }

        cardList.Add(card);
        card.transform.SetParent(gameObject.transform);
        CardScript cardScript = card.GetComponent<CardScript>();
        cardScript.Hologram = false;
        cardScript.Obstructed = false;

        SetCardPositions();

        int cardValCount = CountReactorCard();
        SetReactorScore(cardValCount);
        CheckGameOver(cardValCount);
    }

    public void RemoveCard(GameObject card)
    {
        cardList.RemoveAt(cardList.LastIndexOf(card));

        if (cardList.Count != 0)
        {
            cardList[^1].GetComponent<CardScript>().Obstructed = false;
        }

        SetCardPositions();
        SetReactorScore();
    }

    public Vector3 GetNextCardPosition()
    {
        Vector3 nextPosition;
        if (cardList.Count == 0)
        {
            nextPosition = new Vector3(xOffset, startingYOffset, 0);
        }
        else
        {
            nextPosition = cardList[^1].transform.localPosition;
            nextPosition.y += largeYOffset;
        }
        return this.gameObject.transform.TransformPoint(nextPosition);
    }

    public int CountReactorCard()
    {
        int totalSum = 0;
        int cardListVal = cardList.Count;
        for (int i = 0; i < cardListVal; i++)
        {
            totalSum += cardList[i].GetComponent<CardScript>().Card.Rank.ReactorValue;
        }

        return totalSum;
    }

    public bool OverLimitSoon()
    {
        if (CountReactorCard() + GetIncreaseOnNextCycle() > Config.Instance.CurrentDifficulty.ReactorLimit)
        {
            Alert = true;
            return true;
        }

        return false;
    }

    public void SetReactorScore()
    {
        SetReactorScore(CountReactorCard());
    }

    public void SetReactorScore(int cardValCount)
    {
        reactorScore.text = $"{cardValCount}/{Config.Instance.CurrentDifficulty.ReactorLimit}";
    }

    public bool TryHighlightOverloaded(bool turnOn)
    {
        if (turnOn)
        {
            // will turn glowing on but not set the flag for it
            // so that it will not be turned off in the same frame
            if (CountReactorCard() > Config.Instance.CurrentDifficulty.ReactorLimit)
            {
                GlowColor = Config.Instance.CurrentColorMode.Over;
                reactorScore.color = Config.Instance.CurrentColorMode.Over.Color;
                _glowing = false;
                if (!Config.Instance.HintsEnabled)
                {
                    glowSR.enabled = true;
                    suitGlowSR.enabled = true;
                }
                return true;
            }
        }
        else
        {
            GlowColor = GameValues.Colors.normal;
            reactorScore.color = Color.black;
            _glowing = true;
            Glowing = false;
            if (!Config.Instance.HintsEnabled)
            {
                glowSR.enabled = false;
                suitGlowSR.enabled = false;
            }
        }
        return false;
    }

    public void ChangeSuitGlow(HighLightColor highLightColor)
    {
        //suitGlowSR.enabled = true; // TODO: this is needed again because of a Unity bug
        if (!Config.Instance.HintsEnabled)
        {
            suitGlowSR.enabled = true;
        }
        suitGlowSR.color = highLightColor.GlowColor;
    }

    public void RevertSuitGlow()
    {
        if (!Config.Instance.HintsEnabled)
        {
            suitGlowSR.enabled = false;
        }
        suitGlowSR.color = GlowColor.GlowColor;
    }

    private void CheckGameOver(int cardValCount)
    {
        if (Config.Instance.tutorialOn || Config.Instance.gameOver) return;

        if (cardValCount > Config.Instance.CurrentDifficulty.ReactorLimit)
        {
            EndGame.Instance.GameOver(false);
        }
    }

    private void SetCardPositions()
    {
        for (int i = 0; i < cardList.Count; i++)
        {
            cardList[i].transform.localPosition = GetCardPosition(i);
        }
    }

    private Vector3 GetCardPosition(int index)
    {
        float zOffset = index * -0.01f;

        // if there are too many cards in the reactor to display them all in full
        if (cardList.Count > maxFullReactorCards)
        {
            int numSmallCards = cardList.Count - maxFullReactorCards;
            // if this card is below the top number of maxFullReactorCards
            if (index < numSmallCards)
            {
                // make the y-offset smaller
                return new Vector3(xOffset, startingYOffset + smallYOffset * index, zOffset);
            }
            else
            {
                // add the needed number of small y-offsets in addition to a number of large y-offsets
                return new Vector3(xOffset, startingYOffset + smallYOffset * numSmallCards + largeYOffset * (index - numSmallCards), zOffset);
            }
        }
        else
        {
            return new Vector3(xOffset, startingYOffset + largeYOffset * index, zOffset);
        }
    }

    private int GetIncreaseOnNextCycle()
    {
        int output = 0;
        foreach (FoundationScript foundationScript in UtilsScript.Instance.foundationScripts)
        {
            if (foundationScript.CardList.Count != 0)
            {
                CardScript topCardScript = foundationScript.CardList[^1].GetComponent<CardScript>();
                if (topCardScript.Card.Suit.Equals(ReactorSuit))
                {
                    output += topCardScript.Card.Rank.ReactorValue;
                }
            }
        }

        return output;
    }
}
