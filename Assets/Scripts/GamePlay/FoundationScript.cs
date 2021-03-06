﻿using System.Collections.Generic;
using UnityEngine;

public class FoundationScript : MonoBehaviour
{
    public List<GameObject> cardList;
    private SpriteRenderer sp;
    private bool isGlowing;

    void Start()
    {
        sp = gameObject.GetComponent<SpriteRenderer>();
    }

    public void AddCard(GameObject card, bool showHolo = true)
    {
        if (cardList.Count != 0)
            cardList[0].GetComponent<CardScript>().HideHologram();

        cardList.Insert(0, card);
        card.transform.SetParent(gameObject.transform);

        if (showHolo)
            cardList[0].GetComponent<CardScript>().ShowHologram();

        SetCardPositions();
    }

    public void RemoveCard(GameObject card, bool showHolo = true)
    {
        cardList.Remove(card);

        if (cardList.Count != 0)
        {
            if (cardList[0].gameObject.GetComponent<CardScript>().IsHidden)
                cardList[0].gameObject.GetComponent<CardScript>().SetFoundationVisibility(true);

            if (showHolo)
                cardList[0].GetComponent<CardScript>().ShowHologram();
        }

        SetCardPositions();
    }

    public void SetCardPositions()
    {
        int positionCounter = 0;
        int hiddenCards = 0;
        float yOffset = 0;

        for (int i = cardList.Count - 1; i >= 0; i--) // go backwards through the list
        {
            // as we go through, place cards above and in-front the previous one
            cardList[i].transform.position = gameObject.transform.position + new Vector3(0, yOffset, -1 - positionCounter * 0.1f);

            if (cardList[i].GetComponent<CardScript>().IsHidden)  // don't show hidden cards as much
            {
                hiddenCards++;
                if (cardList.Count > 18)
                    yOffset += 0.01f;
                else if (cardList.Count > 12)
                    yOffset += 0.05f;
                else if (cardList.Count > 10)
                    yOffset += 0.1f;
                else
                    yOffset += 0.2f;
            }
            else if (hiddenCards != 0 && cardList.Count > 17)
                yOffset += 0.42f;
            else
                yOffset += 0.45f;

            positionCounter++;
        }
    }

    public void GlowOn()
    {
        if (isGlowing)
            return;
        isGlowing = true;
        sp.color = Color.yellow;
    }

    public void GlowOff()
    {
        if (!isGlowing)
            return;
        isGlowing = false;
        sp.color = Color.white;
    }

    public bool IsGlowing()
    {
        return isGlowing;
    }

    public Color GetGlowColor()
    {
        return sp.color;
    }

    public void ProcessAction(GameObject input)
    {
        GameObject selectedCard = UtilsScript.Instance.selectedCards[0];
        CardScript selectedCardScript = selectedCard.GetComponent<CardScript>();

        if (input.CompareTag("Card"))
        {
            CardScript inputCardScript = input.GetComponent<CardScript>();

            if (UtilsScript.Instance.selectedCards.Count == 1)
            {
                if (CardTools.CanMatch(inputCardScript, selectedCardScript))
                {
                    UtilsScript.Instance.Match(input, selectedCard);
                    return;
                }
                else if (inputCardScript.container.CompareTag("Reactor"))
                {
                    if (!CardTools.IsSameSuit(input, selectedCard))
                        return;

                    SoundEffectsController.Instance.CardToReactorSound();
                    selectedCardScript.MoveCard(inputCardScript.container);
                    UtilsScript.Instance.UpdateActions(1, checkGameOver: true);
                    return;
                }
                else if (inputCardScript.container.CompareTag("Foundation"))
                {
                    if (inputCardScript.container.GetComponent<FoundationScript>().cardList[0] != input ||
                        inputCardScript.cardNum != selectedCardScript.cardNum + 1)
                        return;

                    SoundEffectsController.Instance.CardStackSound();
                    selectedCardScript.MoveCard(inputCardScript.container);
                }
                else
                    return;
            }
            else if (inputCardScript.container.CompareTag("Foundation"))
            {
                if (inputCardScript.container.GetComponent<FoundationScript>().cardList[0] != input ||
                    inputCardScript.cardNum != selectedCardScript.cardNum + 1)
                    return;

                SoundEffectsController.Instance.CardStackSound();

                for (int i = 0; i < UtilsScript.Instance.selectedCards.Count - 1; i++) //goes through and moves all selesctedCards to clicked location
                    UtilsScript.Instance.selectedCards[i].GetComponent<CardScript>().MoveCard(inputCardScript.container, isStack: true, showHolo: false);

                UtilsScript.Instance.selectedCards[UtilsScript.Instance.selectedCards.Count - 1].GetComponent<CardScript>().MoveCard(inputCardScript.container, isStack: true);
            }
            else
                return;
        }
        else if (input.CompareTag("Reactor"))
        {
            if (UtilsScript.Instance.selectedCards.Count != 1 || !CardTools.IsSameSuit(input, selectedCard))
                return;

            SoundEffectsController.Instance.CardToReactorSound();
            selectedCardScript.MoveCard(input);
            UtilsScript.Instance.UpdateActions(1, checkGameOver: true);
            return;
        }
        else if (input.CompareTag("Foundation"))
        {
            if (input.GetComponent<FoundationScript>().cardList.Count != 0)
                return;

            SoundEffectsController.Instance.CardStackSound();

            if (UtilsScript.Instance.selectedCards.Count == 1)
                selectedCardScript.MoveCard(input);
            else
            {
                for (int i = 0; i < UtilsScript.Instance.selectedCards.Count - 1; i++) //goes through and moves all selesctedCards to clicked location
                    UtilsScript.Instance.selectedCards[i].GetComponent<CardScript>().MoveCard(input, isStack: true, showHolo: false);

                UtilsScript.Instance.selectedCards[UtilsScript.Instance.selectedCards.Count - 1].GetComponent<CardScript>().MoveCard(input, isStack: true);
            }
        }
        else
            return;

        UtilsScript.Instance.UpdateActions(1);
    }
}
