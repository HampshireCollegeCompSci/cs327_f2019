using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WastepileScript : MonoBehaviour, ICardContainer
{
    // Singleton instance.
    public static WastepileScript Instance { get; private set; }
    private static readonly WaitForSeconds deckResetDelay = new(0.5f);

    [SerializeField]
    private List<GameObject> cardList;

    [SerializeField]
    private GameObject cardContainerPrefab;
    private List<GameObject> cardContainers;

    [SerializeField]
    private GameObject contentPanel;
    private float cardSpacing;
    private ScrollRect scrollRect;
    private RectTransform contentRectTransform;

    [SerializeField]
    private bool _scrolling, _draggingCard;

    // Initialize the singleton instance.
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            cardList = new List<GameObject>(GameValues.GamePlay.cardCount);
            cardContainers = new List<GameObject>(GameValues.GamePlay.cardCount);

            cardSpacing = contentPanel.GetComponent<HorizontalLayoutGroup>().spacing + 
                cardContainerPrefab.GetComponent<RectTransform>().sizeDelta.x;
            scrollRect = this.gameObject.GetComponent<ScrollRect>();
            contentRectTransform = contentPanel.GetComponent<RectTransform>();
        }
        else if (Instance != this)
        {
            throw new System.ArgumentException("there should not already be an instance of this");
        }
    }

    public List<GameObject> CardList => cardList;

    public bool DraggingCard
    {
        get => _draggingCard;
        set
        {
            _draggingCard = value;
            if (value)
            {
                scrollRect.horizontal = false;
                //scrollRect.horizontalScrollbar.interactable = false;
            }
            else
            {
                scrollRect.horizontal = true;
                //scrollRect.horizontalScrollbar.interactable = true;
            }
        }
    }

    public void AddCards(List<GameObject> cards, bool doLog = true)
    {
        bool wastePileWasEmpty = false;
        if (cardList.Count != 0) // hide the current top token now
        {
            CardScript cardScript = cardList[^1].GetComponent<CardScript>();
            cardScript.Hologram = false;
            cardScript.Obstructed = true;
        }
        else
        {
            wastePileWasEmpty = true;
        }

        // add the new cards, for the non-top cards: don't try to show their hologram
        for (int i = 0; i < cards.Count - 1; i++)
        {
            cards[i].GetComponent<CardScript>().MoveCard(Constants.CardContainerType.WastePile, this.gameObject, doLog, showHolo: false);
        }
        cards[^1].GetComponent<CardScript>().MoveCard(Constants.CardContainerType.WastePile, this.gameObject, doLog);

        if (doLog)
        {
            Actions.MoveUpdate();
        }

        StartCoroutine(ScrollBarAdding(cards.Count, wastePileWasEmpty));
    }

    public void AddCard(GameObject card, bool showHolo)
    {
        AddCard(card);

        if (showHolo)
        {
            CardScript cardScript = card.GetComponent<CardScript>();
            cardScript.Hologram = true;
            cardScript.HitBox = true;

            if (cardList.Count == GameValues.GamePlay.cardsToDeal + 1)
            {
                DeckScript.Instance.TryUpdateDeckCounter(true);
            }
        }
    }

    public void AddCard(GameObject card)
    {
        // obstructing the top
        if (cardList.Count != 0)
        {
            CardScript cardScript = cardList[^1].GetComponent<CardScript>();
            cardScript.Hologram = false;
            cardScript.Obstructed = true;
        }

        cardList.Add(card);

        // making a container for the card so that it plays nice with the scroll view
        GameObject newCardContainer = Instantiate(cardContainerPrefab, contentPanel.transform);
        newCardContainer.transform.SetSiblingIndex(cardList.Count);
        cardContainers.Add(newCardContainer);

        // updating the card, set the z offset so that cards appear on top of eachother
        card.transform.SetParent(newCardContainer.transform);
        card.transform.localPosition = new Vector3(0, 0, -cardList.Count * 0.01f);
    }

    public void RemoveCard(GameObject card, bool showHolo)
    {
        RemoveCard(card, false, showHolo);
    }

    public void RemoveCard(GameObject card, bool undoingOrDeck, bool showHolo)
    {
        // get cards wastepile container before removal
        GameObject parentCardContainer = card.transform.parent.gameObject;

        RemoveCard(card);

        if (cardList.Count != 0)
        {
            CardScript cardScript = cardList[^1].GetComponent<CardScript>();

            // during the tutorial we don't want the next card avalible for user interaction
            // save for when the deck deal button is unlocked as that can cause a deck flip
            if (undoingOrDeck || !Config.Instance.TutorialOn)
            {
                // set obstruction to false as it isn't accounted for elsewhere
                cardScript.Obstructed = false;
            }

            // will the new top card stay
            if (showHolo)
            {
                if (undoingOrDeck)
                {
                    cardScript.EnableHologramImmediately();
                }
                cardScript.Hologram = true;

                if (cardList.Count == GameValues.GamePlay.cardsToDeal)
                {
                    DeckScript.Instance.TryUpdateDeckCounter(false);
                }
            }
        }

        if (undoingOrDeck || cardList.Count == 0)
        {
            // immediately remove 
            Destroy(parentCardContainer);
        }
        else
        {
            // move the conveyor belt around to simulate card removal
            StartCoroutine(ScrollBarRemoving(parentCardContainer));
        }
    }

    public void RemoveCard(GameObject card)
    {
        card.transform.parent = null;
        cardList.RemoveAt(cardList.LastIndexOf(card));
    }

    public void StartDeckReset()
    {
        StartCoroutine(DeckReset());
    }

    private IEnumerator ScrollBarAdding(int numCardsAdded, bool wastePileWasEmpty)
    {
        SetScrolling(true);

        // move the scroll rect's content so that the new cards are hidden to the left side of the belt
        Vector2 startPosition = contentRectTransform.anchoredPosition;
        if (wastePileWasEmpty)
        {
            startPosition.x = -cardSpacing * (numCardsAdded  + 1);
        }
        else
        {
            startPosition.x -= cardSpacing * numCardsAdded;
        }
        contentRectTransform.anchoredPosition = startPosition;

        Vector2 endPosition = contentRectTransform.anchoredPosition;
        endPosition.x = 0;

        // get the number of cards that have been scrolled away from
        float numCardsFromStart = -contentRectTransform.anchoredPosition.x / cardSpacing;
        yield return Animate.SmoothstepRectTransform(contentRectTransform, startPosition, endPosition, GetScrollDuration(numCardsFromStart));

        DeckScript.Instance.StartButtonUp();

        SetScrolling(false);
    }

    private IEnumerator ScrollBarRemoving(GameObject parentCardContainer)
    {
        SetScrolling(true);

        Vector2 startPosition = contentRectTransform.anchoredPosition;
        // back 1 token distance
        Vector2 endPosition = contentRectTransform.anchoredPosition;
        endPosition.x = -cardSpacing;

        float duration = (cardSpacing + contentRectTransform.anchoredPosition.x) / (cardSpacing * 6);

        yield return Animate.SmoothstepRectTransform(contentRectTransform, startPosition, endPosition, duration);

        Destroy(parentCardContainer);
        SetScrolling(false);
    }

    private IEnumerator DeckReset()
    {
        SetScrolling(true);

        // move the scroll rect's content so that the new cards are hidden to the left side of the belt
        Vector2 startPosition = contentRectTransform.anchoredPosition;
        Vector2 endPosition = contentRectTransform.anchoredPosition;
        endPosition.x = -cardSpacing * (cardList.Count + 1);

        double numCardsFromEnd = cardList.Count + (contentRectTransform.anchoredPosition.x / cardSpacing);
        yield return Animate.SmoothstepRectTransform(contentRectTransform, startPosition, endPosition, GetScrollDuration(numCardsFromEnd));

        // move all the tokens
        while (cardList.Count > 0)
        {
            cardList[^1].GetComponent<CardScript>().MoveCard(Constants.CardContainerType.Deck, DeckScript.Instance.gameObject, showHolo: false);
        }

        // move it back to 0 so that when auto dealing occurs, the proper position is used to start scrolling
        endPosition.x = 0;
        contentRectTransform.anchoredPosition = endPosition;

        yield return deckResetDelay;
        DeckScript.Instance.Deal();
        // do not set scrolling to false yet as the deck deal will do that at the end
    }

    private void SetScrolling(bool value)
    {
        if (_scrolling == value) return;
        _scrolling = value;
        if (value)
        {
            // disable scrolling
            scrollRect.horizontal = false;
            // if there was a scrollbar
            //scrollRect.horizontalScrollbar.interactable = false;

            // we need unrestricted scroll for later shenanigans
            scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
            scrollRect.inertia = false;
        }
        else
        {
            contentRectTransform.anchoredPosition = Vector3.zero;

            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;

            scrollRect.horizontal = true;
            //scrollRect.horizontalScrollbar.interactable = true;
        }
        GameInput.Instance.InputStopped = value;
    }

    private float GetScrollDuration(double numCardsToScroll)
    {
        // for the more cards to scroll, the shorter the duration per card
        return (float) (0.25 * Math.Pow(numCardsToScroll, 0.4));
    }
}
