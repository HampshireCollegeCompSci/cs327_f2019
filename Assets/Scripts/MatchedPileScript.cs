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
    public void AddCard(GameObject card, bool checkHolo = true)
    {
        card.GetComponent<CardScript>().HideHologram();
        cardList.Insert(0, card);
        card.transform.SetParent(gameObject.transform);
        card.transform.localPosition = Vector3.zero;
        //card.SetActive(false);
        card.GetComponent<SpriteRenderer>().enabled = false;
        card.GetComponent<BoxCollider2D>().enabled = false;
        //utils.CheckNextCycle(); since matching doesn't count to the action count atm
    }

    public void RemoveCard(GameObject card, bool checkHolo = false)
    {
        card.GetComponent<SpriteRenderer>().enabled = true;
        card.GetComponent<BoxCollider2D>().enabled = true;
        cardList.Remove(card);
        //card.SetActive(true);
    }
}
