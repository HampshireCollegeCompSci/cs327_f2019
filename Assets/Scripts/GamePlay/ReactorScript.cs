using System.Collections.Generic;
using UnityEngine;

public class ReactorScript : MonoBehaviour
{
    public GameObject gameUI;

    public List<GameObject> cardList;
    public string suit;

    public GameObject suitGlow;
    private SpriteRenderer suitGlowSR;
    private SpriteRenderer glowSR;
    private Color oldSuitGlow;

    public Sprite glow;
    private bool isGlowing;
    private bool alertOn;

    void Start()
    {
        suitGlowSR = suitGlow.GetComponent<SpriteRenderer>();
        glowSR = this.gameObject.GetComponent<SpriteRenderer>();
    }

    private void CheckGameOver()
    {
        if (Config.Instance.tutorialOn) return;

        if (!Config.Instance.gameOver && CountReactorCard() >= Config.Instance.maxReactorVal)
        {
            EndGame.Instance.GameOver(false);
        }
    }

    public void AddCard(GameObject card)
    {
        if (cardList.Count != 0)
        {
            cardList[0].GetComponent<BoxCollider2D>().enabled = false;
            cardList[0].GetComponent<SpriteRenderer>().color = Config.GameValues.cardObstructedColor;
        }

        cardList.Insert(0, card);
        card.transform.SetParent(gameObject.transform);
        card.GetComponent<CardScript>().HideHologram();

        SetCardPositions();
        ReactorScoreSetScript.Instance.SetReactorScore();
        CheckGameOver();
    }

    public void RemoveCard(GameObject card)
    {
        card.GetComponent<SpriteRenderer>().color = card.GetComponent<CardScript>().originalColor;
        cardList.Remove(card);

        if (cardList.Count != 0)
        {
            cardList[0].GetComponent<BoxCollider2D>().enabled = true;
            cardList[0].GetComponent<SpriteRenderer>().color = cardList[0].GetComponent<CardScript>().originalColor;
        }

        SetCardPositions();
        ReactorScoreSetScript.Instance.SetReactorScore();
    }

    public Vector3 GetNextCardPosition()
    {
        return this.gameObject.transform.TransformPoint(GetCardPosition(0, cardList.Count + 1));
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

    private const int maxFullReactorCards = 4;
    private const float xOffset = -0.02f;
    private const float startingYOffset = -0.34f;
    private const float largeYOffset = 0.16f;
    private const float smallYOffset = 0.03f;
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

    public void ProcessAction(GameObject input)
    {
        if (!input.CompareTag(Constants.cardTag)) return;

        if (UtilsScript.Instance.selectedCards.Count != 1)
        {
            throw new System.ArgumentException("utils.selectedCards must be of size 1");
        }

        GameObject selectedCard = UtilsScript.Instance.selectedCards[0];

        if (CardTools.CanMatch(input.GetComponent<CardScript>(), selectedCard.GetComponent<CardScript>()))
        {
            UtilsScript.Instance.Match(input, selectedCard);
        }
    }

    public int GetIncreaseOnNextCycle()
    {
        int output = 0;
        foreach (GameObject foundation in UtilsScript.Instance.foundations)
        {
            FoundationScript currentFoundationScript = foundation.GetComponent<FoundationScript>();
            if (currentFoundationScript.cardList.Count != 0)
            {
                CardScript topCardScript = currentFoundationScript.cardList[0].GetComponent<CardScript>();
                if (topCardScript.suit == suit)
                {
                    output += topCardScript.cardVal;
                }
            }
        }

        return output;
    }

    public int CountReactorCard()
    {
        int totalSum = 0;
        int cardListVal = cardList.Count;
        for (int i = 0; i < cardListVal; i++)
        {
            totalSum += cardList[i].gameObject.GetComponent<CardScript>().cardVal;
        }

        return totalSum;
    }

    public void TryHighlightOverloaded()
    {
        if (CountReactorCard() >= Config.Instance.maxReactorVal)
        {
            GlowOn(2);
            AlertOn();
        }
    }

    public void GlowOn(byte alertLevel)
    {
        if (isGlowing) return;

        isGlowing = true;

        suitGlowSR.enabled = true;
        glowSR.enabled = true;

        if (alertLevel == 1) // just highlight
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 0, 0.5f);
            ChangeSuitGlow(new Color(1, 1, 0, 0.3f));
        }
        else if (alertLevel == 2) // moving the selected token here will overload this reactor
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.5f);
            ChangeSuitGlow(new Color(1, 0, 0, 0.3f));
        }
    }

    public void GlowOff()
    {
        if (!isGlowing) return;

        isGlowing = false;

        suitGlowSR.enabled = false;
        glowSR.enabled = false;
    }

    public void AlertOn()
    {
        if (alertOn) return;

        alertOn = true;
        ReactorScoreSetScript.Instance.ChangeTextColor(gameObject, true);
    }

    public void AlertOff()
    {
        if (!alertOn) return;

        alertOn = false;
        ReactorScoreSetScript.Instance.ChangeTextColor(gameObject, false);
    }

    public void ChangeSuitGlow(Color newColor)
    {
        oldSuitGlow = suitGlowSR.color;
        suitGlowSR.color = newColor;
    }

    public void RevertSuitGlow()
    {
        suitGlowSR.color = oldSuitGlow;
    }

    public bool IsGlowing()
    {
        return isGlowing;
    }

    public Color GetGlowColor()
    {
        return gameObject.GetComponent<SpriteRenderer>().color;
    }
}
