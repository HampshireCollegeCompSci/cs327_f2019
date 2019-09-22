using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactorScript : MonoBehaviour
{
    //helloWorld
    public List<GameObject> cardList;
    public UtilsScript utils = UtilsScript.global;
    int counter;
    int cardMax;
    int ReactorVal;

    
    
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

        //change later, just using so the code block isn't ugly
        //if the card numbers match
        if (utils.selectedCards[0].GetComponent<CardScript>().cardNum == utils.clcikedCard.GetComponent<CardScript>().cardNum && MatchSuit())
        {
            utils.Match();
           
        }
            
            //this will work once Max updates it to be public
        
        
        return;
        //if the cards match (check the attributes of the cards - you're only allowed to click top card
        //not sure how to compare to other card yet
        //then call match function
        

    }

    public bool MatchSuit()
    {
        //just to make it cleaner because this utils.blah blah blah is yucky
        //basically a string of if/else cases for matching
        string selectedCardSuit = utils.selectedCards[0].GetComponent<CardScript>().cardSuit;
        string clickedCardSuit = utils.clcikedCard.GetComponent<CardScript>().cardSuit;
        if (selectedCardSuit.Equals("hearts") && clickedCardSuit.Equals("diamonds"))
        {
            
            return true;
        }
        else if (selectedCardSuit.Equals("diamonds") && clickedCardSuit.Equals("hearts"))
        {
            return true;
        }
        else if (selectedCardSuit.Equals("spades") && clickedCardSuit.Equals("clubs"))
        {
            return true;
        }
        else if (selectedCardSuit.Equals("clubs") && clickedCardSuit.Equals("spades"))
        {
            return true;
        }
        else
        {
            return false;
        }
      

    }

    //TODO:
    //when there's like, 
    private int CountReactorCard()
    {
        //sum the values into totalSum, return
        int totalSum = 0;
        counter = 0; //index for tracking position in cardlist
        while (counter < cardMax)
        {
            //this is going to need to be changed because card list is not an array
            totalSum += cardList[counter].gameObject.GetComponent<CardScript>().cardNum;
        }

        return totalSum;
    }
}
