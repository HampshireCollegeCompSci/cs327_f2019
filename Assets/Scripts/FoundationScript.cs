using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationScript : MonoBehaviour
{
    public UtilsScript utils;
    public List<GameObject> cardList;
    int indexCounter;
    int positionCounter;
    int cardMax;

    void Start()
    {
        utils = UtilsScript.global;
        SetCardPositions();
    }

    void Update()
    {
        return;
    }

    public void RemoveCard(GameObject card)
    {
        cardList.Remove(card);
        checkTopCard();
    }

    public void checkTopCard()
    {
        if (cardList.Count != 0 && cardList[0].gameObject.GetComponent<CardScript>().hidden)
        {
            cardList[0].gameObject.GetComponent<CardScript>().hidden = false;
            cardList[0].gameObject.GetComponent<CardScript>().SetCardAppearance();
        }
    }

    //assigns card positions and render order and sets this foundation as the cards parents

        //iterate over the cardlist
        //for each one there, copy transform of foundation
        //apply to the card
        //offset card y axis by a little bit
        //offset card z axis by a little bit
    public void SetCardPositions()
    {
        positionCounter = 0;

        float yOffset = 0;
        for(indexCounter = cardList.Count - 1; indexCounter > -1; indexCounter--)
        {
            if (cardList[indexCounter].GetComponent<CardScript>().hidden)
            {
                cardList[indexCounter].transform.position = gameObject.transform.position + new Vector3(0, yOffset, -0.5f * positionCounter) + new Vector3(0, 0, -0.5f);
                if (cardList.Count > 10)
                {
                    yOffset -= Config.config.foundationStackDensity * 0.25f;
                }
                else
                {
                    yOffset -= Config.config.foundationStackDensity * 0.5f;
                }
            }
            else
            {
                cardList[indexCounter].transform.position = gameObject.transform.position + new Vector3(0, yOffset, -0.5f * positionCounter) + new Vector3(0, 0, -0.5f);
                yOffset -= Config.config.foundationStackDensity;
            }

            positionCounter += 1;
        }
    }

    public void ProcessAction(GameObject input)
    {
        Debug.Log("matchcheck");

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
                Debug.Log("matched");
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
    }
}
