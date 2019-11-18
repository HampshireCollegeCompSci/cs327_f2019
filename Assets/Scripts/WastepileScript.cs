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

    private void Start()
    {
        utils = UtilsScript.global;
    }

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

    public void AddCard(GameObject card)
    {
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

        Debug.Log(gameObject.GetComponent<ScrollRect>().horizontalNormalizedPosition + " add");
        StartCoroutine(UpdateScrollBar());
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

        Debug.Log(gameObject.GetComponent<ScrollRect>().horizontalNormalizedPosition + " remove");
        StartCoroutine(UpdateScrollBar());
    }

    IEnumerator UpdateScrollBar()
    {
        yield return null;
        gameObject.GetComponent<ScrollRect>().horizontalNormalizedPosition = 1;
        Debug.Log("scrolling");
    }

    public void DraggingCard(GameObject card, bool isDragging)
    {
        //Debug.Log(card.GetComponent<Renderer>().bounds.size.x);
        if (isDragging)
        {
            card.GetComponent<CardScript>().UpdateMaskInteraction(SpriteMaskInteraction.None);
            gameObject.GetComponent<ScrollRect>().horizontal = false;
        }
        else
        {
            if (card.GetComponent<CardScript>().container == this.gameObject)
            {
                card.GetComponent<CardScript>().UpdateMaskInteraction(SpriteMaskInteraction.VisibleInsideMask);
            }
            gameObject.GetComponent<ScrollRect>().horizontal = true;
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
