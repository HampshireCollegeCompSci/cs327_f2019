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

    public void Clicked(GameObject inputCard)
    {
        if (utils.IsSuitMatch(inputCard, utils.selectedCards[0]) && utils.selectedCards.Count == 1) //check if selectedCards and the input card match and that selesctedCards is only one card
        {
            utils.Match(inputCard, utils.selectedCards[0]); //removes the two matched cards
        }
        else if ((utils.selectedCards[0].GetComponent<CardScript>().cardNum + 1) == inputCard.GetComponent<CardScript>().cardNum)
        {
            foreach (GameObject card in utils.selectedCards) //goes through and moves all selesctedCards to clicked location
                {
                    card.GetComponent<CardScript>().MoveCard(inputCard.GetComponent<CardScript>().container);
                }
        }
    }
}
