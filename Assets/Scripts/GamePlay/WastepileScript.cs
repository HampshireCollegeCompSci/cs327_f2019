using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WastepileScript : MonoBehaviour, ICardContainer
{
    public List<GameObject> cardList;

    public GameObject cardContainerPrefab;
    private List<GameObject> cardContainers;

    public GameObject contentPanel;
    private int cardSpacing;
    private ScrollRect scrollRect;
    private RectTransform contentRectTransform;

    // Singleton instance.
    public static WastepileScript Instance = null;

    // Initialize the singleton instance.
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            cardContainers = new List<GameObject>();

            cardSpacing = (int) contentPanel.GetComponent<HorizontalLayoutGroup>().spacing;
            scrollRect = this.gameObject.GetComponent<ScrollRect>();
            contentRectTransform = contentPanel.GetComponent<RectTransform>();
        }
        else if (Instance != this)
        {
            throw new System.ArgumentException("there should not already be an instance of this");
        }
    }

    public void AddCards(List<GameObject> cards, bool doLog = true)
    {
        StartCoroutine(ScrollBarAdding(cards, doLog));
    }

    IEnumerator ScrollBarAdding(List<GameObject> cards, bool doLog)
    {
        if (!Scrolling)
        {
            Scrolling = true;
        }

        if (cardList.Count != 0) // hide the current top token now
        {
            cardList[0].GetComponent<CardScript>().Hologram = false;
            cardList[0].GetComponent<CardScript>().Interactable = false;
        }

        Vector3 temp = contentRectTransform.anchoredPosition;

        // if the contents have been moved from their default position scroll to the left
        while (temp.x > 0)
        {
            yield return null;
            temp.x -= Time.deltaTime * Config.GameValues.wastepileAnimationSpeedFast;
            contentRectTransform.anchoredPosition = temp;
        }

        // add the new cards, for the non-top cards: don't try to show their hologram
        for (int i = 0; i < cards.Count - 1; i++)
        {
            cards[i].GetComponent<CardScript>().MoveCard(this.gameObject, doLog, showHolo: false);
        }
        cards[^1].GetComponent<CardScript>().MoveCard(gameObject, doLog);

        // move the scroll rect's content so that the new cards are hidden to the left side of the belt
        temp.x = cards.Count * cardSpacing;
        contentRectTransform.anchoredPosition = temp;

        // scroll the tokens back into view
        while (temp.x > 0)
        {
            yield return null;
            temp.x -= Time.deltaTime * Config.GameValues.wastepileAnimationSpeedSlow;
            contentRectTransform.anchoredPosition = temp;
        }

        DeckScript.Instance.StartButtonUp();

        Scrolling = false;

        if (doLog)
        {
            UtilsScript.Instance.UpdateActions(1);
        }
    }

    public void AddCard(GameObject card, bool showHolo)
    {
        AddCard(card);

        if (showHolo)
        {
            CardScript cardScript = card.GetComponent<CardScript>();
            cardScript.Interactable = true;
            cardScript.Hologram = true;

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
            CardScript cardScript = cardList[0].GetComponent<CardScript>();
            cardScript.Interactable = false;
            cardScript.Hologram = false;
        }

        cardList.Insert(0, card);

        // making a container for the card so that it plays nice with the scroll view
        cardContainers.Insert(0, Instantiate(cardContainerPrefab));
        cardContainers[0].transform.SetParent(contentPanel.transform, false);
        RectTransform cardContainerTransform = cardContainers[0].GetComponent<RectTransform>();
        cardContainerTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1);
        cardContainerTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1);
        //Debug.Log(cardContainerTransform.localPosition);
        //cardContainerTransform.localPosition = new Vector3(cardContainerTransform.localPosition.x + 100, cardContainerTransform.localPosition.y, cardContainerTransform.localPosition.z);

        // updating the card
        card.transform.SetParent(cardContainers[0].transform);
        //card.transform.position = card.transform.parent.position;
        card.transform.localPosition = new Vector3(0, 0, -1 - (cardList.Count * 0.01f));
    }

    public void RemoveCard(GameObject card, bool undoingOrDeck = false, bool showHolo = true)
    {
        // get cards wastepile container before removal
        GameObject parentCardContainer = card.transform.parent.gameObject;

        RemoveCard(card);

        if (cardList.Count != 0)
        {
            cardList[0].GetComponent<CardScript>().Interactable = true;
            CardScript cardScript = cardList[0].GetComponent<CardScript>();

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
        // removing the cards wastepile container
        cardContainers.Remove(card.transform.parent.gameObject);
        card.transform.parent = null;

        card.GetComponent<CardScript>().DefaultColor();
        cardList.Remove(card);
    }

    private IEnumerator ScrollBarRemoving(GameObject parentCardContainer)
    {
        Scrolling = true;

        // Scrolls the conveyor belt back 1 token distance
        Vector3 temp = contentRectTransform.anchoredPosition;
        while (temp.x < cardSpacing)
        {
            temp.x += Time.deltaTime * Config.GameValues.wastepileAnimationSpeedSlow;
            contentRectTransform.anchoredPosition = temp;
            yield return null;
        }

        Destroy(parentCardContainer);
        Scrolling = false;
    }

    public void StartDeckReset()
    {
        StartCoroutine(DeckReset());
    }

    private IEnumerator DeckReset()
    {
        Scrolling = true;

        // move all the tokens in the conveyor belt to the left
        Vector3 temp = contentRectTransform.anchoredPosition;
        int xGoal = (cardList.Count + 1) * cardSpacing;
        while (temp.x < xGoal)
        {
            temp.x += Time.deltaTime * Config.GameValues.wastepileAnimationSpeedFast;
            contentRectTransform.anchoredPosition = temp;
            yield return null;
        }

        // move all the tokens
        while (cardList.Count > 0)
        {
            cardList[0].GetComponent<CardScript>().MoveCard(DeckScript.Instance.gameObject, showHolo: false);
        }

        yield return new WaitForSeconds(0.5f);
        DeckScript.Instance.Deal();
    }

    private bool _scrolling;
    private bool Scrolling
    {
        get { return _scrolling; }
        set
        {
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

    private bool _draggingCard;
    public bool DraggingCard
    {
        get { return _draggingCard; }
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
}
