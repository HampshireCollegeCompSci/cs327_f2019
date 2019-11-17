using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchedPileScript : MonoBehaviour
{
    public List<GameObject> cardList;

    public void AddCard(GameObject card)
    {
        cardList.Insert(0, card);
        card.transform.SetParent(gameObject.transform);
        card.transform.localPosition = Vector3.zero;
        card.SetActive(false);
    }

    public void RemoveCard(GameObject card)
    {
        cardList.Remove(card);
        card.SetActive(true);
    }

    //assigns card positions and render order and sets this foundation as the cards parents
    public void SetCardPositions()
    {
        //int counter = 0;
        //for (int i = cardList.Count - 1; i > -1; i--)
        //{
        //    cardList[i].transform.parent = gameObject.transform;
        //    cardList[i].transform.localPosition = new Vector3(0, -0.5f * counter, -0.1f * counter);
        //    counter++;
        //}
    }
}
