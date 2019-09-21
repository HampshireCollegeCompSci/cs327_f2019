using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationScript : MonoBehaviour
{
    public List<GameObject> cardList;
    int counter;
    int cardMax;

    void Update()
    {
        SetCardPositions();
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
        return;
    }

}
