using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReactorScript : MonoBehaviour, ICardContainer, IGlow
{
    private const int maxFullReactorCards = 4;
    private const float xOffset = -0.02f;
    private const float startingYOffset = -0.34f;
    private const float largeYOffset = 0.16f;
    private const float smallYOffset = 0.03f;

    [SerializeField]
    private List<GameObject> cardList;

    [SerializeField]
    private byte reactorSuitIndex;
    [SerializeField]
    private Text reactorScore;
    [SerializeField]
    private GameObject suitGlow;

    [SerializeField]
    private bool _glowing;
    [SerializeField]
    private byte _glowLevel;
    [SerializeField]
    private bool _alert;

    private SpriteRenderer suitGlowSR;
    private SpriteRenderer glowSR;

    void Awake()
    {
        cardList = new();
        suitGlowSR = suitGlow.GetComponent<SpriteRenderer>();
        glowSR = this.gameObject.GetComponent<SpriteRenderer>();

        _glowing = false;
        _glowLevel = 0;
        _alert = false;
    }

    public List<GameObject> CardList
    {
        get => cardList;
    }

    public bool Glowing
    {
        get => _glowing;
        set
        {
            if (value && !_glowing)
            {
                _glowing = true;
                suitGlowSR.enabled = true;
                glowSR.enabled = true;
            }
            else if (!value && _glowing)
            {
                _glowing = false;
                RevertSuitGlow();
                suitGlowSR.enabled = false;
                glowSR.enabled = false;
            }
        }
    }

    public byte GlowLevel
    {
        get => _glowLevel;
        set
        {
            if (value != _glowLevel)
            {
                _glowLevel = value;
                Color glowColor = Config.GameValues.highlightColors[value];
                glowColor.a = 0.3f;
                glowSR.color = glowColor;
                ChangeSuitGlow(glowColor);
            }

            Glowing = true;
        }
    }

    public bool Alert
    {
        get => _alert;
        set
        {
            if (value == _alert) return;
            _alert = value;
            if (value)
            {
                reactorScore.color = Color.red;
            }
            else
            {
                reactorScore.color = Color.black;
            }
        }
    }

    public byte ReactorSuitIndex
    {
        get => reactorSuitIndex;
    }

    public void SetReactorSuitIndex(byte reactorSuitIndex)
    {
        this.reactorSuitIndex = reactorSuitIndex;
    }

    public void AddCard(GameObject card)
    {
        if (cardList.Count != 0)
        {
            cardList[0].GetComponent<CardScript>().Obstructed = true;
        }

        cardList.Insert(0, card);
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
        cardList.Remove(card);

        if (cardList.Count != 0)
        {
            cardList[0].GetComponent<CardScript>().Obstructed = false;
        }

        SetCardPositions();
        SetReactorScore();
    }

    public Vector3 GetNextCardPosition()
    {
        return this.gameObject.transform.TransformPoint(GetCardPosition(0, cardList.Count + 1));
    }

    public int CountReactorCard()
    {
        int totalSum = 0;
        int cardListVal = cardList.Count;
        for (int i = 0; i < cardListVal; i++)
        {
            totalSum += cardList[i].GetComponent<CardScript>().CardReactorValue;
        }

        return totalSum;
    }

    public bool OverLimitSoon()
    {
        if (CountReactorCard() + GetIncreaseOnNextCycle() > Config.Instance.reactorLimit)
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
        reactorScore.text = $"{cardValCount}/{Config.Instance.reactorLimit}";
    }

    public void TryHighlightOverloaded(bool turnOn)
    {
        if (turnOn)
        {
            // will turn glowing on but not set the flag for it
            // so that it will not be turned off later
            if (CountReactorCard() > Config.Instance.reactorLimit)
            {
                Glowing = true;
                GlowLevel = Constants.overHighlightColorLevel;
                _glowing = false;
                Alert = true;
                ChangeSuitGlow(3);
            }
        }
        else
        {
            _glowing = true;
            Glowing = false;
        }
    }

    public void RevertSuitGlow()
    {
        ChangeSuitGlow(GlowLevel);
    }

    public void ChangeSuitGlow(byte level)
    {
        Color newColor = Config.GameValues.highlightColors[level];
        newColor.a = 0.3f;
        ChangeSuitGlow(newColor);
    }

    private void CheckGameOver(int cardValCount)
    {
        if (Config.Instance.tutorialOn || Config.Instance.gameOver) return;

        if (cardValCount > Config.Instance.reactorLimit)
        {
            EndGame.Instance.GameOver(false);
        }
    }

    private void SetCardPositions()
    {
        int cardCount = cardList.Count;
        for (int i = 0; i < cardCount; i++)
        {
            Vector3 newPos = GetCardPosition(i, cardCount);
            cardList[i].transform.localPosition = newPos;
        }
    }

    private Vector3 GetCardPosition(int index, int cardListCount)
    {
        int reverseIndex = cardListCount - index - 1;
        float zOffset = -0.05f * (reverseIndex + 1);

        if (cardListCount > maxFullReactorCards)
        {
            if (index >= maxFullReactorCards)
            {
                return new Vector3(xOffset, startingYOffset + smallYOffset * reverseIndex, zOffset);
            }
            else
            {
                return new Vector3(xOffset, startingYOffset + smallYOffset * (cardListCount - 1 - maxFullReactorCards) + largeYOffset * (maxFullReactorCards - index), zOffset);
            }
        }
        else
        {
            return new Vector3(xOffset, startingYOffset + largeYOffset * reverseIndex, zOffset);
        }
    }

    private int GetIncreaseOnNextCycle()
    {
        int output = 0;
        foreach (FoundationScript foundationScript in UtilsScript.Instance.foundationScripts)
        {
            if (foundationScript.CardList.Count != 0)
            {
                CardScript topCardScript = foundationScript.CardList[0].GetComponent<CardScript>();
                if (topCardScript.CardSuitIndex == reactorSuitIndex)
                {
                    output += topCardScript.CardReactorValue;
                }
            }
        }

        return output;
    }

    private void ChangeSuitGlow(Color newColor)
    {
        suitGlowSR.color = newColor;
    }
}
