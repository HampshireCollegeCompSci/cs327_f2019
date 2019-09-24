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

    //this function is run on selected card's container
    //if click reactor then click other card,
    //click method gets run on container of first card clicked
    //know first card is from reactor
    //selectedCards = list of the currently selected cards
    //selectedCard[0] is the first card (from Wastepile)
    //check if has more than 1 card -> shouldn't 
    //DON'T USE CLICKED CARD
    //take input (inputCard)
    //that is the second card 
    public void Clicked(GameObject inputCard)
    {

        GameObject card1 = utils.selectedCards[0];

        //list needs to only be 1, something wrong if not
        if (utils.selectedCards.Count == 1)
        {
            if (utils.IsMatch(inputCard, card1))
            {
                utils.Match(inputCard, card1);
            }
            else
            {
                utils.selectedCards.Remove(card1);
            }
        }

        //this is just the return call to end after having clicked
        return;

    }
    public List<GameObject> GetCardList()
    {
        return cardList;
    }
}
