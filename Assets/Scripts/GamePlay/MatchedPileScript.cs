using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchedPileScript : MonoBehaviour
{
    public List<GameObject> cardList;

    private void Start()
    {
        Config.config.matches = gameObject;
    }
    public void AddCard(GameObject card)
    {
        cardList.Insert(0, card);
        card.transform.SetParent(gameObject.transform);
        card.transform.localPosition = Vector3.zero;
        card.GetComponent<CardScript>().SetGameplayVisibility(false);
    }

    public void RemoveCard(GameObject card)
    {
        card.GetComponent<CardScript>().SetGameplayVisibility(true);
        cardList.Remove(card);
    }
}
