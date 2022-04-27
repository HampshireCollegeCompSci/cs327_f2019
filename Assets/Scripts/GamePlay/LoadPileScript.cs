using System.Collections.Generic;
using UnityEngine;

public class LoadPileScript : MonoBehaviour, ICardContainer
{
    public List<GameObject> cardList;

    // Singleton instance.
    public static LoadPileScript Instance = null;

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
        cardList.Add(card);
    }

    public void RemoveCard(GameObject card)
    {
        cardList.Remove(card);
    }
}
