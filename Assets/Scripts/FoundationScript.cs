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
        float yOffset = 0;

        for (int i = cardList.Count - 1; i >= 0; i--) // go backwards through the list
        {
            // as we go through, place cards above and in-front the previous one
            cardList[i].transform.position = gameObject.transform.position + new Vector3(0, yOffset, -1 - positionCounter * 0.1f);

            if (cardList[i].GetComponent<CardScript>().isHidden())  // don't show hidden cards as much
            {
                if (cardList.Count > 12) // especially if the stack is large
                {
                    yOffset += 0.05f;
                }
                else if (cardList.Count > 10) // less so if the stack is medium
                {
                    yOffset += 0.1f;
                }
                else
                {
                    yOffset += 0.2f;
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
                if (input.CompareTag("Reactor") && (utils.selectedCards.Count != 1 || utils.selectedCards[0].GetComponent<CardScript>().cardSuit != input.GetComponent<ReactorScript>().suit))
                {
                    return;
                }

                if (input.CompareTag("Reactor") || input.CompareTag("Foundation") && input.GetComponent<FoundationScript>().cardList.Count == 0)
                {
                    if (input.TryGetComponent(typeof(CardScript), out Component throwaway))
                    {
                        utils.selectedCards[0].GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container);
                        for (int i = 1; i < utils.selectedCards.Count; i++) //goes through and moves all selesctedCards to clicked location
                        {
                            utils.selectedCards[i].GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container, true, false);
                        }
                    }
                    else {
                        foreach (GameObject card in utils.selectedCards)
                        {
                            card.GetComponent<CardScript>().MoveCard(input);
                        }
                    }

                    Config.config.actions += 1; //adds to the action count
                }
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
                Debug.Log("matched");
            }
        }
        else if (input.GetComponent<CardScript>().container.CompareTag("Foundation") &&
            utils.selectedCards[0].GetComponent<CardScript>().cardNum + 1 == input.GetComponent<CardScript>().container.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().cardNum)
        {
            soundController.CardStackSound();
            if (utils.selectedCards.Count > 1)
            {
                Debug.Log(utils.selectedCards.Count + " card stack move");
                for (int i = 0; i < utils.selectedCards.Count - 1; i++) //goes through and moves all selesctedCards to clicked location
                {
                    utils.selectedCards[i].GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container, isAction: false, removeUpdateHolo: false, addUpdateHolo: false);
                }
                utils.selectedCards[utils.selectedCards.Count - 1].GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container, isAction: false);
            }
            else
            {
                Debug.Log(utils.selectedCards.Count + " this should be 1");
                utils.selectedCards[0].GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container);
            }

            Config.config.actions += 1; //adds to the action count
        }
        else if (input.GetComponent<CardScript>().container.CompareTag("Reactor") && utils.IsSameSuit(input, utils.selectedCards[0]) && utils.selectedCards.Count == 1)
        {
            utils.selectedCards[0].GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container);

            Config.config.actions += 1; //adds to the action count
        }
    }
}
