using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchedCardsScript : MonoBehaviour
{
    public List<GameObject> cardList1;
    public List<GameObject> cardList2;
    int counter;
    int cardMax;

    private void Update()
    {
        SetCardPositions();
    }

    public void SetCardPositions()
    {
        counter = 0;
        cardMax = cardList1.Count;

        while (counter < cardMax)
        {
            cardList1[counter].transform.parent = gameObject.transform;
            cardList1[counter].transform.localPosition = new Vector3(0, 0.5f * counter, 0);
            cardList1[counter].gameObject.GetComponent<SpriteRenderer>().sortingOrder = cardMax - counter;
            counter += 1;
        }

        counter = 0;
        cardMax = cardList2.Count;

        while (counter < cardMax)
        {
            cardList2[counter].transform.parent = gameObject.transform;
            cardList2[counter].transform.localPosition = new Vector3(-1, 0.5f * counter, 0);
            cardList2[counter].gameObject.GetComponent<SpriteRenderer>().sortingOrder = cardMax - counter;
            counter += 1;
        }


    }
}
