using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadPileScript : MonoBehaviour
{
    public List<GameObject> cardList;

    private void Start()
    {
        Config.config.loadPile = gameObject;
    }
    public void AddCard(GameObject card, bool checkHolo = true)
    {
        card.GetComponent<CardScript>().HideHologram();
        cardList.Insert(0, card);
        card.transform.SetParent(gameObject.transform);
        card.transform.localPosition = Vector3.zero;
        card.SetActive(false);
    }

    public void RemoveCard(GameObject card, bool checkHolo = false)
    {
        cardList.Remove(card);
        card.SetActive(true);
    }
}
