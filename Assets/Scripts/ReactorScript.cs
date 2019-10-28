using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactorScript : MonoBehaviour
{
    public List<GameObject> cardList;
    public UtilsScript utils;
    public SoundController soundController;

    GameObject myPrefab;
    public string suit;


    void Start()
    {
        utils = UtilsScript.global;
    }

    private void CheckGameOver()
    {
        if (CountReactorCard() >= Config.config.maxReactorVal)
        {
            Config.config.GetComponent<SoundController>().ReactorExplodeSound();
            myPrefab = (GameObject)Resources.Load("Prefabs/Explosion", typeof(GameObject));
            Instantiate(myPrefab, gameObject.transform.position, gameObject.transform.rotation);
            Destroy(gameObject);

            Config.config.gameOver = true;
            Config.config.gameWin = false;
        }
    }

    public void RemoveCard(GameObject card)
    {
        cardList.Remove(card);
    }

    public void SetCardPositions()
    {
        int positionCounter = 0;
        float yOffset = -0.5f;

        for (int i = cardList.Count - 1; i >= 0; i--)  // go backwards through the list
        {
            // as we go through, place cards above and in-front the previous one
            cardList[i].transform.position = gameObject.transform.position + new Vector3(0, yOffset, -1 - positionCounter * 0.1f);

            // 4 tokens can visibly fit in the container at once, so hide the bottom ones if over 4
            if (!(cardList.Count > 4 && positionCounter < cardList.Count - 4))
            {
                yOffset += 0.35f;
            }
            else
            {
                yOffset += 0;
            }

            positionCounter += 1;
        }

        // these two things need to be put in the correct ProcessAction() if statements
        //soundController.CardToReactorSound();
        CheckGameOver();
    }

    //this function is run on selected card's container
    //if click reactor then click other card,
    //click method gets run on container of first card clicked
    //know first card is from reactor
    //selectedCards = list of the currently selected cards
    //selectedCard[0] is the first card (from Reactor)
    //check if has more than 1 card -> shouldn't 
    //DON'T USE CLICKED CARD
    //take input (inputCard), which is the second card
    //match with them if they match
    public void ProcessAction(GameObject input)
    {
        GameObject card1 = utils.selectedCards[0];

        if (input.CompareTag("Card"))
        {
            //list needs to only be 1, something wrong if not -> skip to return
            if (utils.selectedCards.Count == 1)
            {
                if (utils.IsMatch(input, card1) && utils.selectedCards.Count == 1)
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

                    return;
                }
                else
                {
                    utils.DeselectCard(card1);
                }
            }
        }
    }

    public int CountReactorCard()
    {
        int totalSum = 0;
        int cardListVal = cardList.Count;
        for (int i = 0; i < cardListVal; i++)
        {
            totalSum += cardList[i].gameObject.GetComponent<CardScript>().cardVal;
        }

        return totalSum;
    }
}
