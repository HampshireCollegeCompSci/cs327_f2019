using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactorScript : MonoBehaviour
{
    //helloWorld
    public List<GameObject> cardList;
    public UtilsScript utils;
    int counter;
    int cardMax;
    int ReactorVal;

    void Start()
    {
        utils = UtilsScript.global;
    }


    void Update()
    {
        //constantly checking to see if reactor score is below
        if (CountReactorCard() >= 18)
        {
            //TODO: game over


        }
    }

    public void SetCardPositions()
    {
        counter = 0;
        cardMax = cardList.Count;

        while (counter < cardMax)
        {
            cardList[counter].transform.parent = gameObject.transform;
            cardList[counter].transform.localPosition = new Vector3(0, 0.5f * counter, 0);
            cardList[counter].gameObject.GetComponent<SpriteRenderer>().sortingOrder = cardMax - counter;
            counter += 1;
        }
    }

    //this function is getting called when clicked
    public void Clicked()
    {

        //this is the double clicking part in theory? Jona said it was how he
        //compared for double clicking so if it's wrong my bad
        //I did modify the index though because I thought it's the first
        //if it's not the first index, I will change it back
        if (utils.cilckedCard == cardList[0])
        {
            //insert the clicked card into the list because in theory that's what we want?
            //unless it's selected card, honestly not sure
            cardList.Insert(0, utils.cilckedCard);
            //this was Jona's code initially, I'm just leaving it in case I forget wtf I'm doing
            //in theory make it null, e.g. deselecting? 
            utils.cilckedCard = null;
        }
        else // if not double clicked, they aren't adding, whcih means they are trying to match
        {
            //if the card numbers match and the suits match (MatchSuit)
            if (utils.selectedCards[0].GetComponent<CardScript>().cardNum ==
                utils.cilckedCard.GetComponent<CardScript>().cardNum && utils.MatchSuit())
            {
                utils.Match();

            }
        }
        //this is just the return call to end after having clicked
        return;



    }

    //this will need to be changed once I know what the actual string names are 


    //this is just meant to iterate through the list of cards in the stack
    //sum the amounts of them, and then return whatever that sum is
    //in order to be used in the update function
    //basically just in case it goes over 18, in which case end game
    private int CountReactorCard()
    {
        //sum the values into totalSum, return
        int totalSum = 0;
        counter = 0; //index for tracking position in cardlist
        while (counter < cardMax)
        {
            //this is going to need to be changed because card list is not an array according to
            //I think Ian? 
            totalSum += cardList[counter].gameObject.GetComponent<CardScript>().cardNum;
        }

        return totalSum;
    }
}
