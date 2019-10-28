using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WastepileScript : MonoBehaviour
{
    public UtilsScript utils;
    public List<GameObject> cardList;
    public SoundController soundController;
    int counter;
    int cardMax;

    private void Start()
    {
        utils = UtilsScript.global;
    }


    void Update()
    {
        return;
    }

    public void RemoveCard(GameObject card)
    {
        cardList.Remove(card);
    }

    public void SetCardPositions()
    {
        int positionCounter = 0;
        float xOffset = 0;
    
        for (int i = 0; i < cardList.Count; i++)  // go through the list
        {
            // as we go through, place cards to the right and behind of the previous one
            cardList[i].transform.position = gameObject.transform.position + new Vector3(xOffset, 0, positionCounter * 0.1f);

            //if (counter <= Config.config.wastepileCardsToShow)
            //{
            //    xOffset += 0.8f;
            //}
            xOffset += 0.8f;

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

                if (input.CompareTag("Foundation") && input.GetComponent<FoundationScript>().cardList.Count == 0)
                {
                    foreach (GameObject card in utils.selectedCards) //goes through and moves all selesctedCards to clicked location
                    {
                        card.GetComponent<CardScript>().MoveCard(input);
                    }
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
            }
        }

        else if ((utils.selectedCards[0].GetComponent<CardScript>().cardNum + 1) == input.GetComponent<CardScript>().cardNum && input.GetComponent<CardScript>().container.CompareTag("Foundation"))
        {
            soundController.CardStackSound();
            foreach (GameObject card in utils.selectedCards) //goes through and moves all selesctedCards to clicked location
            {
                card.GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container);
            }
        }

        else if (input.GetComponent<CardScript>().container.CompareTag("Reactor") && utils.IsSameSuit(input, utils.selectedCards[0]) && utils.selectedCards.Count == 1)
        {
            soundController.CardToReactorSound();
            utils.selectedCards[0].GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container);
        }

        return;
    }
    public List<GameObject> GetCardList()
    {
        return cardList;
    }
}
