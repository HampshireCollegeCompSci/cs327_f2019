using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchedPileScript : MonoBehaviour
{
    public List<GameObject> cardList;
    int counter;
    int cardMax;

    void Update()
    {
        return;
    }

    public void RemoveCard(GameObject card)
    {
        cardList.Remove(card);
    }

    //assigns card positions and render order and sets this foundation as the cards parents
    public void SetCardPositions()
    {
        for (int i = cardList.Count - 1; i > -1; i--)
        {
            cardList[i].transform.parent = gameObject.transform;
            cardList[i].transform.localPosition = new Vector3(0, -0.5f * counter, -0.1f * counter);
            counter++;
        }
    }
}
