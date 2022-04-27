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

    private bool scrollingDisabled;

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
        if (!scrollingDisabled)
        {
            DisableScrolling();
        }

        if (cardList.Count != 0) // hide the current top tokens hologram now
        {
            cardList[0].GetComponent<CardScript>().HideHologram();
            cardList[0].GetComponent<SpriteRenderer>().color = Config.GameValues.cardObstructedColor;
        }

        Vector3 temp = contentRectTransform.anchoredPosition;

        // if the contents have been moved from their default position scroll to the left
        while (temp.x > 0)
        {
            yield return null;
            temp.x -= Time.deltaTime * Config.GameValues.wastepileAnimationSpeedFast;
            contentRectTransform.anchoredPosition = temp;
        }

        // add the new cards, for the non-top cards:
        // shade them differently and
        // don't try to show their hologram
        for (int i = 0; i < cards.Count - 1; i++)
        {
            cards[i].GetComponent<CardScript>().MoveCard(gameObject, doLog, showHolo: false);
            //cards[i].GetComponent<SpriteRenderer>().color = Config.GameValues.cardObstructedColor;
        }
        cards[cards.Count - 1].GetComponent<CardScript>().MoveCard(gameObject, doLog);

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

        ResetScrollBar();

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
            card.GetComponent<CardScript>().ShowHologram();
            card.GetComponent<CardScript>().SetCollider(true);
        }
    }

    public void AddCard(GameObject card)
    {
        // obstructing the top
        if (cardList.Count != 0)
        {
            cardList[0].GetComponent<CardScript>().SetObstructed(true);
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

        if (showHolo && cardList.Count != 0)
        {
            // the new top card will stay
            cardList[0].GetComponent<CardScript>().SetObstructed(false);
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

        card.GetComponent<CardScript>().SetColor();
        cardList.Remove(card);
    }

    private IEnumerator ScrollBarRemoving(GameObject parentCardContainer)
    {
        DisableScrolling();

        // Scrolls the conveyor belt back 1 token distance
        Vector3 temp = contentRectTransform.anchoredPosition;
        while (temp.x < cardSpacing)
        {
            temp.x += Time.deltaTime * Config.GameValues.wastepileAnimationSpeedSlow;
            contentRectTransform.anchoredPosition = temp;
            yield return null;
        }

        Destroy(parentCardContainer);
        ResetScrollBar();
    }

    public void StartDeckReset()
    {
        StartCoroutine(DeckReset());
    }

    private IEnumerator DeckReset()
    {
        DisableScrolling();

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

    private void DisableScrolling()
    {
        scrollingDisabled = true;
        UtilsScript.Instance.InputStopped = true;

        // disable scrolling
        scrollRect.horizontal = false;
        //scrollRect.horizontalScrollbar.interactable = false;

        // we need unrestricted scroll for later shenanigans
        scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
        scrollRect.inertia = false;
    }

    public void ResetScrollBar()
    {
        contentRectTransform.anchoredPosition = Vector3.zero;

        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = true;

        scrollRect.horizontal = true;
        //scrollRect.horizontalScrollbar.interactable = true;

        UtilsScript.Instance.InputStopped = false;
        scrollingDisabled = false;
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

    public void ProcessAction(GameObject input)
    {
        if (UtilsScript.Instance.selectedCards.Count != 1)
            throw new System.ArgumentException("utils.selectedCards must be of size 1");

        GameObject selectedCard = UtilsScript.Instance.selectedCards[0];
        CardScript selectedCardScript = selectedCard.GetComponent<CardScript>();

        if (input.CompareTag(Constants.cardTag))
        {
            CardScript inputCardScript = input.GetComponent<CardScript>();

            if (CardTools.CanMatch(inputCardScript, selectedCardScript))
            {
                UtilsScript.Instance.Match(input, selectedCard);
                return;
            }
            else if (inputCardScript.container.CompareTag(Constants.reactorTag))
            {
                if (!CardTools.CompareSameSuitObjects(input, selectedCard))
                    return;

                SoundEffectsController.Instance.CardToReactorSound();
                selectedCardScript.MoveCard(inputCardScript.container);
            }
            else if (inputCardScript.container.CompareTag(Constants.foundationTag))
            {
                if (inputCardScript.container.GetComponent<FoundationScript>().cardList[0] != input ||
                    inputCardScript.cardNum != selectedCardScript.cardNum + 1)
                    return;

                SoundEffectsController.Instance.CardStackSound();
                selectedCardScript.MoveCard(inputCardScript.container);
            }
            else
                return;
        }
        else if (input.CompareTag(Constants.reactorTag))
        {
            if (!CardTools.CompareSameSuitObjects(input, selectedCard))
                return;

            SoundEffectsController.Instance.CardToReactorSound();
            selectedCardScript.MoveCard(input);
        }
        else if (input.CompareTag(Constants.foundationTag))
        {
            if (input.GetComponent<FoundationScript>().cardList.Count != 0)
                return;

            SoundEffectsController.Instance.CardStackSound();
            selectedCardScript.MoveCard(input);
        }
        else
        {
            return;
        }

        UtilsScript.Instance.UpdateActions(1);
    }
}
