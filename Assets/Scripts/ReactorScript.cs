using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactorScript : MonoBehaviour
{
    //helloWorld
    public List<GameObject> cardList;
    int counter;
    int cardMax;
    int ReactorVal;

    void Update()
    {
        
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

    public void Clicked()
    {
        if (CountReactorCard() < 18)
        {
            //TODO:
            
        }
        else
        {
            //TODO: game over
        }
        return;
        //on click begin reactor function
        //from utils
        //if the count is under 18
            //when you call Count Reactor Card
            //check if a match
            //if it's a match then do something? Unclear how to do
            //else don't need to do anything
        //else
            //game over

    }

    private int CountReactorCard()
    {
        //sum the values into totalSum, return
        int totalSum = 0;
        counter = 0; //index for tracking position in cardlist
        while (counter < cardMax)
        {
            //this is going to need to be changed because card list is not an array
            totalSum += cardList[counter].gameObject.GetComponent<CardScript>().cardVal;
        }

        return totalSum;
    }
}
