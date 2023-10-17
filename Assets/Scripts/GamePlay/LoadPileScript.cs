using System.Collections.Generic;
using UnityEngine;

public class LoadPileScript : MonoBehaviour, ICardContainer
{
    // Singleton instance.
    public static LoadPileScript Instance { get; private set; }

    [SerializeField]
    private List<GameObject> cardList;

    public LoadPileScript()
    {
        cardList = new(GameValues.GamePlay.cardCount);
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

    public List<GameObject> CardList => cardList;

    public void AddCard(GameObject card)
    {
        cardList.Add(card);
        card.transform.SetParent(gameObject.transform);
    }

    public void RemoveCard(GameObject card)
    {
        cardList.RemoveAt(cardList.LastIndexOf(card));
    }
}
