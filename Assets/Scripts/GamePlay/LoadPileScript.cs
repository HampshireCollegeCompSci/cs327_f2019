using System.Collections.Generic;
using UnityEngine;

public class LoadPileScript : MonoBehaviour
{
    public List<GameObject> cardList;

    // Singleton instance.
    public static LoadPileScript Instance = null;

    // Initialize the singleton instance.
    private void Awake()
    {
        // If there is not already an instance of SoundManager, set it to this.
        if (Instance == null)
        {
            Instance = this;
        }
        //If an instance already exists, destroy whatever this object is to enforce the singleton.
        else if (Instance != this)
        {
            throw new System.ArgumentException("there should not already be an instance of this");
        }
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
