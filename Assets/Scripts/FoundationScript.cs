using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationScript : MonoBehaviour
{
    private UtilsScript utils;
    public List<GameObject> cardList;
    public SoundController soundController;
    private SpriteRenderer sp;
    private bool isGlowing;

    void Start()
    {
        utils = UtilsScript.global;
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
            if (cardList[0].gameObject.GetComponent<CardScript>().isHidden())
                cardList[0].gameObject.GetComponent<CardScript>().SetVisibility(true);

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

            if (cardList[i].GetComponent<CardScript>().isHidden())  // don't show hidden cards as much
            {
                hiddenCards++;
                if (cardList.Count > 18)
                {
                    yOffset += 0.01f;
                }
                else if (cardList.Count > 12)
                {
                    yOffset += 0.05f;
                }
                else if (cardList.Count > 10)
                {
                    yOffset += 0.1f;
                }
                else
                {
                    yOffset += 0.2f;
                }
            }
            else if (hiddenCards > 0)
            {
                if (cardList.Count > 11)
                {
                    yOffset += 0.42f;
                }
                else
                {
                    yOffset += 0.45f;
                }
            }
            else
            {
                yOffset += 0.45f;
            }

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
        GameObject selectedCard = utils.selectedCards[0];
        CardScript selectedCardScript = selectedCard.GetComponent<CardScript>();

        if (input.CompareTag("Card"))
        {
            CardScript inputCardScript = input.GetComponent<CardScript>();

            if (utils.selectedCards.Count == 1)
            {
                if (utils.CanMatch(inputCardScript, selectedCardScript))
                {
                    utils.Match(input, selectedCard);
                    return;
                }
                else if (inputCardScript.container.CompareTag("Reactor"))
                {
                    if (!utils.IsSameSuit(input, selectedCard))
                        return;

                    soundController.CardToReactorSound();
                    selectedCardScript.MoveCard(inputCardScript.container);
                    utils.UpdateActions(1, checkGameOver: true);
                    return;
                }
                else if (inputCardScript.container.CompareTag("Foundation"))
                {
                    if (inputCardScript.container.GetComponent<FoundationScript>().cardList[0] != input ||
                        inputCardScript.cardNum != selectedCardScript.cardNum + 1)
                        return;

                    soundController.CardStackSound();
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

                soundController.CardStackSound();

                for (int i = 0; i < utils.selectedCards.Count - 1; i++) //goes through and moves all selesctedCards to clicked location
                    utils.selectedCards[i].GetComponent<CardScript>().MoveCard(inputCardScript.container, isStack: true, showHolo: false);

                utils.selectedCards[utils.selectedCards.Count - 1].GetComponent<CardScript>().MoveCard(inputCardScript.container, isStack: true);
            }
            else
                return;
        }
        else if (input.CompareTag("Reactor"))
        {
            if (utils.selectedCards.Count != 1 || !utils.IsSameSuit(input, selectedCard))
                return;

            soundController.CardToReactorSound();
            selectedCardScript.MoveCard(input);
            utils.UpdateActions(1, checkGameOver: true);
            return;
        }
        else if (input.CompareTag("Foundation"))
        {
            if (input.GetComponent<FoundationScript>().cardList.Count != 0)
                return;

            soundController.CardStackSound();

            if (utils.selectedCards.Count == 1)
                selectedCardScript.MoveCard(input);
            else
            {
                for (int i = 0; i < utils.selectedCards.Count - 1; i++) //goes through and moves all selesctedCards to clicked location
                    utils.selectedCards[i].GetComponent<CardScript>().MoveCard(input, isStack: true, showHolo: false);

                utils.selectedCards[utils.selectedCards.Count - 1].GetComponent<CardScript>().MoveCard(input, isStack: true);
            }
        }
        else
            return;

        utils.UpdateActions(1);
    }
}
