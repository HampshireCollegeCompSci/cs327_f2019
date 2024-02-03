﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DeckScript : MonoBehaviour, ICardContainer, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private const string deckFlipText = "FLIP";
    private const string deckEmptyText = "EMPTY";

    private static readonly WaitForSeconds buttonWait = new(0.06f);

    private int buttonAnimationIndex;

    // Singleton instance.
    public static DeckScript Instance { get; private set; }

    [SerializeField]
    private List<GameObject> cardList;

    [SerializeField]
    private Image buttonImage;
    [SerializeField]
    private Text deckCounter;
    [SerializeField]
    private Sprite[] buttonAnimation;

    private Coroutine buttonCoroutine;

    public DeckScript()
    {
        cardList = new(GameValues.GamePlay.cardCount);
    }

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

    public List<GameObject> CardList => cardList;

    public void AddCard(GameObject card)
    {
        cardList.Add(card);
        card.transform.SetParent(gameObject.transform);
        card.transform.localPosition = Vector3.zero;
        card.GetComponent<CardScript>().Enabled = false;
        UpdateDeckCounter();
    }

    public void RemoveCard(GameObject card)
    {
        cardList.RemoveAt(cardList.LastIndexOf(card));

        card.GetComponent<CardScript>().Enabled = true;
        UpdateDeckCounter(dealed: true);
    }

    public void Deal(bool doLog = true)
    {
        List<GameObject> toMoveList = new(GameValues.GamePlay.cardsToDeal);

        // try to deal set number of cards, take them starting from the top, [^1], down
        for (int i = 1; i <= GameValues.GamePlay.cardsToDeal && i <= cardList.Count; i++)
        {
            toMoveList.Add(cardList[^i]);
        }

        if (toMoveList.Count != 0)
        {
            if (Config.Instance.TutorialOn)
            {
                doLog = false;
            }

            WastepileScript.Instance.AddCards(toMoveList, doLog);
        }
    }

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
            if (WastepileScript.Instance.CardList.Count > GameValues.GamePlay.cardsToDeal ||
                (dealed && WastepileScript.Instance.CardList.Count == GameValues.GamePlay.cardsToDeal))
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

    public static bool mouseOverButton;
    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOverButton = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOverButton = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (buttonCoroutine != null)
        {
            StopCoroutine(buttonCoroutine);
        }
        buttonCoroutine = StartCoroutine(ButtonDown());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (mouseOverButton)
        {
            TryDealing();
        }
        else
        {
            StartButtonUp();
        }
    }

    public void StartButtonUp()
    {
        if (buttonCoroutine != null)
        {
            StopCoroutine(buttonCoroutine);
        }
        buttonCoroutine = StartCoroutine(ButtonUp());
    }

    private IEnumerator ButtonDown()
    {
        if (buttonAnimationIndex <= 0)
        {
            buttonAnimationIndex = 1;
        }

        for (; buttonAnimationIndex < buttonAnimation.Length; buttonAnimationIndex++)
        {
            buttonImage.sprite = buttonAnimation[buttonAnimationIndex];
            yield return buttonWait;
        }
        buttonCoroutine = null;
    }

    private IEnumerator ButtonUp()
    {
        if (buttonAnimationIndex >= buttonAnimation.Length - 1)
        {
            buttonAnimationIndex = buttonAnimation.Length - 2;
        }

        for (; buttonAnimationIndex >= 0; buttonAnimationIndex--)
        {
            buttonImage.sprite = buttonAnimation[buttonAnimationIndex];
            yield return buttonWait;
        }
        buttonCoroutine = null;
    }

    private void TryDealing()
    {
        // don't allow dealing when other stuff is happening
        if (GameInput.Instance.InputStopped) return;

        if (cardList.Count != 0) // can the deck can be drawn from
        {
            SoundEffectsController.Instance.DeckDeal();
            Deal();
        }
        // if it is possible to repopulate the deck
        else if (WastepileScript.Instance.CardList.Count > GameValues.GamePlay.cardsToDeal)
        {
            // moves all wastePile cards into the deck
            WastepileScript.Instance.StartDeckReset();
            AchievementsManager.FailedNoDeckFlip();
            SoundEffectsController.Instance.DeckReshuffle();
        }
    }
}
