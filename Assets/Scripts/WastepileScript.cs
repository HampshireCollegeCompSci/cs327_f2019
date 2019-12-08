﻿using System.Collections;
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

    private ScrollRect scrollRect;
    private RectTransform contentRectTransform;
    private bool scrolling;

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
        scrolling = false;
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
                yield return new WaitForSeconds(0.01f);
                temp.x -= 0.2f;
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
        while (temp.x > 0.01f)
        {
            yield return new WaitForSeconds(.01f);
            temp.x -= 0.1f;
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

        //if (cardList.Count != 0)
        //{
        //    StartCoroutine(ScrollBarRemoving(x));
        //}
    }

    IEnumerator ScrollBarRemoving(GameObject parentCardContainer)
    {
        DisableScrolling();

        Vector3 temp = contentRectTransform.anchoredPosition;
        while (temp.x < 1)
        {
            temp.x += 0.1f;
            contentRectTransform.anchoredPosition = temp;
            yield return new WaitForSeconds(.01f);
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
            yield return new WaitForSeconds(.01f);
            temp.x += 0.3f;
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
        scrolling = true;

        // we need unrestricted scroll for later shenanigans
        scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
    }

    public void ResetScrollBar(Vector3 temp)
    {
        temp.x = 0;
        contentRectTransform.anchoredPosition = temp;

        // set everything back to how it was
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.horizontal = true;
        scrollRect.horizontalScrollbar.interactable = true;
        scrolling = false;
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
            if (card.GetComponent<CardScript>().container == this.gameObject)
            {
                card.GetComponent<CardScript>().UpdateMaskInteraction(SpriteMaskInteraction.VisibleInsideMask);
            }
            scrollRect.horizontal = true;
        }
    }

    public void ProcessAction(GameObject input)
    {
        if (!input.CompareTag("Card"))
        {
            if ((input.CompareTag("Foundation") || input.CompareTag("Reactor")) && utils.selectedCards.Count != 0)
            {
                if (input.CompareTag("Reactor") && (utils.selectedCards.Count != 1 || utils.selectedCards[0].GetComponent<CardScript>().cardSuit != input.GetComponent<ReactorScript>().suit))
                {
                    return;
                }
                if (input.CompareTag("Foundation") && input.GetComponent<FoundationScript>().cardList.Count > 0)
                {
                    return;
                }
                foreach (GameObject card in utils.selectedCards) //goes through and moves all selesctedCards to clicked location
                {
                    card.GetComponent<CardScript>().MoveCard(input);
                }

                utils.UpdateActionCounter(1);
            }
        }

        else if (utils.IsMatch(input, utils.selectedCards[0]) && utils.selectedCards.Count == 1) //check if selectedCards and the input card match and that selesctedCards is only one card
        {
            GameObject inputContainer = input.GetComponent<CardScript>().container;

            if (inputContainer.CompareTag("Foundation"))
            {
                if (inputContainer.GetComponent<FoundationScript>().cardList[0] == input)
                {
                    utils.Match(input, utils.selectedCards[0]); //removes the two matched cards
                }

                return;
            }

            if (inputContainer.CompareTag("Reactor"))
            {
                if (inputContainer.GetComponent<ReactorScript>().cardList[0] == input)
                {
                    utils.Match(input, utils.selectedCards[0]); //removes the two matched cards
                }

                return;
            }

            if (inputContainer.CompareTag("Wastepile"))
            {
                if (inputContainer.GetComponent<WastepileScript>().cardList[0] == input)
                {
                    utils.Match(input, utils.selectedCards[0]); //removes the two matched cards
                }

                return;
            }

            else
            {
                utils.Match(input, utils.selectedCards[0]); //removes the two matched cards
            }
        }

        else if (input.GetComponent<CardScript>().container.CompareTag("Foundation") &&
           utils.selectedCards[0].GetComponent<CardScript>().cardNum + 1 ==
           input.GetComponent<CardScript>().container.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().cardNum) //this is checking the inputs containers top card allowing you to click on 
        {
            soundController.CardStackSound();
            foreach (GameObject card in utils.selectedCards) //goes through and moves all selesctedCards to clicked location
            {
                card.GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container);
            }
            utils.UpdateActionCounter(1);
        }

        else if (input.GetComponent<CardScript>().container.CompareTag("Reactor") && utils.IsSameSuit(input, utils.selectedCards[0]) && utils.selectedCards.Count == 1)
        {
            soundController.CardToReactorSound();
            utils.selectedCards[0].GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container);
            utils.UpdateActionCounter(1);
        }

        utils.CheckNextCycle();
        utils.CheckGameOver();
    }
    public List<GameObject> GetCardList()
    {
        return cardList;
    }

    public bool isScrolling()
    {
        return scrolling;
    }

}
