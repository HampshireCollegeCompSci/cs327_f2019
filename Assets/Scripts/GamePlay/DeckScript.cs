using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DeckScript : MonoBehaviour, ICardContainer, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private const string deckFlipText = "FLIP";
    private const string deckEmptyText = "EMPTY";

    private static readonly WaitForSeconds buttonWait = new(0.04f);

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

    private bool mouseOverButton, mousePressingButton;
    private Coroutine buttonCoroutine;

    public bool ButtonReady { get; set; }

    public DeckScript()
    {
        cardList = new(GameValues.GamePlay.cardCount);
        ButtonReady = true;
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
            return;
        }

        // if there are enough cards that a deck flip will do something worthwhile
        // notice: cards are removed from containers before they are added to a new one
        bool cardsToFlip = (WastepileScript.Instance.CardList.Count > GameValues.GamePlay.cardsToDeal) ||
            (dealed && WastepileScript.Instance.CardList.Count == GameValues.GamePlay.cardsToDeal);
        deckCounter.text = cardsToFlip ? deckFlipText : deckEmptyText;
        deckCounter.fontSize = 18;
    }

    public void TryUpdateDeckCounter(bool canFlip)
    {
        if (cardList.Count != 0) return;
        deckCounter.text = canFlip ? deckFlipText : deckEmptyText;
        deckCounter.fontSize = 18;
    }

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
        if (!ButtonReady || GameInput.Instance.InputStopped) return;
        ButtonReady = false;
        mousePressingButton = true;
        GameInput.Instance.InputStopped = true;

        if (buttonCoroutine != null)
        {
            StopCoroutine(buttonCoroutine);
        }
        buttonCoroutine = StartCoroutine(ButtonDown());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!mousePressingButton) return;
        mousePressingButton = false;
        GameInput.Instance.InputStopped = false;
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
        ButtonReady = true;
    }

    private void TryDealing()
    {
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
        else
        {
            // nothing is in the deck
            StartButtonUp();
        }
    }
}
