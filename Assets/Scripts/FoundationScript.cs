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
                if (cardList.Count > 12)
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

    public void ProcessAction(GameObject input)
    {
        if (!input.CompareTag("Card"))
        {
            if ((input.CompareTag("Foundation") || input.CompareTag("Reactor")) && utils.selectedCards.Count != 0)
            {
                if (input.CompareTag("Reactor"))
                {
                    if (utils.selectedCards.Count > 1 || utils.selectedCards[0].GetComponent<CardScript>().cardSuit != input.GetComponent<ReactorScript>().suit)
                    {
                        return;
                    }
                    utils.selectedCards[0].GetComponent<CardScript>().MoveCard(input);
                }
                else if (input.CompareTag("Foundation"))
                {
                    if (input.GetComponent<FoundationScript>().cardList.Count != 0)
                    {
                        return;
                    }

                    if (utils.selectedCards.Count > 1)
                    {
                        for (int i = 0; i < utils.selectedCards.Count - 1; i++) //goes through and moves all selesctedCards to clicked location
                        {
                            utils.selectedCards[i].GetComponent<CardScript>().MoveCard(input, isStack: true, removeUpdateHolo: false, addUpdateHolo: false);
                        }
                        utils.selectedCards[utils.selectedCards.Count - 1].GetComponent<CardScript>().MoveCard(input, isStack: true);
                    }
                    else
                    {
                        utils.selectedCards[0].GetComponent<CardScript>().MoveCard(input);
                    }
                }

                utils.UpdateActionCounter(1);
            }
        }
        else if (utils.IsMatch(input, utils.selectedCards[0]) && utils.selectedCards.Count == 1) //check if selectedCards and the input card match and that selesctedCards is only one card
        {
            GameObject inputContainer = input.GetComponent<CardScript>().container;

            if (inputContainer.CompareTag("Foundation"))
            {
                if (inputContainer.GetComponent<FoundationScript>().cardList[0] == input)
                {
                    utils.Match(input, utils.selectedCards[0]); //removes the two matched cards
                }

                return;
            }

            if (inputContainer.CompareTag("Reactor"))
            {
                if (inputContainer.GetComponent<ReactorScript>().cardList[0] == input)
                {
                    utils.Match(input, utils.selectedCards[0]); //removes the two matched cards
                }

                return;
            }

            if (inputContainer.CompareTag("Wastepile"))
            {
                if (inputContainer.GetComponent<WastepileScript>().cardList[0] == input)
                {
                    utils.Match(input, utils.selectedCards[0]); //removes the two matched cards
                }

                return;
            }

            else
            {
                utils.Match(input, utils.selectedCards[0]); //removes the two matched cards
            }
        }
        else if (input.GetComponent<CardScript>().container.CompareTag("Foundation") &&
            utils.selectedCards[0].GetComponent<CardScript>().cardNum + 1 == input.GetComponent<CardScript>().container.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().cardNum)
        {
            soundController.CardStackSound();
            if (utils.selectedCards.Count > 1)
            {
                GameObject inputContainer = input.GetComponent<CardScript>().container;
                for (int i = 0; i < utils.selectedCards.Count - 1; i++) //goes through and moves all selesctedCards to clicked location
                {
                    utils.selectedCards[i].GetComponent<CardScript>().MoveCard(inputContainer, isStack: true, removeUpdateHolo: false, addUpdateHolo: false);
                }
                utils.selectedCards[utils.selectedCards.Count - 1].GetComponent<CardScript>().MoveCard(inputContainer, isStack: true);
            }
            else
            {
                utils.selectedCards[0].GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container);
            }

            utils.UpdateActionCounter(1);
        }
        else if (input.GetComponent<CardScript>().container.CompareTag("Reactor") && utils.IsSameSuit(input, utils.selectedCards[0]) && utils.selectedCards.Count == 1)
        {
            utils.selectedCards[0].GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container);

            utils.UpdateActionCounter(1);
        }

        utils.CheckNextCycle();
        utils.CheckGameOver();
    }
}
