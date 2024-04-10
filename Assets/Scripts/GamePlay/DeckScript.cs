using System.Collections.Generic;
using UnityEngine;

public class DeckScript : MonoBehaviour, ICardContainer
{
    // Singleton instance.
    public static DeckScript Instance { get; private set; }

    [SerializeField]
    private List<GameObject> cardList;

    public DeckScript()
    {
        cardList = new(GameValues.GamePlay.cardCount);
    }

    // Initialize the singleton instance.
    void Awake()
    {
        if (Instance != null)
        {
            throw new System.ArgumentException("there should not already be an instance of this");
        }

        Instance = this;
    }

    public Constants.CardContainerType ContainerType => Constants.CardContainerType.Deck;

    public List<GameObject> CardList => cardList;

    public void AddCard(GameObject card)
    {
        cardList.Add(card);
        card.transform.SetParent(gameObject.transform);
        card.transform.localPosition = Vector3.zero;
        card.GetComponent<CardScript>().Enabled = false;
    }

    public void RemoveCard(GameObject card)
    {
        cardList.RemoveAt(cardList.LastIndexOf(card));

        card.GetComponent<CardScript>().Enabled = true;
    }

    public void Deal(bool doLog = true)
    {
        int numberOfCardsToDeal = Mathf.Min(GameValues.GamePlay.cardsToDeal, cardList.Count);
        if (numberOfCardsToDeal == 0) return;
        List<GameObject> toMoveList = new(numberOfCardsToDeal);
        // loop backwards through the list
        for (int i = 1; i <= numberOfCardsToDeal; i++)
        {
            toMoveList.Add(cardList[^i]);
        }
        
        if (Config.Instance.TutorialOn)
        {
            doLog = false;
        }
        WastepileScript.Instance.AddCards(toMoveList, doLog);
    }

    public void TryDealing()
    {
        if (cardList.Count != 0) // can the deck can be drawn from
        {
            SoundEffectsController.Instance.DeckDeal();
            Deal();
        }
        // if it is possible to repopulate the deck
        else if (WastepileScript.Instance.CardList.Count > GameValues.GamePlay.cardsToDeal)
        {
            // moves all wastePile cards into the deck
            WastepileScript.Instance.StartDeckReset();
            AchievementsManager.FailedNoDeckFlip();
            SoundEffectsController.Instance.DeckReshuffle();
        }
        else
        {
            // nothing is in the deck
            DeckButtonScript.Instance.StartButtonUp();
        }
    }
}
