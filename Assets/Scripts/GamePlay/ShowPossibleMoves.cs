using System.Collections.Generic;
using UnityEngine;

public class ShowPossibleMoves
{
    public bool reactorIsGlowing, foundationIsGlowing,
        moveTokensAreGlowing, matchTokensAreGlowing;

    [SerializeField]
    public GameObject reactorMove, cardMatch;
    [SerializeField]
    public List<GameObject> foundationMoves, cardMoves;

    public ShowPossibleMoves()
    {
        reactorMove = null;
        cardMatch = null;
        foundationMoves = new(4);
        cardMoves = new(4);
        TokenMoveable = true;
        ReactorObstructed = false;
    }

    public bool TokenMoveable { get; set; }

    public bool ReactorObstructed { get; set; }

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

        FindMoves(selectedCardScript);

        if (cardMoves.Count != 0)
        {
            moveTokensAreGlowing = true;
        }
        foreach (GameObject card in cardMoves)
        {
            card.GetComponent<CardScript>().GlowColor = Config.Instance.CurrentColorMode.Move;
        }

        if (foundationMoves.Count != 0)
        {
            foundationIsGlowing = true;
        }
        foreach (GameObject foundation in foundationMoves)
        {
            foundation.GetComponent<FoundationScript>().GlowColor = Config.Instance.CurrentColorMode.Move;
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
            bool willMoveCauseGameOver = (reactorMoveScript.CardValueCount + selectedCardScript.Card.Rank.ReactorValue) >
                Config.Instance.CurrentDifficulty.ReactorLimit;
            reactorMoveScript.GlowColor = willMoveCauseGameOver ?
                Config.Instance.CurrentColorMode.Over :Config.Instance.CurrentColorMode.Move;
        }

        if (cardMatch != null)
        {
            cardMatch.GetComponent<CardScript>().GlowColor = Config.Instance.CurrentColorMode.Match;
            matchTokensAreGlowing = true;
        }
    }

    public void HideMoves()
    {
        if (!TokenMoveable) return;

        foreach (GameObject card in cardMoves)
        {
            card.GetComponent<CardScript>().Glowing = false;
        }
        moveTokensAreGlowing = false;
        cardMoves.Clear();

        foreach (GameObject card in foundationMoves)
        {
            card.GetComponent<FoundationScript>().Glowing = false;
        }
        foundationIsGlowing = false;
        foundationMoves.Clear();

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
            reactorIsGlowing = false;
            reactorMove = null;
        }

        if (cardMatch != null)
        {
            cardMatch.GetComponent<CardScript>().Glowing = false;
            matchTokensAreGlowing = false;
            cardMatch = null;
        }
    }

    private void FindMoves(CardScript selectedCardScript)
    {
        bool cardIsFromFoundation = selectedCardScript.CurrentContainerType == Constants.CardContainerType.Foundation;
        bool cardIsFromWastepile = selectedCardScript.CurrentContainerType == Constants.CardContainerType.WastePile;

        bool cardCanBeMatched = !GameInput.Instance.DraggingStack;

        // find moves that can only occur when dragging only one token/card
        if (cardCanBeMatched)
        {
            // check if there is a card in a reactor that can be matched with
            ReactorScript complimentaryReactorScript = GameInput.Instance.reactorScripts[
                Suit.GetComplementaryIndex(selectedCardScript.Card.Suit)];
            if (complimentaryReactorScript.CardList.Count != 0)
            {
                CardScript topCardScript = complimentaryReactorScript.CardList[^1].GetComponent<CardScript>();
                // during the tutorial, the top card may be obstructed at times
                if (!topCardScript.Obstructed && Card.CanMatch(topCardScript.Card, selectedCardScript.Card))
                {
                    cardMatch = topCardScript.gameObject;
                    cardCanBeMatched = false;
                }
            }

            // if the reactor is not obstructed (tutorial setting)
            // and the card is not in the reactor, get the reactor that we can move into
            if (!ReactorObstructed && selectedCardScript.CurrentContainerType != Constants.CardContainerType.Reactor)
            {
                reactorMove = GameInput.Instance.reactorScripts[selectedCardScript.Card.Suit.Index].gameObject;
            }
        }

        foreach (FoundationScript foundationScript in GameInput.Instance.foundationScripts)
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
                        cardMatch = topFoundationCardScript.gameObject;
                        cardCanBeMatched = false;
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
            CardScript cardScript = WastepileScript.Instance.CardList[^1].GetComponent<CardScript>();

            // during the tutorial, the top card may be obstructed at times
            if (!cardScript.Obstructed && Card.CanMatch(cardScript.Card, selectedCardScript.Card))
            {
                cardMatch = cardScript.gameObject;
                //cardCanBeMatched = false;
            }
        }
    }
}
