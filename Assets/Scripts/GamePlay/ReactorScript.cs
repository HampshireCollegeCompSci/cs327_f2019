using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactorScript : MonoBehaviour
{
    public GameObject gameUI;
    private ReactorScoreSetScript rsss;

    public List<GameObject> cardList;
    public string suit;

    public GameObject suitGlow;
    private SpriteRenderer suitGlowSR;
    private Color oldSuitGlow;

    public Sprite glow;
    private bool isGlowing;
    private bool alertOn;

    void Start()
    {
        rsss = gameUI.GetComponent<ReactorScoreSetScript>();
        suitGlowSR = suitGlow.GetComponent<SpriteRenderer>();
    }

    private void CheckGameOver()
    {
        if (Config.config.tutorialOn)
            return;

        if (CountReactorCard() >= Config.config.maxReactorVal && !Config.config.gameOver)
            Config.config.GameOver(false);
    }

    public void AddCard(GameObject card)
    {
        if (cardList.Count != 0)
        {
            cardList[0].GetComponent<BoxCollider2D>().enabled = false;
            cardList[0].GetComponent<SpriteRenderer>().color = Config.config.cardObstructedColor;
        }

        cardList.Insert(0, card);
        card.transform.SetParent(gameObject.transform);
        card.GetComponent<CardScript>().HideHologram();

        SetCardPositions();
        rsss.SetReactorScore();
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
        rsss.SetReactorScore();
    }

    public void SetCardPositions()
    {
        int positionCounter = 0;
        float yOffset = -0.8f;

        for (int i = cardList.Count - 1; i >= 0; i--)  // go backwards through the list
        {
            // as we go through, place cards above and in-front the previous one
            cardList[i].transform.position = gameObject.transform.position + new Vector3(-0.02f, yOffset, -1 - positionCounter * 0.1f);

            // 4 tokens can visibly fit in the container at once, so hide the bottom ones if over 4
            if (!(cardList.Count > 5 && positionCounter < cardList.Count - 5))
                yOffset += 0.45f;
            else
                yOffset += 0.1f;

            positionCounter += 1;
        }
    }

    public void ProcessAction(GameObject input)
    {
        if (!input.CompareTag("Card"))
            return;

        if (UtilsScript.Instance.selectedCards.Count != 1)
            throw new System.ArgumentException("utils.selectedCards must be of size 1");

        GameObject selectedCard = UtilsScript.Instance.selectedCards[0];

        if (CardTools.CanMatch(input.GetComponent<CardScript>(), selectedCard.GetComponent<CardScript>()))
            UtilsScript.Instance.Match(input, selectedCard);
    }

    public int GetIncreaseOnNextCycle()
    {
        int output = 0;
        foreach (GameObject foundation in Config.config.foundations)
        {
            FoundationScript currentFoundationScript = foundation.GetComponent<FoundationScript>();
            if (currentFoundationScript.cardList.Count != 0)
            {
                CardScript topCardScript = currentFoundationScript.cardList[0].GetComponent<CardScript>();
                if (topCardScript.cardSuit == suit)
                    output += topCardScript.cardVal;
            }
        }

        return output;
    }

    public int CountReactorCard()
    {
        int totalSum = 0;
        int cardListVal = cardList.Count;
        for (int i = 0; i < cardListVal; i++)
            totalSum += cardList[i].gameObject.GetComponent<CardScript>().cardVal;

        return totalSum;
    }

    public void GlowOn(byte alertLevel)
    {
        if (isGlowing)
            return;
        isGlowing = true;

        suitGlow.SetActive(true);
        gameObject.GetComponent<SpriteRenderer>().enabled = true;
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
        if (!isGlowing)
            return;
        isGlowing = false;
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        suitGlow.SetActive(false);
    }

    public void AlertOn()
    {
        if (alertOn)
            return;
        alertOn = true;
        rsss.ChangeTextColor(gameObject, true);
    }

    public void AlertOff()
    {
        if (!alertOn)
            return;
        alertOn = false;
        rsss.ChangeTextColor(gameObject, false);
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
