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
        reactorMove = null;
        foundationMoves = new(4);
        cardMoves = new(4);
        cardMatches = new(1);
        TokenMoveable = true;
        ReactorObstructed = false;
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

    public bool AreThingsGlowing()
    {
        return reactorIsGlowing || foundationIsGlowing || moveTokensAreGlowing || matchTokensAreGlowing;
    }

    public bool AreCardsGlowing()
    {
        return moveTokensAreGlowing || matchTokensAreGlowing;
    }

    public void ShowMoves(CardScript selectedCardScript)
    {
        if (!TokenMoveable) return;

        cardMoves.Clear();
        cardMatches.Clear();
        foundationMoves.Clear();
        reactorMove = null;

        FindMoves(selectedCardScript);

        foreach (GameObject card in cardMoves)
        {
            card.GetComponent<CardScript>().GlowColor = GameValues.Colors.Highlight.move;
            moveTokensAreGlowing = true;
        }

        foreach (GameObject card in cardMatches)
        {
            card.GetComponent<CardScript>().GlowColor = GameValues.Colors.Highlight.match;
            matchTokensAreGlowing = true;
        }

        foreach (GameObject foundation in foundationMoves)
        {
            foundation.GetComponent<FoundationScript>().GlowColor = GameValues.Colors.Highlight.move;
            foundationIsGlowing = true;
        }

        if (reactorMove != null)
        {
            reactorIsGlowing = true;
            ReactorScript reactorMoveScript = reactorMove.GetComponent<ReactorScript>();

            // disable the top cards hitbox for the reactors hitbox to be on top
            // the top card can normally be clicked and dragged to match with other cards
            if (reactorMoveScript.CardList.Count != 0)
            {
                reactorMoveScript.CardList[^1].GetComponent<CardScript>().HitBox = false;
            }

            // if moving the card into the reactor will lose us the game
            if (reactorMoveScript.CountReactorCard() + selectedCardScript.Card.Rank.ReactorValue >
                Config.Instance.CurrentDifficulty.ReactorLimit)
            {
                reactorMoveScript.GlowColor = GameValues.Colors.Highlight.over;
            }
            else
            {
                reactorMoveScript.GlowColor = GameValues.Colors.Highlight.move;
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
                reactorMoveScript.CardList[^1].GetComponent<CardScript>().HitBox = true;
            }

            reactorMoveScript.Glowing = false;
        }
    }

    private void FindMoves(CardScript selectedCardScript)
    {
        bool cardIsFromFoundation = selectedCardScript.Container.CompareTag(Constants.Tags.foundation);
        bool cardIsFromWastepile = selectedCardScript.Container.CompareTag(Constants.Tags.wastepile);

        // if the card is not in a foundation or not at the top of a foundation
        bool cardCanBeMatched = !(cardIsFromFoundation &&
            !selectedCardScript.Equals(selectedCardScript.Container.GetComponent<FoundationScript>().CardList[^1].GetComponent<CardScript>()));

        // find moves that can only occur when dragging only one token/card
        if (cardCanBeMatched)
        {
            // check if there is a card in a reactor that can be matched with
            ReactorScript complimentaryReactorScript = UtilsScript.Instance.reactorScripts[
                Suit.GetComplementaryIndex(selectedCardScript.Card.Suit)];
            if (complimentaryReactorScript.CardList.Count != 0)
            {
                CardScript topCardScript = complimentaryReactorScript.CardList[^1].GetComponent<CardScript>();
                // during the tutorial, the top card may be obstructed at times
                if (!topCardScript.Obstructed && Card.CanMatch(topCardScript.Card, selectedCardScript.Card))
                {
                    cardMatches.Add(complimentaryReactorScript.CardList[^1]);
                }
            }

            // if the reactor is not obstructed (tutorial setting)
            // and the card is not in the reactor, get the reactor that we can move into
            if (!ReactorObstructed && !selectedCardScript.Container.CompareTag(Constants.Tags.reactor))
            {
                reactorMove = UtilsScript.Instance.reactorScripts[selectedCardScript.Card.Suit.Index].gameObject;
            }
        }

        foreach (FoundationScript foundationScript in UtilsScript.Instance.foundationScripts)
        {
            if (foundationScript.CardList.Count != 0)
            {
                CardScript topFoundationCardScript = foundationScript.CardList[^1].GetComponent<CardScript>();

                // during the tutorial, the top card may be obstructed at times
                if (!topFoundationCardScript.Obstructed)
                {
                    // if the card can match and matches with the foundation top
                    if (cardCanBeMatched && Card.CanMatch(selectedCardScript.Card, topFoundationCardScript.Card))
                    {
                        cardMatches.Add(foundationScript.CardList[^1]);
                    }
                    // if the card is not from a reactor can it stack?
                    else if ((cardIsFromFoundation || cardIsFromWastepile) &&
                        topFoundationCardScript.Card.Rank.Value == selectedCardScript.Card.Rank.Value + 1)
                    {
                        cardMoves.Add(foundationScript.CardList[^1]);
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
            GameObject topWastepileCard = WastepileScript.Instance.CardList[^1];
            CardScript cardScript = topWastepileCard.GetComponent<CardScript>();

            // during the tutorial, the top card may be obstructed at times
            if (!cardScript.Obstructed && Card.CanMatch(cardScript.Card, selectedCardScript.Card))
            {
                cardMatches.Add(topWastepileCard);
            }
        }
    }
}
