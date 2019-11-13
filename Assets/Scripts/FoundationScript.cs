using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationScript : MonoBehaviour
{
    public UtilsScript utils;
    public List<GameObject> cardList;
    public SoundController soundController;

    void Start()
    {
        utils = UtilsScript.global;
        SetCardPositions();
    }

    private void Update()
    {
        CheckHologram();
    }

    public void CheckHologram()
    {
        if (cardList.Count != 0)
        {
            cardList[0].gameObject.GetComponent<CardScript>().ShowHologram();

            for (int i = 1; i < cardList.Count; i++)
            {
                cardList[i].GetComponent<CardScript>().DestroyHologram();
            }
        }
    }

    public void RemoveCard(GameObject card)
    {
        card.GetComponent<CardScript>().DestroyHologram();
        cardList.Remove(card);
        CheckTopCard();
    }

    public void CheckTopCard()
    {
        if (cardList.Count != 0 && cardList[0].gameObject.GetComponent<CardScript>().hidden)
        {
            cardList[0].gameObject.GetComponent<CardScript>().hidden = false;
            cardList[0].gameObject.GetComponent<CardScript>().SetCardAppearance();

            Config.config.GetComponent<SoundController>().CardRevealSound();
        }
    }

    //assigns card positions and render order and sets this foundation as the cards parents

    //iterate over the cardlist
    //for each one there, copy transform of foundation
    //apply to the card
    //offset card y axis by a little bit
    //offset card z axis by a little bit
    public void SetCardPositions()
    {
        int positionCounter = 0;
        float yOffset = 0;

        for (int i = cardList.Count - 1; i >= 0; i--) // go backwards through the list
        {
            // as we go through, place cards above and in-front the previous one
            cardList[i].transform.position = gameObject.transform.position + new Vector3(0, yOffset, -1 - positionCounter * 0.1f);

            if (cardList[i].GetComponent<CardScript>().hidden)  // don't show hidden cards as much
            {
                if (cardList.Count > 12) // especially if the stack is large
                {
                    yOffset += 0.05f;
                }
                else if (cardList.Count > 10) // less so if the stack is medium
                {
                    yOffset += 0.1f;
                }
                else
                {
                    yOffset += 0.2f;
                }
            }
            else
            {
                yOffset += 0.35f;
            }

            positionCounter++;
        }

    }

    public void ProcessAction(GameObject input)
    {
        Debug.Log("matchcheck");

        if (!input.CompareTag("Card"))
        {
            if ((input.CompareTag("Foundation") || input.CompareTag("Reactor")) && utils.selectedCards.Count != 0)
            {
                if (input.CompareTag("Reactor") && (utils.selectedCards.Count != 1 || utils.selectedCards[0].GetComponent<CardScript>().cardSuit != input.GetComponent<ReactorScript>().suit))
                {
                    return;
                }

                if (input.CompareTag("Reactor") || input.CompareTag("Foundation") && input.GetComponent<FoundationScript>().cardList.Count == 0)
                {
                    utils.selectedCards[0].GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container);
                    for (int i = 1; i < utils.selectedCards.Count; i++) //goes through and moves all selesctedCards to clicked location
                    {
                        utils.selectedCards[i].GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container, true, false);
                    }

                    Config.config.actions += 1; //adds to the action count
                }
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
                Debug.Log("matched");
            }
        }

        else if (input.GetComponent<CardScript>().container.CompareTag("Foundation") &&
            utils.selectedCards[0].GetComponent<CardScript>().cardNum + 1 ==
            input.GetComponent<CardScript>().container.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().cardNum) //this is checking the inputs containers top card allowing you to click on 
        {

            soundController.CardStackSound();

            utils.selectedCards[0].GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container);
            for (int i = 1; i<utils.selectedCards.Count;i++) //goes through and moves all selesctedCards to clicked location
            {
                utils.selectedCards[i].GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container, true, false);
            }

            Config.config.actions += 1; //adds to the action count
        }

        else if (input.GetComponent<CardScript>().container.CompareTag("Reactor") && utils.IsSameSuit(input, utils.selectedCards[0]) && utils.selectedCards.Count == 1)
        {
            utils.selectedCards[0].GetComponent<CardScript>().MoveCard(input.GetComponent<CardScript>().container);

            Config.config.actions += 1; //adds to the action count
        }
    }
}
