﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchedPileScript : MonoBehaviour
{
    public List<GameObject> cardList;

    // Singleton instance.
    public static MatchedPileScript Instance = null;

    // Initialize the singleton instance.
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            throw new System.ArgumentException("there should not already be an instance of this");
        }
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
