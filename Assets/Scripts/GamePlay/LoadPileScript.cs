using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadPileScript : MonoBehaviour
{
    public List<GameObject> cardList;

    private void Start()
    {
        Config.config.loadPile = this.gameObject;
    }

    public void AddCard(GameObject card)
    {
        //card.GetComponent<CardScript>().HideHologram();
        cardList.Add(card);
    }

    public void RemoveCard(GameObject card)
    {
        cardList.Remove(card);
    }
}
