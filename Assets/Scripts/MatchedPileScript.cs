using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchedPileScript : MonoBehaviour
{
    public List<GameObject> cardList;
    public UtilsScript utils;

    private void Start()
    {
        Config.config.matches = gameObject;
        utils = UtilsScript.global;
    }
    public void AddCard(GameObject card, bool checkHolo = true)
    {
        card.GetComponent<CardScript>().HideHologram();
        cardList.Insert(0, card);
        card.transform.SetParent(gameObject.transform);
        card.transform.localPosition = Vector3.zero;
        card.SetActive(false);
        //utils.CheckNextCycle(); since matching doesn't count to the action count atm
        utils.CheckGameOver();
    }

    public void RemoveCard(GameObject card, bool checkHolo = false)
    {
        cardList.Remove(card);
        card.SetActive(true);
    }
}
