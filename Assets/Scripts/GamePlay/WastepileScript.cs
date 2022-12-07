using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WastepileScript : MonoBehaviour, ICardContainerHolo
{
    // Singleton instance.
    public static WastepileScript Instance;

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

            cardList = new List<GameObject>(52);
            cardContainers = new List<GameObject>(52);

            cardSpacing = contentPanel.GetComponent<HorizontalLayoutGroup>().spacing;
            scrollRect = this.gameObject.GetComponent<ScrollRect>();
            contentRectTransform = contentPanel.GetComponent<RectTransform>();
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
        StartCoroutine(ScrollBarAdding(cards, doLog));
    }

    public void AddCard(GameObject card, bool showHolo)
    {
        AddCard(card);

        if (showHolo)
        {
            CardScript cardScript = card.GetComponent<CardScript>();
            cardScript.Hologram = true;
            cardScript.HitBox = true;

            if (cardList.Count == Config.GameValues.cardsToDeal + 1)
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
        cardContainers.Add(Instantiate(cardContainerPrefab));
        cardContainers[^1].transform.SetParent(contentPanel.transform, false);
        RectTransform cardContainerTransform = cardContainers[^1].GetComponent<RectTransform>();
        cardContainerTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1);
        cardContainerTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1);
        //Debug.Log(cardContainerTransform.localPosition);
        //cardContainerTransform.localPosition = new Vector3(cardContainerTransform.localPosition.x + 100, cardContainerTransform.localPosition.y, cardContainerTransform.localPosition.z);

        // updating the card
        card.transform.SetParent(cardContainers[^1].transform);
        //card.transform.position = card.transform.parent.position;
        card.transform.localPosition = new Vector3(0, 0, -1 - (cardList.Count * 0.01f));
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
            if (undoingOrDeck || !Config.Instance.tutorialOn)
            {
                // set obstruction to false as it isn't accounted for elsewhere
                cardScript.Obstructed = false;
            }

            // will the new top card stay
            if (showHolo)
            {
                cardScript.Hologram = true;

                if (cardList.Count == Config.GameValues.cardsToDeal)
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

    private IEnumerator ScrollBarAdding(List<GameObject> cards, bool doLog)
    {
        SetScrolling(true);

        if (cardList.Count != 0) // hide the current top token now
        {
            CardScript cardScript = cardList[^1].GetComponent<CardScript>();
            cardScript.Hologram = false;
            cardScript.Obstructed = true;
        }

        Vector3 contentPosition = contentRectTransform.anchoredPosition;

        // if the contents have been moved from their default position scroll to the left
        while (contentPosition.x > 0)
        {
            yield return null;
            contentPosition.x -= Time.deltaTime * Config.GameValues.wastepileAnimationSpeedFast;
            contentRectTransform.anchoredPosition = contentPosition;
        }

        // add the new cards, for the non-top cards: don't try to show their hologram
        for (int i = 0; i < cards.Count - 1; i++)
        {
            cards[i].GetComponent<CardScript>().MoveCard(this.gameObject, doLog, showHolo: false);
        }
        cards[^1].GetComponent<CardScript>().MoveCard(this.gameObject, doLog);

        // move the scroll rect's content so that the new cards are hidden to the left side of the belt
        contentPosition.x = cardSpacing * cards.Count;
        contentRectTransform.anchoredPosition = contentPosition;

        // scroll the tokens back into view
        while (contentPosition.x > 0)
        {
            yield return null;
            contentPosition.x -= Time.deltaTime * Config.GameValues.wastepileAnimationSpeedSlow;
            contentRectTransform.anchoredPosition = contentPosition;
        }

        DeckScript.Instance.StartButtonUp();

        SetScrolling(false);

        if (doLog)
        {
            UtilsScript.Instance.UpdateActions(1);
        }
    }

    private IEnumerator ScrollBarRemoving(GameObject parentCardContainer)
    {
        SetScrolling(true);

        // Scrolls the conveyor belt back 1 token distance
        Vector3 contentPosition = contentRectTransform.anchoredPosition;
        while (contentPosition.x < cardSpacing)
        {
            contentPosition.x += Time.deltaTime * Config.GameValues.wastepileAnimationSpeedSlow;
            contentRectTransform.anchoredPosition = contentPosition;
            yield return null;
        }

        Destroy(parentCardContainer);
        SetScrolling(false);
    }

    private IEnumerator DeckReset()
    {
        SetScrolling(true);

        // move all the tokens in the conveyor belt to the left
        Vector3 contentPosition = contentRectTransform.anchoredPosition;
        float xGoal = cardSpacing * (cardList.Count + 1);
        while (contentPosition.x < xGoal)
        {
            contentPosition.x += Time.deltaTime * Config.GameValues.wastepileAnimationSpeedFast;
            contentRectTransform.anchoredPosition = contentPosition;
            yield return null;
        }

        // move all the tokens
        while (cardList.Count > 0)
        {
            cardList[^1].GetComponent<CardScript>().MoveCard(DeckScript.Instance.gameObject, showHolo: false);
        }

        yield return new WaitForSeconds(0.3f);
        DeckScript.Instance.Deal();
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
        UtilsScript.Instance.InputStopped = value;
    }
}
