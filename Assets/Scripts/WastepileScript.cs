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

        counter = 0;
        /*cardMax = cardList.Count;

        while (counter < cardMax)
        {
            cardList[counter].transform.parent = gameObject.transform;
            cardList[counter].transform.localPosition = new Vector3(0, -0.5f * counter, 0);
            cardList[counter].gameObject.GetComponent<SpriteRenderer>().sortingOrder = cardMax - counter;
            counter += 1;
        }*/

        for (int i = cardList.Count - 1; i > -1; i--)
        {
            cardList[i].transform.parent = gameObject.transform;
            cardList[i].transform.localPosition = new Vector3(0, -0.5f * counter, -0.1f * counter);
            counter++;
        }
    }


    public void ProcessAction(GameObject input)
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
    }

    public List<GameObject> GetCardList()
    {
        return cardList;
    }
}
