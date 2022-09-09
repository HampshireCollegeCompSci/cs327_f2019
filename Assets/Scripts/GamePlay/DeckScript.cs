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

    private Coroutine buttonCoroutine;

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
        UpdateDeckCounter(dealed: true);
    }

    public void DealButton()
    {
        // don't allow dealing when other stuff is happening
        if (UtilsScript.Instance.InputStopped) return;

        if (cardList.Count != 0) // can the deck can be drawn from
        {
            buttonCoroutine = StartCoroutine(ButtonDown());
            SoundEffectsController.Instance.DeckDeal();
            Deal();
        }
        // if it is possible to repopulate the deck
        else if (WastepileScript.Instance.cardList.Count > Config.GameValues.cardsToDeal)
        {
            buttonCoroutine = StartCoroutine(ButtonDown());
            DeckReset();
        }
    }

    public void Deal(bool doLog = true)
    {
        List<GameObject> toMoveList = new List<GameObject>();
        int cardCount = cardList.Count;

        // try to deal set number of cards
        for (int i = 0; i < Config.GameValues.cardsToDeal && i < cardCount; i++)
        {
            toMoveList.Add(cardList[i]);
        }

        if (toMoveList.Count != 0)
        {
            if (Config.Instance.tutorialOn)
            {
                doLog = false;
            }

            WastepileScript.Instance.AddCards(toMoveList, doLog);
        }
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
        if (buttonCoroutine == null) return;
        StopCoroutine(buttonCoroutine);
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

    private const string deckFlipText = "FLIP";
    private const string deckEmptyText = "EMPTY";
    public void UpdateDeckCounter(bool dealed = false)
    {
        if (cardList.Count != 0)
        {
            deckCounter.fontSize = 25;
            deckCounter.text = cardList.Count.ToString();
        }
        else
        {
            // if there are enough cards that a deck flip will do something worthwhile
            // notice: cards are removed from containers before they are added to a new one
            if (WastepileScript.Instance.cardList.Count > Config.GameValues.cardsToDeal || 
                (dealed && WastepileScript.Instance.cardList.Count == Config.GameValues.cardsToDeal))
            {
                deckCounter.text = deckFlipText;
            }
            else
            {
                deckCounter.text = deckEmptyText;
            }
            deckCounter.fontSize = 18;
        }
    }

    public void TryUpdateDeckCounter(bool canFlip)
    {
        if (cardList.Count == 0)
        {
            if (canFlip)
            {
                deckCounter.text = deckFlipText;
            }
            else
            {
                deckCounter.text = deckEmptyText;
            }
        }
    }
}
