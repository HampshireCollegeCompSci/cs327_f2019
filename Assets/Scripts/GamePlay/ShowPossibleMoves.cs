using System.Collections.Generic;
using UnityEngine;

public class ShowPossibleMoves
{
    public bool reactorIsGlowing, foundationIsGlowing,
        moveTokensAreGlowing, matchTokensAreGlowing;

    [SerializeField]
    private GameObject reactorMove;
    [SerializeField]
    private List<GameObject> foundationMoves, cardMoves, cardMatches;

    public ShowPossibleMoves()
    {
        foundationMoves = new();
        cardMoves = new();
        cardMatches = new();
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
            if (reactorMoveScript.CardList.Count != 0)
                reactorMoveScript.CardList[0].GetComponent<CardScript>().HitBox = false;

            // if moving the card into the reactor will lose us the game
            if (reactorMoveScript.CountReactorCard() + selectedCard.GetComponent<CardScript>().CardReactorValue >
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
            if (reactorMoveScript.CardList.Count != 0)
            {
                reactorMoveScript.CardList[0].GetComponent<CardScript>().HitBox = true;
            }

            reactorMoveScript.Glowing = false;
        }
    }

    private void FindMoves(GameObject selectedCard)
    {
        CardScript selectedCardScript = selectedCard.GetComponent<CardScript>();

        bool cardIsFromFoundation = selectedCardScript.Container.CompareTag(Constants.foundationTag);
        bool cardIsFromWastepile = selectedCardScript.Container.CompareTag(Constants.wastepileTag);

        bool cardCanBeMatched = true;
        // if the card is in a foundation and not at the top of it
        if (cardIsFromFoundation && selectedCard !=
            selectedCardScript.Container.GetComponent<FoundationScript>().CardList[0])
        {
            cardCanBeMatched = false;
        }

        // find moves that can only occur when dragging only one token/card
        if (cardCanBeMatched)
        {
            ReactorScript complimentaryReactorScript = UtilsScript.Instance.reactorScripts[
                CardTools.GetComplimentarySuit(selectedCardScript.CardSuitIndex)];

            if (complimentaryReactorScript.CardList.Count != 0)
            {
                CardScript cardScript = complimentaryReactorScript.CardList[0].GetComponent<CardScript>();
                // during the tutorial, the top card may be obstructed at times
                if (!cardScript.Obstructed && cardScript.CardRank == selectedCardScript.CardRank)
                {
                    cardMatches.Add(complimentaryReactorScript.CardList[0]);
                }
            }

            // if the reactor is not obstructed (tutorial setting)
            // and the card is not in the reactor, get the reactor that we can move into
            if (!ReactorObstructed && !selectedCardScript.Container.CompareTag(Constants.reactorTag))
            {
                reactorMove = UtilsScript.Instance.reactorScripts[selectedCardScript.CardSuitIndex].gameObject;
            }
        }

        foreach (FoundationScript foundationScript in UtilsScript.Instance.foundationScripts)
        {
            if (foundationScript.CardList.Count != 0)
            {
                CardScript topFoundationCardScript = foundationScript.CardList[0].GetComponent<CardScript>();

                // during the tutorial, the top card may be obstructed at times
                if (!topFoundationCardScript.Obstructed)
                {
                    // if the card can match and matches with the foundation top
                    if (cardCanBeMatched && CardTools.CanMatch(selectedCardScript, topFoundationCardScript, checkIsTop: false))
                    {
                        cardMatches.Add(foundationScript.CardList[0]);
                    }
                    // if the card is not from a reactor can it stack?
                    else if ((cardIsFromFoundation || cardIsFromWastepile) &&
                        topFoundationCardScript.CardRank == selectedCardScript.CardRank + 1)
                    {
                        cardMoves.Add(foundationScript.CardList[0]);
                    }
                }
            }
            else if (cardIsFromFoundation || cardIsFromWastepile)
            {
                foundationMoves.Add(foundationScript.gameObject);
            }
        }

        // if the card can match and matches with the wastepile top
        if (cardCanBeMatched && WastepileScript.Instance.CardList.Count != 0)
        {
            GameObject topWastepileCard = WastepileScript.Instance.CardList[0];
            CardScript cardScript = topWastepileCard.GetComponent<CardScript>();

            // during the tutorial, the top card may be obstructed at times
            if (!cardScript.Obstructed && CardTools.CanMatch(cardScript, selectedCardScript, checkIsTop: false))
            {
                cardMatches.Add(topWastepileCard);
            }
        }
    }

    public bool TokenMoveable
    {
        get;
        set;
    }

    public bool ReactorObstructed
    {
        get;
        set;
    }
}
