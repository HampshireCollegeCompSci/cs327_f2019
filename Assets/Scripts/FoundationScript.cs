using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationScript : MonoBehaviour
{
    public GameObject Utils;
    public List<GameObject> cardList;
    int counter;
    int cardMax;

    void Start()
    {
        //TODO: have something that finds the Utils object and sets it
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
         if (Utils.GetComponent<UtilsScript>().selectedCards[0].GetComponent<CardScript>().container.CompareTag("Foundation")) //checking where selected card is from
        {
            if (Utils.GetComponent<UtilsScript>().isMatch(cardList[0], Utils.GetComponent<UtilsScript>().selectedCards[0]) && Utils.GetComponent<UtilsScript>().selectedCards.Count == 1) //check if selectedCards and the clicked card match and that selesctedCards is only one card
            {
                Utils.GetComponent<UtilsScript>().Match(cardList[0], Utils.GetComponent<UtilsScript>().selectedCards[0]); //removes the two matched cards
            }

            else if ((cardList[0].GetComponent<CardScript>().cardNum - 1) == Utils.GetComponent<UtilsScript>().selectedCards[0].GetComponent<CardScript>().cardNum) //checks card destination is one higher
            {
                foreach (GameObject card in Utils.GetComponent<UtilsScript>().selectedCards) //goes through and moves all selesctedCards to clicked location
                {
                    card.GetComponent<CardScript>().MoveCard(cardList[0].GetComponent<CardScript>().container);
                }
            }
        }

        else if (Utils.GetComponent<UtilsScript>().selectedCards[0].GetComponent<CardScript>().container.CompareTag("Reactor") && Utils.GetComponent<UtilsScript>().selectedCards.Count == 1) //checking if selectedcards is from a reactor and that it is only one card
        {
            if (Utils.GetComponent<UtilsScript>().isMatch(cardList[0], Utils.GetComponent<UtilsScript>().selectedCards[0])) //check if selectedCards and the clicked card match
            {
                Utils.GetComponent<UtilsScript>().Match(cardList[0], Utils.GetComponent<UtilsScript>().selectedCards[0]); //removes the two matched cards
            }
        }

        else if (Utils.GetComponent<UtilsScript>().selectedCards[0].GetComponent<CardScript>().container.CompareTag("Wastepile") && Utils.GetComponent<UtilsScript>().selectedCards.Count == 1) //checking if selectedcards is from a wastepile and that it is only one card
        {
            if (Utils.GetComponent<UtilsScript>().isMatch(cardList[0], Utils.GetComponent<UtilsScript>().selectedCards[0])) //check if selectedCards and the clicked card match
            {
                Utils.GetComponent<UtilsScript>().Match(cardList[0], Utils.GetComponent<UtilsScript>().selectedCards[0]); //removes the two matched cards
            }

            else if ((cardList[0].GetComponent<CardScript>().cardNum - 1) == Utils.GetComponent<UtilsScript>().selectedCards[0].GetComponent<CardScript>().cardNum) //checks card destination is one higher
            {
                Utils.GetComponent<UtilsScript>().selectedCards[0].GetComponent<CardScript>().MoveCard(cardList[0].GetComponent<CardScript>().container); //moves card in selectedCards to clicked location
            }
        }
        return;
    }

}
