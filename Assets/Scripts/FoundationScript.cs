using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationScript : MonoBehaviour
{
    public List<GameObject> cardList;
    public GameObject cardToAdd;
    int counter;
    int cardMax;
    int locationInList;

    public void AddCard(GameObject cardToAdd, int locationInList)
    {
        cardList.Insert(locationInList, cardToAdd);
    }

    public void RemoveCard(int locationInList)
    {
        if (cardList[locationInList].gameObject.transform.parent = gameObject.transform)
        {
            cardList[locationInList].gameObject.transform.parent = null;
        }

        cardList.Remove(cardList[locationInList]);
    }

    void Update()
    {
        counter = 0;
        cardMax = cardList.Count;

        while(counter < cardMax)
        {
            cardList[counter].transform.parent = gameObject.transform;
            cardList[counter].transform.localPosition = new Vector3(0, -0.5f * counter, 0);
            cardList[counter].gameObject.GetComponent<SpriteRenderer>().sortingOrder = cardMax - counter;
            counter += 1;
        }
    }
}
