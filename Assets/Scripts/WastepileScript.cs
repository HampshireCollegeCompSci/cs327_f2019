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
        int counter = 1;
        float xOffset = 0;
    
        for (int i = cardList.Count - 1; i > -1; i--)
        {
            cardList[i].transform.parent = gameObject.transform;
            cardList[i].transform.localPosition = new Vector3(xOffset, 0, counter * -0.1f);
            if (i < Config.config.cardsAtTopOfWastePile)
            {
                xOffset += Config.config.foundationStackDensity;
            }
            else
            {
                xOffset += Config.config.nonTopXOffset;
            }
            counter++;
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
                foreach (GameObject card in utils.selectedCards) //goes through and moves all selesctedCards to clicked location
                {
                    card.GetComponent<CardScript>().MoveCard(input);
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
