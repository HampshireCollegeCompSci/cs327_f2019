using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WastepileScript : MonoBehaviour
{
    private UtilsScript utils;
    public SoundController soundController;
    public GameObject deck;
    private DeckScript deckScript;
    public GameObject contentPanel;

    public List<GameObject> cardList;

    public GameObject cardContainer;
    private List<GameObject> cardContainers;

    public ScrollRect scrollRect;
    private RectTransform contentRectTransform;
    public bool isScrolling;

    private void Start()
    {
        utils = UtilsScript.global;
        deckScript = deck.GetComponent<DeckScript>();

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
        DisableScrolling();

        if (cardList.Count != 0) // hide the current top tokens hologram now
            cardList[0].GetComponent<CardScript>().HideHologram();

        Vector3 temp = contentRectTransform.anchoredPosition;

        // if the contents have been moved from their default position scroll to the left
        while (temp.x > 0)
        {
            yield return null;
            temp.x -= Time.deltaTime * Config.config.wastepileAnimationSpeedFast;
            contentRectTransform.anchoredPosition = temp;
        }

        // add the new cards
        for (int i = 0; i < cards.Count; i++)
            cards[i].GetComponent<CardScript>().MoveCard(gameObject, doLog);

        utils.UpdateActionCounter(1);

        // move the scroll rect's content so that the new cards are hidden to the left side of the belt
        temp.x = cards.Count;
        contentRectTransform.anchoredPosition = temp;

        // scroll the tokens back into view
        while (temp.x > 0)
        {
            yield return null;
            temp.x -= Time.deltaTime * Config.config.wastepileAnimationSpeedSlow;
            contentRectTransform.anchoredPosition = temp;
        }

        deckScript.StartButtonUp();

        ResetScrollBar(temp);
        utils.CheckNextCycle();
        utils.CheckGameOver();
    }

    public void AddCard(GameObject card)
    {
        // hidding the top
        if (cardList.Count != 0)
        {
            cardList[0].GetComponent<CardScript>().HideHologram();
            cardList[0].GetComponent<BoxCollider2D>().enabled = false;
        }

        cardList.Insert(0, card);
        cardList[0].GetComponent<CardScript>().ShowHologram();

        // making a container for the card so that it plays nice with the scroll view
        cardContainers.Insert(0, Instantiate(cardContainer));
        cardContainers[0].transform.SetParent(contentPanel.transform, false);
        cardContainers[0].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1);
        cardContainers[0].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1);

        // updating the card
        card.transform.SetParent(cardContainers[0].transform);
        card.transform.position = new Vector3(card.transform.parent.position.x, card.transform.parent.position.y, -1 - (cardList.Count * 0.01f));
        card.GetComponent<CardScript>().UpdateMaskInteraction(SpriteMaskInteraction.VisibleInsideMask);

        deckScript.UpdateDeckCounter();
    }

    public void RemoveCard(GameObject card, bool undoingOrDeck = false)
    {
        // removing the cards wastepile container
        GameObject parentCardContainer = card.transform.parent.gameObject;
        card.transform.parent = null;
        cardContainers.Remove(parentCardContainer);

        card.GetComponent<CardScript>().UpdateMaskInteraction(SpriteMaskInteraction.None);
        cardList.Remove(card);

        if (cardList.Count != 0)
        {
            cardList[0].GetComponent<CardScript>().ShowHologram();
            cardList[0].GetComponent<BoxCollider2D>().enabled = true;
        }

        if (undoingOrDeck || cardList.Count == 0)
            Destroy(parentCardContainer);
        else
            StartCoroutine(ScrollBarRemoving(parentCardContainer));

        deckScript.UpdateDeckCounter();
    }

    IEnumerator ScrollBarRemoving(GameObject parentCardContainer)
    {
        // Scrolls the conveyor belt back 1 token distance

        DisableScrolling();

        Vector3 temp = contentRectTransform.anchoredPosition;
        while (temp.x < 1)
        {
            temp.x += Time.deltaTime * Config.config.wastepileAnimationSpeedSlow;
            contentRectTransform.anchoredPosition = temp;
            yield return null;
        }

        Destroy(parentCardContainer);
        ResetScrollBar(temp);
    }

    public void DeckReset()
    {
        StartCoroutine(ResetDeck());
    }

    IEnumerator ResetDeck()
    {
        DisableScrolling();

        // move all the tokens in the conveyor belt to the left
        Vector3 temp = contentRectTransform.anchoredPosition;
        while (temp.x < cardList.Count + 1)
        {
            yield return null;
            temp.x += Time.deltaTime * Config.config.wastepileAnimationSpeedFast;
            contentRectTransform.anchoredPosition = temp;
        }

        // move all the tokens
        while (cardList.Count > 0)
            cardList[0].GetComponent<CardScript>().MoveCard(deck);

        yield return new WaitForSeconds(0.5f);

        isScrolling = false;
        deckScript.Deal();
    }

    private void DisableScrolling()
    {
        isScrolling = true;
        utils.SetInputStopped(true);

        // disable scrolling
        scrollRect.horizontal = false;
        scrollRect.horizontalScrollbar.interactable = false;

        // we need unrestricted scroll for later shenanigans
        scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
    }

    public void ResetScrollBar(Vector3 temp)
    {
        temp.x = 0;
        contentRectTransform.anchoredPosition = temp;

        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        if (!utils.isMatching)
        {
            scrollRect.horizontal = true;
        }

        scrollRect.horizontalScrollbar.interactable = true;
        utils.SetInputStopped(false);
        isScrolling = false;
    }

    public void DraggingCard(GameObject card, bool isDragging)
    {
        if (isDragging)
        {
            card.GetComponent<CardScript>().UpdateMaskInteraction(SpriteMaskInteraction.None);
            scrollRect.horizontal = false;
        }
        else
        {
            if (!utils.isMatching && card.GetComponent<CardScript>().container == this.gameObject)
            {
                card.GetComponent<CardScript>().UpdateMaskInteraction(SpriteMaskInteraction.VisibleInsideMask);
                scrollRect.horizontal = true;
            }
        }
    }

    public void ProcessAction(GameObject input)
    {
        if (utils.selectedCards.Count != 1)
            throw new System.ArgumentException("utils.selectedCards must be of size 1");

        GameObject selectedCard = utils.selectedCards[0];
        CardScript selectedCardScript = selectedCard.GetComponent<CardScript>();

        if (input.CompareTag("Card"))
        {
            CardScript inputCardScript = input.GetComponent<CardScript>();

            if (utils.CanMatch(inputCardScript, selectedCardScript))
                utils.Match(input, selectedCard);
            else if (inputCardScript.container.CompareTag("Reactor"))
            {
                if (!utils.IsSameSuit(input, selectedCard))
                    return;

                soundController.CardToReactorSound();
                selectedCardScript.MoveCard(inputCardScript.container);
                utils.UpdateActionCounter(1);
            }
            else if (inputCardScript.container.CompareTag("Foundation"))
            {
                if (inputCardScript.container.GetComponent<FoundationScript>().cardList[0] != input ||
                    inputCardScript.cardNum != selectedCardScript.cardNum + 1)
                    return;

                soundController.CardStackSound();
                selectedCardScript.MoveCard(inputCardScript.container);
                utils.UpdateActionCounter(1);
            }
        }
        else if (input.CompareTag("Reactor"))
        {
            if (!utils.IsSameSuit(input, selectedCard))
                return;

            soundController.CardToReactorSound();
            selectedCardScript.MoveCard(input);
            utils.UpdateActionCounter(1);
        }
        else if (input.CompareTag("Foundation"))
        {
            if (input.GetComponent<FoundationScript>().cardList.Count != 0)
                return;

            soundController.CardStackSound();
            selectedCardScript.MoveCard(input);
            utils.UpdateActionCounter(1);
        }
        else
        {
            Debug.Log("PA: wastepile reached the end");
            return;
        }

        utils.CheckNextCycle();
        utils.CheckGameOver();
    }
}
