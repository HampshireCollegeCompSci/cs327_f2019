using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WastepileScript : MonoBehaviour
{
    public GameObject utils;
    public List<GameObject> cardList;
    int counter;
    int cardMax;

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
        // has the waste pile been selected twice?
        if (utils.GetComponent<UtilsScript>().clcikedCard == cardList[cardList.Count - 1])
        {
            utils.GetComponent<UtilsScript>().clcikedCard = null;
        }
        else // select the top of the waste pile
        {
            utils.GetComponent<UtilsScript>().clcikedCard = cardList[cardList.Count - 1];
        }
        return;
    }

    public List<GameObject> GetCardList()
    {
        return cardList;
    }
}
