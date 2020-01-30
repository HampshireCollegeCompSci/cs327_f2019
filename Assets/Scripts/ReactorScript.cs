﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactorScript : MonoBehaviour
{
    public List<GameObject> cardList;
    public UtilsScript utils;
    public GameObject gameUI;
    private ReactorScoreSetScript rsss;
    public SoundController soundController;

    public string suit;

    public Sprite glowSelected;
    public Sprite glowAlmostFull;
    private bool glowing = false;
    private bool alert = false;



    void Start()
    {
        utils = UtilsScript.global;
        rsss = gameUI.GetComponent<ReactorScoreSetScript>();
    }


    private void CheckGameOver()
    {
        if (CountReactorCard() >= Config.config.maxReactorVal && !Config.config.gameOver)
        {
            Config.config.GetComponent<SoundController>().LoseSound();
            Config.config.GameOver(false);
        }
    }

    public void AddCard(GameObject card, bool checkHolo = true)
    {
        card.GetComponent<CardScript>().HideHologram();
        cardList.Insert(0, card);
        card.transform.SetParent(gameObject.transform);

        SetCardPositions();
        soundController.CardToReactorSound();
        rsss.SetReactorScore();
        CheckGameOver();
    }

    public void RemoveCard(GameObject card, bool checkHolo = false)
    {
        cardList.Remove(card);
        SetCardPositions();
        rsss.SetReactorScore();
    }

    public void SetCardPositions()
    {
        int positionCounter = 0;
        float yOffset = -0.6f;

        for (int i = cardList.Count - 1; i >= 0; i--)  // go backwards through the list
        {
            // as we go through, place cards above and in-front the previous one
            cardList[i].transform.position = gameObject.transform.position + new Vector3(0, yOffset, -1 - positionCounter * 0.1f);

            // 4 tokens can visibly fit in the container at once, so hide the bottom ones if over 4
            if (!(cardList.Count > 4 && positionCounter < cardList.Count - 4))
            {
                yOffset += 0.45f;
            }
            else
            {
                yOffset += 0.05f;
            }

            positionCounter += 1;
        }
    }

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
                    utils.UnselectCards();
                }
            }
        }

        utils.CheckNextCycle();
        utils.CheckGameOver();
    }

    public int GetIncreaseOnNextCycle()
    {
        int output = 0;
        foreach (GameObject foundation in Config.config.foundations)
        {
            if (foundation.GetComponent<FoundationScript>().cardList.Count > 0)
            {
                if (foundation.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().cardSuit == suit)
                {
                    output += foundation.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().cardVal;
                }
            }
        }
        return output;
    }

    public int CountReactorCard()
    {
        int totalSum = 0;
        int cardListVal = cardList.Count;
        for (int i = 0; i < cardListVal; i++)
        {
            if (cardList[i].gameObject.GetComponent<CardScript>().cardSuit == suit)
            {
                totalSum += cardList[i].gameObject.GetComponent<CardScript>().cardVal;
            }
        }

        return totalSum;
    }

    public bool GlowOn()
    {
        if (!glowing && !alert)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = glowSelected;
            glowing = true;
            return true;
        }
        return false;
    }

    public bool GlowOff()
    {
        if (glowing && !alert)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = null;
            glowing = false;
            return true;
        }
        return false;
    }

    public bool AlertOn()
    {
        if (!alert)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = glowAlmostFull;
            alert = true;
            return true;
        }
        return false;
    }

    public bool AlertOff()
    {
        if (alert)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = null;
            alert = false;
            return true;
        }
        return false;
    }

    public bool isAlertOn()
    {
        return alert;
    }
}
