using System.Collections.Generic;
using UnityEngine;

public class DeckOrientation : MonoBehaviour
{
    // Singleton instance.
    public static DeckOrientation Instance { get; private set; }

    [SerializeField]
    private Transform conveyorSystem, deckCounter;
    bool _flip;

    // Initialize the singleton instance.
    private void Awake()
    {
        if (Instance != null)
        {
            throw new System.ArgumentException("there should not already be an instance of this");
        }

        Instance = this;
    }

    private void Start()
    {
        _flip = true;
        Flip = PersistentSettings.DeckOrientation;
    }

    public bool Flip
    {
        get => _flip;
        set
        {
            if (_flip == value) return;
            _flip = value;
            Vector3 scale = Vector3.one;
            if (!value)
                scale.x = -1;
            conveyorSystem.transform.localScale = scale;
            deckCounter.localScale = scale;

            FlipCards(WastepileScript.Instance.CardList);
            FlipCards(DeckScript.Instance.CardList);
        }
    }

    private void FlipCards(List<GameObject> cardList)
    {
        if (cardList.Count == 0) return;
        Vector3 scale = cardList[0].transform.localScale;
        scale.x = -scale.x;
        foreach (GameObject card in cardList)
        {
            card.GetComponent<Transform>().localScale = scale;
        }
    }
}
