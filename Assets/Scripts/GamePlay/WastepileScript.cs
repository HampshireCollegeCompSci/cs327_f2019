using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WastepileScript : MonoBehaviour
{
    public List<GameObject> cardList;

    public GameObject cardContainerPrefab;
    private List<GameObject> cardContainers;

    public GameObject contentPanel;
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
        }
        else if (Instance != this)
        {
            throw new System.ArgumentException("there should not already be an instance of this");
        }
    }

    private void Start()
    {
        cardContainers = new List<GameObject>();

        scrollRect = gameObject.GetComponent<ScrollRect>();
        contentRectTransform = contentPanel.GetComponent<RectTransform>();
    }

    public void AddCards(List<GameObject> cards, bool doLog = true)
    {
        StartCoroutine(ScrollBarAdding(cards, doLog));
    }

    IEnumerator ScrollBarAdding(List<GameObject> cards, bool doLog)
    {
        if (!scrollingDisabled)
            DisableScrolling();

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

        // add the new cards
        for (int i = 0; i < cards.Count - 1; i++)
        {
            cards[i].GetComponent<CardScript>().MoveCard(gameObject, doLog, showHolo: false);
            cards[i].GetComponent<SpriteRenderer>().color = Config.GameValues.cardObstructedColor;
        }

        cards[cards.Count - 1].GetComponent<CardScript>().MoveCard(gameObject, doLog);

        // move the scroll rect's content so that the new cards are hidden to the left side of the belt
        temp.x = cards.Count;
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
            UtilsScript.Instance.UpdateActions(1);
    }

    public void AddCard(GameObject card, bool showHolo = true)
    {
        // hidding the top
        if (cardList.Count != 0)
        {
            cardList[0].GetComponent<CardScript>().HideHologram();
            cardList[0].GetComponent<SpriteRenderer>().color = Config.GameValues.cardObstructedColor;
            cardList[0].GetComponent<BoxCollider2D>().enabled = false;
        }

        cardList.Insert(0, card);

        if (showHolo)
        {
            card.GetComponent<CardScript>().ShowHologram();
            card.GetComponent<BoxCollider2D>().enabled = true;
        }

        // making a container for the card so that it plays nice with the scroll view
        cardContainers.Insert(0, Instantiate(cardContainerPrefab));
        cardContainers[0].transform.SetParent(contentPanel.transform, false);
        cardContainers[0].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1);
        cardContainers[0].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1);

        // updating the card
        card.transform.SetParent(cardContainers[0].transform);
        card.transform.position = new Vector3(card.transform.parent.position.x, card.transform.parent.position.y, -1 - (cardList.Count * 0.01f));
        card.GetComponent<CardScript>().UpdateMaskInteraction(SpriteMaskInteraction.VisibleInsideMask);

        DeckScript.Instance.UpdateDeckCounter();
    }

    public void RemoveCard(GameObject card, bool undoingOrDeck = false, bool showHolo = true)
    {
        // removing the cards wastepile container
        GameObject parentCardContainer = card.transform.parent.gameObject;
        card.transform.parent = null;
        cardContainers.Remove(parentCardContainer);

        CardScript cardScriptPointer = card.GetComponent<CardScript>();
        card.GetComponent<SpriteRenderer>().color = cardScriptPointer.originalColor;
        cardScriptPointer.UpdateMaskInteraction(SpriteMaskInteraction.None);
        cardList.Remove(card);

        if (showHolo && cardList.Count != 0)
        {
            cardScriptPointer = cardList[0].GetComponent<CardScript>();
            cardScriptPointer.ShowHologram();
            cardList[0].GetComponent<SpriteRenderer>().color = cardScriptPointer.originalColor;
            cardList[0].GetComponent<BoxCollider2D>().enabled = true;
        }

        if (undoingOrDeck || cardList.Count == 0)
            Destroy(parentCardContainer);
        else
            StartCoroutine(ScrollBarRemoving(parentCardContainer));

        DeckScript.Instance.UpdateDeckCounter();
    }

    IEnumerator ScrollBarRemoving(GameObject parentCardContainer)
    {
        // Scrolls the conveyor belt back 1 token distance

        DisableScrolling();

        Vector3 temp = contentRectTransform.anchoredPosition;
        while (temp.x < 1)
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

    IEnumerator DeckReset()
    {
        DisableScrolling();

        // move all the tokens in the conveyor belt to the left
        Vector3 temp = contentRectTransform.anchoredPosition;
        while (temp.x < cardList.Count + 1)
        {
            yield return null;
            temp.x += Time.deltaTime * Config.GameValues.wastepileAnimationSpeedFast;
            contentRectTransform.anchoredPosition = temp;
        }

        // move all the tokens
        while (cardList.Count > 0)
            cardList[0].GetComponent<CardScript>().MoveCard(DeckScript.Instance.gameObject, showHolo: false);

        yield return new WaitForSeconds(0.5f);

        DeckScript.Instance.Deal();
    }

    private void DisableScrolling()
    {
        scrollingDisabled = true;
        UtilsScript.Instance.SetInputStopped(true);

        // disable scrolling
        scrollRect.horizontal = false;
        //scrollRect.horizontalScrollbar.interactable = false;

        // we need unrestricted scroll for later shenanigans
        scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
    }

    public void ResetScrollBar()
    {
        contentRectTransform.anchoredPosition = Vector3.zero;

        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        scrollRect.horizontal = true;
        //scrollRect.horizontalScrollbar.interactable = true;

        UtilsScript.Instance.SetInputStopped(false);
        scrollingDisabled = false;
    }

    public void DraggingCard(GameObject card, bool isDragging)
    {
        if (isDragging)
        {
            card.GetComponent<CardScript>().UpdateMaskInteraction(SpriteMaskInteraction.None);
            scrollRect.horizontal = false;
            //scrollRect.horizontalScrollbar.interactable = false;
        }
        else if (card.GetComponent<CardScript>().container == this.gameObject)
        {
            card.GetComponent<CardScript>().UpdateMaskInteraction(SpriteMaskInteraction.VisibleInsideMask);
            scrollRect.horizontal = true;
            //scrollRect.horizontalScrollbar.interactable = true;
        }
    }

    public void ProcessAction(GameObject input)
    {
        if (UtilsScript.Instance.selectedCards.Count != 1)
            throw new System.ArgumentException("utils.selectedCards must be of size 1");

        GameObject selectedCard = UtilsScript.Instance.selectedCards[0];
        CardScript selectedCardScript = selectedCard.GetComponent<CardScript>();

        if (input.CompareTag("Card"))
        {
            CardScript inputCardScript = input.GetComponent<CardScript>();

            if (CardTools.CanMatch(inputCardScript, selectedCardScript))
            {
                UtilsScript.Instance.Match(input, selectedCard);
                return;
            }
            else if (inputCardScript.container.CompareTag("Reactor"))
            {
                if (!CardTools.CompareSameSuitObjects(input, selectedCard))
                    return;

                SoundEffectsController.Instance.CardToReactorSound();
                selectedCardScript.MoveCard(inputCardScript.container);
            }
            else if (inputCardScript.container.CompareTag("Foundation"))
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
        else if (input.CompareTag("Reactor"))
        {
            if (!CardTools.CompareSameSuitObjects(input, selectedCard))
                return;

            SoundEffectsController.Instance.CardToReactorSound();
            selectedCardScript.MoveCard(input);
        }
        else if (input.CompareTag("Foundation"))
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
