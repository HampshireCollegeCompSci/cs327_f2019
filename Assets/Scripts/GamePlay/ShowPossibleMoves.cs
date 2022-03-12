using System.Collections.Generic;
using UnityEngine;

public class ShowPossibleMoves : MonoBehaviour
{
    private GameObject reactorMove;
    private List<GameObject> foundationMoves;
    private List<GameObject> cardMoves;
    private List<GameObject> cardMatches;

    public bool reactorIsGlowing;
    public bool foundationIsGlowing;
    public bool moveTokensAreGlowing;
    public bool matchTokensAreGlowing;

    // Singleton instance.
    public static ShowPossibleMoves Instance = null;

    // Initialize the singleton instance.
    private void Awake()
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

    private void Start()
    {
        foundationMoves = new List<GameObject>();
        cardMoves = new List<GameObject>();
        cardMatches = new List<GameObject>();
    }

    public bool AreThingsGlowing()
    {
        return reactorIsGlowing || foundationIsGlowing || moveTokensAreGlowing || matchTokensAreGlowing;
    }

    public bool AreCardsGlowing()
    {
        return moveTokensAreGlowing || matchTokensAreGlowing;
    }

    public void ShowMoves(GameObject selectedCard)
    {
        cardMoves.Clear();
        cardMatches.Clear();
        foundationMoves.Clear();
        reactorMove = null;

        FindMoves(selectedCard);

        foreach (GameObject card in cardMoves)
        {
            card.GetComponent<CardScript>().GlowOn(false);
            moveTokensAreGlowing = true;
        }

        foreach (GameObject card in cardMatches)
        {
            card.GetComponent<CardScript>().GlowOn(true);
            matchTokensAreGlowing = true;
        }

        foreach (GameObject foundation in foundationMoves)
        {
            foundation.GetComponent<FoundationScript>().GlowOn();
            foundationIsGlowing = true;
        }

        if (reactorMove != null)
        {
            reactorIsGlowing = true;
            ReactorScript reactorMoveScript = reactorMove.GetComponent<ReactorScript>();

            // disable the top cards hitbox for the reactors hitbox to be on top
            // the top card can normally be clicked and dragged to match with other cards
            if (reactorMoveScript.cardList.Count != 0)
                reactorMoveScript.cardList[0].GetComponent<BoxCollider2D>().enabled = false;

            // if moving the card into the reactor will lose us the game
            if (reactorMoveScript.CountReactorCard() +
                selectedCard.GetComponent<CardScript>().cardVal >= Config.Instance.maxReactorVal)
            {
                reactorMoveScript.GlowOn(2);
            }
            else
            {
                reactorMoveScript.GlowOn(1);
            }
        }
    }

    private void FindMoves(GameObject selectedCard)
    {
        CardScript selectedCardScript = selectedCard.GetComponent<CardScript>();

        bool cardIsFromFoundation = selectedCardScript.container.CompareTag("Foundation");
        bool cardIsFromWastepile = selectedCardScript.container.CompareTag("Wastepile");

        bool cardCanBeMatched = true;
        // if the card is in a foundation and not at the top of it
        if (cardIsFromFoundation && selectedCard !=
            selectedCardScript.container.GetComponent<FoundationScript>().cardList[0])
        {
            cardCanBeMatched = false;
        }

        // find moves that can only occur when dragging only one token/card
        if (cardCanBeMatched)
        {
            // find the one complimentary reactor and check if a top card exists and can then match
            ReactorScript reactorScriptRef;
            foreach (GameObject reactor in Config.Instance.reactors)
            {
                reactorScriptRef = reactor.GetComponent<ReactorScript>();
                if (CardTools.CompareComplimentarySuits(selectedCardScript.suit, reactorScriptRef.suit))
                {
                    if (reactorScriptRef.cardList.Count != 0 &&
                        reactorScriptRef.cardList[0].GetComponent<CardScript>().cardNum == selectedCardScript.cardNum)
                    {
                        cardMatches.Add(reactor.GetComponent<ReactorScript>().cardList[0]);
                    }

                    break;
                }
            }

            // if the card is not in the reactor, get the reactor that we can move into
            if (!selectedCardScript.container.CompareTag("Reactor"))
            {                
                foreach (GameObject reactor in Config.Instance.reactors)
                {
                    if (selectedCardScript.suit == reactor.GetComponent<ReactorScript>().suit)
                    {
                        reactorMove = reactor;
                        break;
                    }
                }
            }
        }

        FoundationScript foundationScriptRef;
        CardScript topFoundationCardScript;
        foreach (GameObject foundation in Config.Instance.foundations)
        {
            foundationScriptRef = foundation.GetComponent<FoundationScript>();
            if (foundationScriptRef.cardList.Count != 0)
            {
                topFoundationCardScript = foundationScriptRef.cardList[0].GetComponent<CardScript>();

                // if the card can match and matches with the foundation top
                if (cardCanBeMatched && CardTools.CanMatch(selectedCardScript, topFoundationCardScript, checkIsTop: false))
                {
                    cardMatches.Add(foundationScriptRef.cardList[0]);
                }
                // if the card is not from a reactor can it stack?
                else if ((cardIsFromFoundation || cardIsFromWastepile) &&
                    topFoundationCardScript.cardNum == selectedCardScript.cardNum + 1)
                {
                    cardMoves.Add(foundationScriptRef.cardList[0]);
                }
            }
            else if (cardIsFromFoundation || cardIsFromWastepile)
            {
                foundationMoves.Add(foundation);
            }
        }

        // if the card can match and matches with the wastepile top
        if (cardCanBeMatched && WastepileScript.Instance.cardList.Count != 0)
        {
            GameObject topWastepileCard = WastepileScript.Instance.cardList[0];
            if (CardTools.CanMatch(topWastepileCard.GetComponent<CardScript>(), selectedCardScript, checkIsTop: false))
            {
                cardMatches.Add(topWastepileCard);
            }
        }
    }

    public void HideMoves()
    {
        reactorIsGlowing = false;
        foundationIsGlowing = false;
        moveTokensAreGlowing = false;
        matchTokensAreGlowing = false;

        foreach (GameObject card in cardMoves)
        {
            card.GetComponent<CardScript>().GlowOff();
        }

        foreach (GameObject card in cardMatches)
        {
            card.GetComponent<CardScript>().GlowOff();
        }

        foreach (GameObject card in foundationMoves)
        {
            card.GetComponent<FoundationScript>().GlowOff();
        }

        if (reactorMove != null)
        {
            ReactorScript reactorMoveScript = reactorMove.GetComponent<ReactorScript>();

            // re-enable the top cards hitbox because it was disabled for the reactors hitbox to be on top
            // the top card can normally be clicked and dragged to match with other cards
            if (reactorMoveScript.cardList.Count != 0)
            {
                reactorMoveScript.cardList[0].GetComponent<BoxCollider2D>().enabled = true;
            }

            reactorMoveScript.GlowOff();
        }
    }
}
