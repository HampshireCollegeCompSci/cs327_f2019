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

    //assigns card positions and render order and sets this foundation as the cards parents
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
         if (utils.selectedCards[0].GetComponent<CardScript>().container.CompareTag("Foundation")) //checking where selected card is from
        {
            if (utils.MatchSuit() && utils.selectedCards.Count == 1) //check if selectedCards and the clicked card match and that selesctedCards is only one card
            {
                utils.Match(); //removes the two matched cards
            }

            else if ((cardList[0].GetComponent<CardScript>().cardNum - 1) == utils.selectedCards[0].GetComponent<CardScript>().cardNum) //checks card destination is one higher
            {
                foreach (GameObject card in utils.selectedCards) //goes through and moves all selesctedCards to clicked location
                {
                    card.GetComponent<CardScript>().MoveCard(cardList[0].GetComponent<CardScript>().container);
                }
            }
        }

        else if (utils.selectedCards[0].GetComponent<CardScript>().container.CompareTag("Reactor") && utils.selectedCards.Count == 1) //checking if selectedcards is from a reactor and that it is only one card
        {
            if (utils.MatchSuit()) //check if selectedCards and the clicked card match
            {
                utils.Match(); //removes the two matched cards
            }
        }

        else if (utils.selectedCards[0].GetComponent<CardScript>().container.CompareTag("Wastepile") && utils.selectedCards.Count == 1) //checking if selectedcards is from a wastepile and that it is only one card
        {
            if (utils.MatchSuit()) //check if selectedCards and the clicked card match
            {
                utils.Match(); //removes the two matched cards
            }

            else if ((cardList[0].GetComponent<CardScript>().cardNum - 1) == utils.selectedCards[0].GetComponent<CardScript>().cardNum) //checks card destination is one higher
            {
                utils.selectedCards[0].GetComponent<CardScript>().MoveCard(cardList[0].GetComponent<CardScript>().container); //moves card in selectedCards to clicked location
            }
        }
        return;
    }

}
