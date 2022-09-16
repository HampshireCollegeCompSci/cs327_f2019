﻿using System.Collections.Generic;
using UnityEngine;

public class MatchedPileScript : MonoBehaviour, ICardContainer
{
    // Singleton instance.
    public static MatchedPileScript Instance;

    [SerializeField]
    private List<GameObject> cardList;

    public MatchedPileScript()
    {
        cardList = new();
    }

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

    public List<GameObject> CardList
    {
        get => cardList;
    }

    public void AddCard(GameObject card)
    {
        cardList.Insert(0, card);
        card.transform.SetParent(gameObject.transform);
        card.transform.localPosition = Vector3.zero;
        card.GetComponent<CardScript>().Enabled = false;
    }

    public void RemoveCard(GameObject card)
    {
        card.GetComponent<CardScript>().Enabled = true;
        cardList.Remove(card);
    }
}
