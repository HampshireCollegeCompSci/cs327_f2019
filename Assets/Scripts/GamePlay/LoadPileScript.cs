﻿using System.Collections.Generic;
using UnityEngine;

public class LoadPileScript : MonoBehaviour, ICardContainer
{
    // Singleton instance.
    public static LoadPileScript Instance;

    [SerializeField]
    private List<GameObject> cardList;

    public LoadPileScript()
    {
        cardList = new(52);
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
        cardList.Add(card);
    }

    public void RemoveCard(GameObject card)
    {
        cardList.RemoveAt(cardList.LastIndexOf(card));
    }
}
