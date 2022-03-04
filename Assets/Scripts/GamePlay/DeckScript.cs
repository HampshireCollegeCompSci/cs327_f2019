using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckScript : MonoBehaviour
{
    public List<GameObject> cardList;

    private Image buttonImage;
    public Sprite[] buttonAnimation;
    public Text deckCounter;

    // Singleton instance.
    public static DeckScript Instance = null;

    // Initialize the singleton instance.
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            buttonImage = this.gameObject.GetComponent<Image>();
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
        card.GetComponent<CardScript>().SetGameplayVisibility(false);
        UpdateDeckCounter();
    }

    public void RemoveCard(GameObject card)
    {
        cardList.Remove(card);
        card.GetComponent<CardScript>().SetGameplayVisibility(true);
        UpdateDeckCounter();
    }

    public void ProcessAction()
    {
        // is called by the deck button
        // user wants to deal cards, other things might need to be done before that

        // don't allow dealing when other stuff is happening
        if (UtilsScript.Instance.IsInputStopped())
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
            yield return new WaitForSeconds(0.08f);
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
            yield return new WaitForSeconds(0.08f);
        }
    }

    // moves all of the top foundation cards into their appropriate reactors
    public void StartNextCycle(bool manuallyTriggered = false)
    {
        if (!(manuallyTriggered && UtilsScript.Instance.IsInputStopped())) // stops 2 NextCycles from happening at once
        {
            UtilsScript.Instance.SetInputStopped(true, nextCycle: true);
            StartCoroutine(NextCycle());
        }
    }

    IEnumerator NextCycle()
    {
        SpaceBabyController.Instance.BabyActionCounter();

        FoundationScript currentFoundation;
        GameObject topFoundationCard;
        CardScript topCardScript;

        foreach (GameObject foundation in Config.Instance.foundations)
        {
            currentFoundation = foundation.GetComponent<FoundationScript>();
            if (currentFoundation.cardList.Count != 0)
            {
                topFoundationCard = currentFoundation.cardList[0];
                topCardScript = topFoundationCard.GetComponent<CardScript>();

                foreach (GameObject reactor in Config.Instance.reactors)
                {
                    if (topCardScript.suit == reactor.GetComponent<ReactorScript>().suit)
                    {
                        topCardScript.HideHologram();
                        topFoundationCard.GetComponent<SpriteRenderer>().sortingLayerName = "SelectedCards";
                        topCardScript.suitObject.GetComponent<SpriteRenderer>().sortingLayerName = "SelectedCards";
                        topCardScript.rankObject.GetComponent<MeshRenderer>().sortingLayerName = "SelectedCards";

                        Vector3 target = reactor.transform.position;
                        int cardCount = reactor.GetComponent<ReactorScript>().cardList.Count;
                        if (cardCount > 4)
                            cardCount = 4;

                        target.y += -0.8f + cardCount * 0.45f;
                        target.x -= 0.02f;

                        // immediately unhide the next possible top foundation card and start its hologram
                        if (currentFoundation.cardList.Count > 1)
                        {
                            CardScript nextTopFoundationCard = currentFoundation.cardList[1].GetComponent<CardScript>();
                            if (nextTopFoundationCard.IsHidden)
                            {
                                nextTopFoundationCard.SetFoundationVisibility(true, isNotForNextCycle: false);
                                nextTopFoundationCard.ShowHologram();
                            }
                        }

                        while (topFoundationCard.transform.position != target)
                        {   
                            topFoundationCard.transform.position = Vector3.MoveTowards(topFoundationCard.transform.position, target,
                                Time.deltaTime * Config.GameValues.cardsToReactorspeed);
                            yield return null;
                        }

                        topFoundationCard.GetComponent<SpriteRenderer>().sortingLayerName = "Gameplay";
                        topCardScript.suitObject.GetComponent<SpriteRenderer>().sortingLayerName = "Gameplay";
                        topCardScript.rankObject.GetComponent<MeshRenderer>().sortingLayerName = "Gameplay";

                        SoundEffectsController.Instance.CardToReactorSound();
                        topCardScript.MoveCard(reactor, isCycle: true);

                        if (Config.Instance.gameOver)
                        {
                            Config.Instance.moveCounter += 1;
                            yield break;
                        }

                        break;
                    }
                }
            }
        }

        UtilsScript.Instance.SetInputStopped(false, nextCycle: true);
        UtilsScript.Instance.UpdateActions(0, setAsValue: true);
    }

    public void UpdateDeckCounter()
    {
        if (cardList.Count != 0)
        {
            deckCounter.fontSize = 50;
            deckCounter.text = cardList.Count.ToString();
        }
        else
        {
            if (WastepileScript.Instance.cardList.Count > Config.GameValues.cardsToDeal)
            {
                deckCounter.fontSize = 45;
                deckCounter.text = "FLIP";
            }
            else
            {
                deckCounter.fontSize = 40;
                deckCounter.text = "EMPTY";
            }
        }
    }
}
