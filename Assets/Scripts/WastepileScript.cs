using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WastepileScript : MonoBehaviour
{
    public UtilsScript utils;
    public List<GameObject> cardList;
    int counter;
    int cardMax;

    private void Start()
    {
        utils = UtilsScript.global;
    }


    void Update()
    {

    }

    public void RemoveCard(GameObject card)
    {
        cardList.Remove(card);
    }

    public void SetCardPositions()
    {
        int counter = 0;
        float xOffset = 0;
        if (false) // Config.config.onlyShowTopWastepileCards
        {
            for (int i = cardList.Count - 1; i > -1; i--)
            {
                cardList[i].transform.parent = gameObject.transform;
                if (i < 3) // Config.config.cardsAtTopOfWastePile
                {
                    cardList[i].transform.localPosition = new Vector3(xOffset, 0, counter * -0.1f);
                    xOffset += Config.config.foundationStackDensity;
                }
                else
                {
                    cardList[i].transform.localPosition = new Vector3(0, 0, counter * -0.1f);
                }
                counter++;
            }
        }
        else
        {
            for (int i = cardList.Count - 1; i > -1; i--)
            {
                cardList[i].transform.parent = gameObject.transform;
                cardList[i].transform.localPosition = new Vector3(xOffset, 0, counter * -0.1f);
                if (i < 3) // replace 3 with Config.config.cardsAtTopOfWastePile
                {
                    xOffset += Config.config.foundationStackDensity;
                }
                else
                {
                    xOffset += Config.config.foundationStackDensity * 0.25f;
                }
                counter++;
            }
        }
    }


    /*public void ProcessAction(GameObject input)
    {
        GameObject selectedCard = utils.selectedCards[0];
        // checking if utils.selectedCards only has the top card in the wastePile  
        if (utils.selectedCards.Count != 1 || selectedCard != cardList[0])
        {
            return;
        }

        if (input.CompareTag("Foundation") && input.GetComponent<FoundationScript>().cardList.Count == 0)
        {
            selectedCard.GetComponent<CardScript>().MoveCard(input);
        }
        else if (input.CompareTag("Reactor") && input.GetComponent<ReactorScript>().cardList.Count == 0)
        {
            selectedCard.GetComponent<CardScript>().MoveCard(input);
        }
        else if (input.CompareTag("Card"))
        {
            GameObject cardContainer = input.GetComponent<CardScript>().container;
            if (cardContainer.CompareTag("Reactor"))
            {
                if (utils.IsMatch(input, selectedCard))
                {
                    utils.Match(input, selectedCard);
                }
            }
            else if (cardContainer.CompareTag("Foundation"))
            {
                // is input the top card in the foundation
                if (input == cardContainer.GetComponent<FoundationScript>().cardList[0])
                {
                    if (utils.IsMatch(input, selectedCard))
                    {
                        utils.Match(input, selectedCard);
                    }
                    else if ((selectedCard.GetComponent<CardScript>().cardNum + 1) == input.GetComponent<CardScript>().cardNum)
                    {
                        foreach (GameObject card in utils.selectedCards) //goes through and moves all selesctedCards to clicked location
                        {
                            card.GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container);
                        }
                    }
                }
            }
        }
        return;
    }*/

    public void ProcessAction(GameObject input)
    {
        if (!input.CompareTag("Card"))
        {
            if ((input.CompareTag("Foundation") || input.CompareTag("Reactor")) && utils.selectedCards.Count != 0)
            {
                foreach (GameObject card in utils.selectedCards) //goes through and moves all selesctedCards to clicked location
                {
                    card.GetComponent<CardScript>().MoveCard(input);
                }
            }
        }

        else if (utils.IsMatch(input, utils.selectedCards[0]) && utils.selectedCards.Count == 1) //check if selectedCards and the input card match and that selesctedCards is only one card
        {
            utils.Match(input, utils.selectedCards[0]); //removes the two matched cards
        }

        else if ((utils.selectedCards[0].GetComponent<CardScript>().cardNum + 1) == input.GetComponent<CardScript>().cardNum && input.GetComponent<CardScript>().container.CompareTag("Foundation"))
        {
            foreach (GameObject card in utils.selectedCards) //goes through and moves all selesctedCards to clicked location
            {
                card.GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container);
            }
        }

        else if (input.GetComponent<CardScript>().container.CompareTag("Reactor") && utils.IsSameSuit(input, utils.selectedCards[0]) && utils.selectedCards.Count == 1)
        {
            utils.selectedCards[0].GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container);
        }

        return;
    }
    public List<GameObject> GetCardList()
    {
        return cardList;
    }
}
