using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationScript : MonoBehaviour
{
    public UtilsScript utils;
    public List<GameObject> cardList;
    int counter;
    int cardMax;

    void Start()
    {
        utils = UtilsScript.global;
        SetCardPositions();
    }

    void Update()
    {

    }

    public void RemoveCard(GameObject card)
    {
        cardList.Remove(card);
    }

    //assigns card positions and render order and sets this foundation as the cards parents

        //iterate over the cardlist
        //for each one there, copy transform of foundation
        //apply to the card
        //offset card y axis by a little bit
        //offset card z axis by a little bit
    public void SetCardPositions()
    {
        counter = 0;
        cardMax = cardList.Count;

        while (counter < cardMax)
        {
            
            cardList[counter].transform.position =  gameObject.transform.position + new Vector3(0, 0.5f * counter, 0.5f * counter);
            cardList[counter].gameObject.GetComponent<SpriteRenderer>().sortingOrder = cardMax - counter;

            counter += 1;
        }
    }

    public void Clicked(GameObject input)
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
        else if ((utils.selectedCards[0].GetComponent<CardScript>().cardNum + 1) == input.GetComponent<CardScript>().cardNum)
        {
            foreach (GameObject card in utils.selectedCards) //goes through and moves all selesctedCards to clicked location
                {
                    card.GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container);
                }
        }
    }
}
