using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationScript : MonoBehaviour
{
    public List<GameObject> cardList;
    public bool addCard;
    public bool removeCard;
    public GameObject cardToAdd;
    int counter;
    int cardMax;
    
    void Update()
    {
        if (removeCard == true)
        {
            if(cardList[0].gameObject.transform.parent = gameObject.transform)
            {
                cardList[0].gameObject.transform.parent = null;
            }
            
            cardList.Remove(cardList[0]);
            removeCard = false;
        }

        if (addCard == true && cardToAdd != null)
        {
            cardList.Insert(0, cardToAdd);
            addCard = false;
        }

        counter = 0;
        cardMax = cardList.Count;

        while(counter < cardMax)
        {
            Debug.Log("foo");
            Debug.Log("Max" + cardMax);
            Debug.Log("counter" + counter);
            cardList[counter].transform.parent = gameObject.transform;
            cardList[counter].transform.localPosition = new Vector3(0, -0.5f * counter, 0);
            cardList[counter].gameObject.GetComponent<SpriteRenderer>().sortingOrder = cardMax - counter;
            counter += 1;
        }
    }
}
