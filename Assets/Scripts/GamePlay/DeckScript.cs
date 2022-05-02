using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckScript : MonoBehaviour, ICardContainer
{
    public List<GameObject> cardList;

    public Image buttonImage;
    public Text deckCounter;
    public Sprite[] buttonAnimation;

    // Singleton instance.
    public static DeckScript Instance = null;

    // Initialize the singleton instance.
    void Awake()
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

    public void AddCard(GameObject card)
    {
        cardList.Insert(0, card);
        card.transform.SetParent(gameObject.transform);
        card.transform.localPosition = Vector3.zero;
        card.GetComponent<CardScript>().Enabled = false;
        UpdateDeckCounter();
    }

    public void RemoveCard(GameObject card)
    {
        cardList.Remove(card);
        card.GetComponent<CardScript>().Enabled = true;
        UpdateDeckCounter();
    }

    public void DealButton()
    {
        // don't allow dealing when other stuff is happening
        if (UtilsScript.Instance.InputStopped)
            return;

        if (cardList.Count != 0) // can the deck can be drawn from
        {
            SoundEffectsController.Instance.DeckDeal();
            Deal();

            StartCoroutine(ButtonDown());
        }
        // if it is possible to repopulate the deck
        else if (WastepileScript.Instance.cardList.Count > Config.GameValues.cardsToDeal)
        {
            DeckReset();
            StartCoroutine(ButtonDown());
        }
    }

    public void Deal(bool doLog = true)
    {
        List<GameObject> toMoveList = new List<GameObject>();
        for (int i = 0; i < Config.GameValues.cardsToDeal; i++) // try to deal set number of cards
        {
            if (cardList.Count <= i) // are there no more cards in the deck?
                break;

            toMoveList.Add(cardList[i]);
        }

        if (toMoveList.Count != 0)
            WastepileScript.Instance.AddCards(toMoveList, doLog);
    }

    public void DeckReset()
    {
        // moves all wastePile cards into the deck

        WastepileScript.Instance.StartDeckReset();
        SoundEffectsController.Instance.DeckReshuffle();
    }

    IEnumerator ButtonDown()
    {
        foreach (Sprite button in buttonAnimation)
        {
            buttonImage.sprite = button;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void StartButtonUp()
    {
        StartCoroutine(ButtonUp());
    }

    IEnumerator ButtonUp()
    {
        for (int i = buttonAnimation.Length - 2; i > 0; i--)
        {
            buttonImage.sprite = buttonAnimation[i];
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void UpdateDeckCounter()
    {
        if (cardList.Count != 0)
        {
            deckCounter.fontSize = 25;
            deckCounter.text = cardList.Count.ToString();
        }
        else
        {
            if (WastepileScript.Instance.cardList.Count > Config.GameValues.cardsToDeal)
            {
                deckCounter.text = "FLIP";
            }
            else
            {
                deckCounter.text = "EMPTY";
            }
            deckCounter.fontSize = 19;
        }
    }
}
