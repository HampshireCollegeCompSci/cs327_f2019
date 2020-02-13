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
        card.GetComponent<CardScript>().HideHologram();
        card.GetComponent<SpriteRenderer>().enabled = false;
        card.GetComponent<BoxCollider2D>().enabled = false;
    }

    public void RemoveCard(GameObject card)
    {
        card.GetComponent<SpriteRenderer>().enabled = true;
        card.GetComponent<BoxCollider2D>().enabled = true;
        cardList.Remove(card);
    }
}
