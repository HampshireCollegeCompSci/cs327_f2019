using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WastepileScript : MonoBehaviour
{
    public UtilsScript utils;
    public SoundController soundController;
    public GameObject deck;
    private DeckScript deckScript;
    public GameObject contentPanel;
    public GameObject cardContainer;
    public List<GameObject> cardList;
    public List<GameObject> cardContainers;

    //public List<GameObject> conveyorBeltPieces;
    //private List<Transform> conveyorTransforms;
    //private int conveyorCount;
    //private double startX;
    //private double endX;

    public ScrollRect scrollRect;
    private RectTransform contentRectTransform;

    private void Start()
    {
        utils = UtilsScript.global;
        deckScript = deck.GetComponent<DeckScript>();

        //conveyorTransforms = new List<Transform>();
        //for (int i = 0; i < conveyorBeltPieces.Count; i++)
        //{
        //    conveyorTransforms.Add(conveyorBeltPieces[i].GetComponent<Transform>());
        //}
        //conveyorCount = conveyorTransforms.Count - 1;
        //startX = -3.28 - 2.28;
        //endX = 5.84 + 2.28;
        //Debug.Log(startX + " " + endX);


        scrollRect = gameObject.GetComponent<ScrollRect>();
        contentRectTransform = contentPanel.GetComponent<RectTransform>();
        //OnEnable();
    }

    void OnEnable()
    {
        //Subscribe to the ScrollRect event
        //scrollRect.onValueChanged.AddListener(Conveyor);
    }

    //private void Conveyor(Vector2 value)
    //{
    //    //Debug.Log("ScrollRect Changed: " + value);
    //    //Debug.Log("0 " + conveyorTransforms[0].position.x + " " + startX);
    //    //Debug.Log("1 " + conveyorTransforms[conveyorCount].position.x + " " + endX);

    //    if (conveyorTransforms[conveyorCount].position.x <= 3.56)
    //    {
    //        Transform conveyorTransformTemp = conveyorTransforms[0];
    //        conveyorTransforms.RemoveAt(0);

    //        Vector3 temp = conveyorTransformTemp.position;
    //        temp.x = 5.84f;
    //        conveyorTransformTemp.position = temp;

    //        conveyorTransforms.Add(conveyorTransformTemp);
    //        Debug.Log("front to back");
    //    }
    //    else if (conveyorTransforms[0].position.x >= -1)
    //    {
    //        Transform conveyorTransformTemp = conveyorTransforms[conveyorCount];
    //        conveyorTransforms.RemoveAt(conveyorCount);

    //        Vector3 temp = conveyorTransformTemp.position;
    //        temp.x = -3.28f;
    //        conveyorTransformTemp.position = temp;

    //        conveyorTransforms.Insert(0, conveyorTransformTemp);
    //        Debug.Log("back to front");
    //    }
    //}

    void OnDisable()
    {
        //Un-Subscribe To ScrollRect Event
        //scrollRect.onValueChanged.RemoveListener(Conveyor);
    }

    public void CheckHologram(bool tryHidingBeneath)
    {
        if (cardList.Count != 0)
        {
            cardList[0].gameObject.GetComponent<CardScript>().ShowHologram();

            if (tryHidingBeneath)
            {
                for (int i = 1; i < cardList.Count; i++)
                {
                    if (cardList[i].GetComponent<CardScript>().HideHologram())
                    {
                        return;
                    }
                }
            }
        }
    }

    public void AddCards(List<GameObject> cards, bool doLog = true)
    {
        StartCoroutine(ScrollBarAdding(cards, doLog));
    }

    IEnumerator ScrollBarAdding(List<GameObject> cards, bool doLog)
    {
        DisableScrolling();

        if (cardList.Count != 0) // hide the current top tokens hologram now
        {
            cardList[0].GetComponent<CardScript>().HideHologram();
        }

        Vector3 temp = contentRectTransform.anchoredPosition;

        // if we are not adding the first cards and
        // if the contents have been moved from their default position
        if (temp.x != 0)
        {
            // scroll to the left
            while (temp.x > 0)
            {
                yield return null;
                temp.x -= Time.deltaTime * Config.config.wastepileAnimationSpeedFast;
                contentRectTransform.anchoredPosition = temp;
            }
        }

        // add the new cards
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].GetComponent<CardScript>().MoveCard(gameObject, doLog: doLog, addUpdateHolo: false);
        }

        if (doLog)
        {
            utils.UpdateActionCounter(1);
        }

        // show the new top tokens hologram now
        CheckHologram(false);

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

    public void AddCard(GameObject card, bool checkHolo = true)
    {
        cardList.Insert(0, card);
        cardContainers.Insert(0, Instantiate(cardContainer));
        cardContainers[0].transform.SetParent(contentPanel.transform, false);
        card.transform.SetParent(cardContainers[0].transform);

        card.transform.parent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1);
        card.transform.parent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1);

        //LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)contentPanel.transform);

        card.transform.position = new Vector3(card.transform.parent.position.x, card.transform.parent.position.y, -1 - (cardList.Count * 0.01f));
        card.GetComponent<CardScript>().UpdateMaskInteraction(SpriteMaskInteraction.VisibleInsideMask);

        // is a deck flip is now possible and worthwhile?
        if (deckScript.cardList.Count == 0)
        {
            if (cardList.Count > Config.config.cardsToDeal)
            {
                deckScript.deckCounter.fontSize = 45;
                deckScript.deckCounter.text = "FLIP";
            }
            else
            {
                deckScript.deckCounter.fontSize = 40;
                deckScript.deckCounter.text = "EMPTY";
            }
        }

        if (checkHolo)
        {
            CheckHologram(true);
        }
    }

    public void RemoveCard(GameObject card, bool checkHolo = true)
    {
        GameObject parentCardContainer = card.transform.parent.gameObject;
        card.transform.parent = null;
        cardContainers.Remove(parentCardContainer);

        card.GetComponent<CardScript>().UpdateMaskInteraction(SpriteMaskInteraction.None);
        cardList.Remove(card);

        if (checkHolo && cardList.Count != 0)
        {
            CheckHologram(false);
            StartCoroutine(ScrollBarRemoving(parentCardContainer));
        }
        else
        {
            Destroy(parentCardContainer);
        }
        
        if (deckScript.cardList.Count == 0 && cardList.Count <= Config.config.cardsToDeal)
        {
            deckScript.deckCounter.fontSize = 40;
            deckScript.deckCounter.text = "EMPTY";
        }
    }

    IEnumerator ScrollBarRemoving(GameObject parentCardContainer)
    {
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

        Vector3 temp = contentRectTransform.anchoredPosition;
        while (temp.x < cardList.Count + 1)
        {
            yield return null;
            temp.x += Time.deltaTime * Config.config.wastepileAnimationSpeedFast;
            contentRectTransform.anchoredPosition = temp;
        }

        while (cardList.Count > 0)
        {
            cardList[0].GetComponent<CardScript>().MoveCard(deck, removeUpdateHolo: false);
        }

        //ResetScrollBar(temp);
        yield return new WaitForSeconds(0.5f);
        if (deckScript.dealOnDeckReset)
        {
            deckScript.Deal();
        }
    }

    private void DisableScrolling()
    {
        // disable scrolling and flag it
        scrollRect.horizontal = false;
        scrollRect.horizontalScrollbar.interactable = false;
        utils.SetInputStopped(true);

        // we need unrestricted scroll for later shenanigans
        scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
    }

    public void ResetScrollBar(Vector3 temp)
    {
        temp.x = 0;
        contentRectTransform.anchoredPosition = temp;

        // set everything back to how it was
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        if (!utils.isMatching)
        {
            scrollRect.horizontal = true;
        }

        scrollRect.horizontalScrollbar.interactable = true;
        utils.SetInputStopped(false);
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
        {
            throw new System.ArgumentException("utils.selectedCards must be of size 1");
        }

        GameObject selectedCard = utils.selectedCards[0];
        CardScript selectedCardScript = selectedCard.GetComponent<CardScript>();

        if (input.CompareTag("Card"))
        {
            CardScript inputCardScript = input.GetComponent<CardScript>();

            if (utils.IsMatch(input, selectedCard))
            {
                utils.Match(input, selectedCard);
            }
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

        utils.CheckNextCycle();
        utils.CheckGameOver();
    }
}
