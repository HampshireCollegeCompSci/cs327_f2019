using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationScript : MonoBehaviour
{
    public UtilsScript utils;
    public List<GameObject> cardList;
    public SoundController soundController;

    void Start()
    {
        utils = UtilsScript.global;
    }

    public void CheckHologram(bool tryHidingBeneath)
    {
        if (cardList.Count != 0)
        {
            cardList[0].gameObject.GetComponent<CardScript>().ShowHologram();

            if (tryHidingBeneath)
            {
                for (int i = 1; i < cardList.Count; i++)
                {
                    if (cardList[i].GetComponent<CardScript>().HideHologram())
                    {
                        return;
                    }
                }
            }
        }
    }

    public void AddCard(GameObject card, bool checkHolo = true)
    {
        cardList.Insert(0, card);
        card.transform.SetParent(gameObject.transform);

        if (checkHolo)
        {
            CheckHologram(true);
        }

        SetCardPositions();
    }

    public void RemoveCard(GameObject card, bool checkHolo = true)
    {
        cardList.Remove(card);

        if (checkHolo)
        {
            CheckTopCard();
            CheckHologram(false);
        }

        SetCardPositions();
    }

    public void CheckTopCard()
    {
        if (cardList.Count != 0 && cardList[0].gameObject.GetComponent<CardScript>().isHidden())
        {
            cardList[0].gameObject.GetComponent<CardScript>().SetVisibility(true);
            //Config.config.GetComponent<SoundController>().CardRevealSound();
        }
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
                    yOffset += 0.37f;
                }
                else
                {
                    yOffset += 0.42f;
                }
            }
            else
            {
                if (cardList.Count > 11)
                {
                    yOffset += 0.38f;
                }

                else
                {
                    yOffset += 0.43f;
                }
            }

            positionCounter++;
        }
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
                    utils.Match(input, selectedCard);
                else if (inputCardScript.container.CompareTag("Reactor"))
                {
                    if (!utils.IsSameSuit(input, selectedCard))
                        return;

                    soundController.CardToReactorSound();
                    selectedCardScript.MoveCard(inputCardScript.container);
                    utils.UpdateActionCounter(1);
                }
                else if (inputCardScript.container.CompareTag("Foundation"))
                {
                    if (inputCardScript.container.GetComponent<FoundationScript>().cardList[0] != input ||
                        inputCardScript.cardNum != selectedCardScript.cardNum + 1)
                        return;

                    soundController.CardStackSound();
                    selectedCardScript.MoveCard(inputCardScript.container);
                    utils.UpdateActionCounter(1);
                }
            }
            else if (inputCardScript.container.CompareTag("Foundation"))
            {
                if (inputCardScript.container.GetComponent<FoundationScript>().cardList[0] != input ||
                    inputCardScript.cardNum != selectedCardScript.cardNum + 1)
                    return;

                soundController.CardStackSound();

                for (int i = 0; i < utils.selectedCards.Count - 1; i++) //goes through and moves all selesctedCards to clicked location
                    utils.selectedCards[i].GetComponent<CardScript>().MoveCard(inputCardScript.container, isStack: true, removeUpdateHolo: false, addUpdateHolo: false);
                utils.selectedCards[utils.selectedCards.Count - 1].GetComponent<CardScript>().MoveCard(inputCardScript.container, isStack: true);

                utils.UpdateActionCounter(1);
            }
        }
        else if (input.CompareTag("Reactor"))
        {
            if (utils.selectedCards.Count != 1 || !utils.IsSameSuit(input, selectedCard))
                return;

            soundController.CardToReactorSound();
            selectedCardScript.MoveCard(input);
            utils.UpdateActionCounter(1);
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
                    utils.selectedCards[i].GetComponent<CardScript>().MoveCard(input, isStack: true, removeUpdateHolo: false, addUpdateHolo: false);
                utils.selectedCards[utils.selectedCards.Count - 1].GetComponent<CardScript>().MoveCard(input, isStack: true);
            }

            utils.UpdateActionCounter(1);
        }

        utils.CheckNextCycle();
        utils.CheckGameOver();
    }
}
