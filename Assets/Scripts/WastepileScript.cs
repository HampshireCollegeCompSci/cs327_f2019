using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WastepileScript : MonoBehaviour
{
    public UtilsScript utils;
    public SoundController soundController;
    public GameObject contentPanel;
    public GameObject cardContainer;
    public List<GameObject> cardList;
    public List<GameObject> cardContainers;
    private ScrollRect scrollRect;
    private bool scrolling;

    private void Start()
    {
        utils = UtilsScript.global;
        scrollRect = gameObject.GetComponent<ScrollRect>();
        scrolling = false;
    }

    //public void Update()
    //{
    //    Debug.Log(scrollRect.horizontalNormalizedPosition + "sb");
    //    Debug.Log(scrollRect.content.position.x + "content");
    //}

    public void CheckHologram(bool hide)
    {
        if (cardList.Count != 0)
        {
            cardList[0].gameObject.GetComponent<CardScript>().ShowHologram();

            if (hide)
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

    public bool isScrolling()
    {
        return scrolling;
    }
    public void AddCards(List<GameObject> cards)
    {
        // disable scrolling, flag it, and start the animation process
        scrollRect.horizontal = false;
        scrollRect.horizontalScrollbar.interactable = false;
        scrolling = true;
        StartCoroutine(UpdateScrollBar(cards));
    }

    IEnumerator UpdateScrollBar(List<GameObject> cards)
    {
        // first, try scrolling a little
        scrollRect.horizontalNormalizedPosition += 0.1f;
        yield return null;

        // if we are not adding the first cards and
        // if we scrolled (can we scroll check)
        if (cardList.Count != 0 && scrollRect.horizontalNormalizedPosition != 0)
        {
            // hide the hologram and scroll to the left (scroll rect is flipped btw)
            cardList[0].GetComponent<CardScript>().HideHologram();
            while (scrollRect.horizontalNormalizedPosition < 1)
            {
                yield return new WaitForSeconds(0.01f);
                scrollRect.horizontalNormalizedPosition += 0.1f;
            }
        }

        // add the new cards
        for (int i = 0; i < cards.Count; i++)
        {
            AddCard(cards[i]);
        }

        // we need unrestricted scroll for the following shenanigans
        scrollRect.movementType = ScrollRect.MovementType.Unrestricted;

        // move the scroll rect's content so that the new cards are hidden to the left side of the belt
        //Debug.Log(scrollRect.content.localPosition.x + " 0");
        Vector3 temp;
        temp = scrollRect.content.localPosition;
        temp.x = 3 + cards.Count;
        scrollRect.content.localPosition = temp;
        //Debug.Log(scrollRect.content.localPosition.x + " 1");

        // "scroll" the scroll rect's content back onto the belt
        // need to deal with floating points precision loss, it's going up by 0.000001 every 10 iterations
        double within = 3 + 0.01d;
        while (scrollRect.content.localPosition.x > within)
        {
            yield return new WaitForSeconds(.01f);
            temp = scrollRect.content.localPosition;
            temp.x -= 0.1f;
            scrollRect.content.localPosition = temp;
            //Debug.Log(scrollRect.content.localPosition.x);
        }

        temp = scrollRect.content.localPosition;
        temp.x = 0;
        scrollRect.content.localPosition = temp;
        //Debug.Log(scrollRect.content.localPosition.x + " 2");

        // set everything back to how it was
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.horizontal = true;
        scrollRect.horizontalScrollbar.interactable = true;
        scrolling = false;
    }

    public void AddCard(GameObject card)
    {
        card.SetActive(true);
        card.GetComponent<CardScript>().hidden = false;
        card.GetComponent<CardScript>().SetCardAppearance();
        card.GetComponent<CardScript>().container = gameObject;

        cardList.Insert(0, card);
        cardContainers.Insert(0, Instantiate(cardContainer));
        cardContainers[0].transform.SetParent(contentPanel.transform, false);
        card.transform.SetParent(cardContainers[0].transform);

        //cardContainers[0].GetComponent<LayoutElement>().preferredWidth = card.GetComponent<Renderer>().bounds.size.x;
        //cardContainers[0].GetComponent<LayoutElement>().preferredHeight = card.GetComponent<Renderer>().bounds.size.y;

        card.transform.parent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1);
        card.transform.parent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1);

        //LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)contentPanel.transform);

        card.transform.position = new Vector3(card.transform.parent.position.x, card.transform.parent.position.y, -1 - (cardList.Count * 0.01f));
        card.GetComponent<CardScript>().UpdateMaskInteraction(SpriteMaskInteraction.VisibleInsideMask);

        CheckHologram(true);

        //Debug.Log(gameObject.GetComponent<ScrollRect>().horizontalNormalizedPosition + " add");
        //StartCoroutine(UpdateScrollBar());
    }

    public void RemoveCard(GameObject card)
    {
        GameObject parentCardContainer = card.transform.parent.gameObject;
        card.transform.parent = null;
        cardContainers.Remove(parentCardContainer);
        Destroy(parentCardContainer);

        card.GetComponent<CardScript>().UpdateMaskInteraction(SpriteMaskInteraction.None);
        card.GetComponent<CardScript>().HideHologram();
        cardList.Remove(card);

        CheckHologram(false);

        //Debug.Log(gameObject.GetComponent<ScrollRect>().horizontalNormalizedPosition + " remove");
        //StartCoroutine(UpdateScrollBar());
    }

    public void DraggingCard(GameObject card, bool isDragging)
    {
        //Debug.Log(card.GetComponent<Renderer>().bounds.size.x);
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
        //Debug.Log(gameObject.GetComponent<ScrollRect>().horizontal);
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

                Config.config.actions += 1; //adds to the action count
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
            Config.config.actions += 1; //adds to the action count
        }

        else if (input.GetComponent<CardScript>().container.CompareTag("Reactor") && utils.IsSameSuit(input, utils.selectedCards[0]) && utils.selectedCards.Count == 1)
        {
            soundController.CardToReactorSound();
            utils.selectedCards[0].GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container);
            Config.config.actions += 1; //adds to the action count
        }

        return;
    }
    public List<GameObject> GetCardList()
    {
        return cardList;
    }

    public void SetCardPositions()
    { }
}
