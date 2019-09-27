using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactorScript : MonoBehaviour
{
    //helloWorld
    public List<GameObject> cardList;
    public UtilsScript utils;
    int positionCounter;
    int cardMax;
    int ReactorVal;

    void Start()
    {
        //because typing is yucky :)
        utils = UtilsScript.global;
    }


    void Update()
    {
        //constantly checking to see if reactor score is below
        if (CountReactorCard() >= 18)
        {
            //TODO: game over


        }
        SetCardPositions();
    }


    
    public void RemoveCard(GameObject card)
    {
        cardList.Remove(card);
    }

    //iterate over cardList (in Reactor)
    //set their location positions
    //in other words, when a card is added to the Reactor
    //this handles positions
    //public void SetCardPositions()
    //{
    //    positionCounter = 0;

    //    for(int indexCounter = cardList.Count - 1; indexCounter > -1; indexCounter--)
    //    {
    //        cardList[indexCounter].transform.position = gameObject.transform.position + new Vector3(0, 0, -0.5f * positionCounter) + new Vector3(0, 0, -0.5f);

    //        positionCounter += 1;
    //    }
    //}

    public void SetCardPositions()
    {
        positionCounter = 0;

        for (int indexCounter = cardList.Count - 1; indexCounter > -1; indexCounter--)
        {
            cardList[indexCounter].transform.position = gameObject.transform.position + new Vector3(0, -Config.config.foundationStackDensity * positionCounter, -0.5f * positionCounter) + new Vector3(0, 0, -0.5f);

            positionCounter += 1;
        }
    }



    //this function is run on selected card's container
    //if click reactor then click other card,
    //click method gets run on container of first card clicked
    //know first card is from reactor
    //selectedCards = list of the currently selected cards
    //selectedCard[0] is the first card (from Reactor)
    //check if has more than 1 card -> shouldn't 
    //DON'T USE CLICKED CARD
    //take input (inputCard), which is the second card
    //match with them if they match


    //TODO: rename this goddamn function and all the other
    public void ProcessAction(GameObject input)
    {

        GameObject card1 = utils.selectedCards[0];

        if (input.CompareTag("Card"))
        {
            //list needs to only be 1, something wrong if not -> skip to return
            if (utils.selectedCards.Count == 1)
            {
                if (utils.IsMatch(input, card1))
                {
                    utils.Match(input, card1);
                }
                else
                {
                    utils.selectedCards.Remove(card1);
                }
            }
        }


        //this is just the return call to end after having clicked
        return;



    }


    //this is just meant to iterate through the list of cards in the stack
    //sum the amounts of them, and then return whatever that sum is
    //in order to be used in the update function
    //basically just in case it goes over 18, in which case end game
    private int CountReactorCard()
    {
        //sum the values into totalSum, return
        int totalSum = 0;
        int cardListNum = cardList.Count;
        for (int i = 0; i < cardListNum; i++)
        {
            totalSum += cardList[i].gameObject.GetComponent<CardScript>().cardNum;
        }

        return totalSum;
    }
}
