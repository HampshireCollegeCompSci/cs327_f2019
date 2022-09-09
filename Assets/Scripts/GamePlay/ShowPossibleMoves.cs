using System.Collections.Generic;
using UnityEngine;

public class ShowPossibleMoves
{
    private GameObject reactorMove;
    private List<GameObject> foundationMoves;
    private List<GameObject> cardMoves;
    private List<GameObject> cardMatches;

    public bool reactorIsGlowing;
    public bool foundationIsGlowing;
    public bool moveTokensAreGlowing;
    public bool matchTokensAreGlowing;

    public ShowPossibleMoves()
    {
        foundationMoves = new List<GameObject>();
        cardMoves = new List<GameObject>();
        cardMatches = new List<GameObject>();
        TokenMoveable = true;
        ReactorObstructed = false;
    }

    public bool AreThingsGlowing()
    {
        return reactorIsGlowing || foundationIsGlowing || moveTokensAreGlowing || matchTokensAreGlowing;
    }

    public bool AreCardsGlowing()
    {
        return moveTokensAreGlowing || matchTokensAreGlowing;
    }

    private bool _tokenMoveable;
    public bool TokenMoveable
    {
        get { return _tokenMoveable; }
        set { _tokenMoveable = value; }
    }

    private bool _reactorObstructed;
    public bool ReactorObstructed
    {
        get { return _reactorObstructed; }
        set { _reactorObstructed = value; }
    }

    public void ShowMoves(GameObject selectedCard)
    {
        if (!TokenMoveable) return;

        cardMoves.Clear();
        cardMatches.Clear();
        foundationMoves.Clear();
        reactorMove = null;

        FindMoves(selectedCard);

        foreach (GameObject card in cardMoves)
        {
            card.GetComponent<CardScript>().GlowLevel = Constants.moveHighlightColorLevel;
            moveTokensAreGlowing = true;
        }

        foreach (GameObject card in cardMatches)
        {
            card.GetComponent<CardScript>().GlowLevel = Constants.matchHighlightColorLevel;
            matchTokensAreGlowing = true;
        }

        foreach (GameObject foundation in foundationMoves)
        {
            foundation.GetComponent<FoundationScript>().GlowLevel = Constants.moveHighlightColorLevel;
            foundationIsGlowing = true;
        }

        if (reactorMove != null)
        {
            reactorIsGlowing = true;
            ReactorScript reactorMoveScript = reactorMove.GetComponent<ReactorScript>();

            // disable the top cards hitbox for the reactors hitbox to be on top
            // the top card can normally be clicked and dragged to match with other cards
            if (reactorMoveScript.cardList.Count != 0)
                reactorMoveScript.cardList[0].GetComponent<CardScript>().HitBox = false;

            // if moving the card into the reactor will lose us the game
            if (reactorMoveScript.CountReactorCard() + selectedCard.GetComponent<CardScript>().cardReactorValue >
                Config.Instance.reactorLimit)
            {
                reactorMoveScript.GlowLevel = Constants.overHighlightColorLevel;
            }
            else
            {
                reactorMoveScript.GlowLevel = Constants.moveHighlightColorLevel;
            }
        }

        //Debug.Log($"{moveTokensAreGlowing}{matchTokensAreGlowing}{foundationIsGlowing}{reactorIsGlowing}");
    }

    private void FindMoves(GameObject selectedCard)
    {
        CardScript selectedCardScript = selectedCard.GetComponent<CardScript>();

        bool cardIsFromFoundation = selectedCardScript.container.CompareTag(Constants.foundationTag);
        bool cardIsFromWastepile = selectedCardScript.container.CompareTag(Constants.wastepileTag);

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
            ReactorScript complimentaryReactorScript = UtilsScript.Instance.reactorScripts[
                CardTools.GetComplimentarySuit(selectedCardScript.cardSuitIndex)];

            if (complimentaryReactorScript.cardList.Count != 0)
            {
                CardScript cardScript = complimentaryReactorScript.cardList[0].GetComponent<CardScript>();
                // during the tutorial, the top card may be obstructed at times
                if (!cardScript.Obstructed && cardScript.cardRank == selectedCardScript.cardRank)
                {
                    cardMatches.Add(complimentaryReactorScript.cardList[0]);
                }
            }

            // if the reactor is not obstructed (tutorial setting)
            // and the card is not in the reactor, get the reactor that we can move into
            if (!ReactorObstructed && !selectedCardScript.container.CompareTag(Constants.reactorTag))
            {
                reactorMove = UtilsScript.Instance.reactorScripts[selectedCardScript.cardSuitIndex].gameObject;
            }
        }

        foreach (FoundationScript foundationScript in UtilsScript.Instance.foundationScripts)
        {
            if (foundationScript.cardList.Count != 0)
            {
                CardScript topFoundationCardScript = foundationScript.cardList[0].GetComponent<CardScript>();

                // during the tutorial, the top card may be obstructed at times
                if (!topFoundationCardScript.Obstructed)
                {
                    // if the card can match and matches with the foundation top
                    if (cardCanBeMatched && CardTools.CanMatch(selectedCardScript, topFoundationCardScript, checkIsTop: false))
                    {
                        cardMatches.Add(foundationScript.cardList[0]);
                    }
                    // if the card is not from a reactor can it stack?
                    else if ((cardIsFromFoundation || cardIsFromWastepile) &&
                        topFoundationCardScript.cardRank == selectedCardScript.cardRank + 1)
                    {
                        cardMoves.Add(foundationScript.cardList[0]);
                    }
                }
            }
            else if (cardIsFromFoundation || cardIsFromWastepile)
            {
                foundationMoves.Add(foundationScript.gameObject);
            }
        }

        // if the card can match and matches with the wastepile top
        if (cardCanBeMatched && WastepileScript.Instance.cardList.Count != 0)
        {
            GameObject topWastepileCard = WastepileScript.Instance.cardList[0];
            CardScript cardScript = topWastepileCard.GetComponent<CardScript>();

            // during the tutorial, the top card may be obstructed at times
            if (!cardScript.Obstructed && CardTools.CanMatch(cardScript, selectedCardScript, checkIsTop: false))
            {
                cardMatches.Add(topWastepileCard);
            }
        }
    }

    public void HideMoves()
    {
        if (!TokenMoveable) return;

        reactorIsGlowing = false;
        foundationIsGlowing = false;
        moveTokensAreGlowing = false;
        matchTokensAreGlowing = false;

        foreach (GameObject card in cardMoves)
        {
            card.GetComponent<CardScript>().Glowing = false;
        }

        foreach (GameObject card in cardMatches)
        {
            card.GetComponent<CardScript>().Glowing = false;
        }

        foreach (GameObject card in foundationMoves)
        {
            card.GetComponent<FoundationScript>().Glowing = false;
        }

        if (reactorMove != null)
        {
            ReactorScript reactorMoveScript = reactorMove.GetComponent<ReactorScript>();

            // re-enable the top cards hitbox because it was disabled for the reactors hitbox to be on top
            // the top card can normally be clicked and dragged to match with other cards
            if (reactorMoveScript.cardList.Count != 0)
            {
                reactorMoveScript.cardList[0].GetComponent<CardScript>().HitBox = true;
            }

            reactorMoveScript.Glowing = false;
        }
    }
}
