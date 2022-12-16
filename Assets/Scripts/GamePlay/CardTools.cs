using UnityEngine;

public static class CardTools
{
    /// <summary>
    /// Checks if two cards can match together.
    /// </summary>
    /// <param name="checkIsTop">To check if the card is at the top of it's container.
    /// Currently, everything but foundation cards can have this be false</param>
    public static bool CanMatch(CardScript card1, CardScript card2, bool checkIsTop = true)
    {
        // checks if the cards are at the top of their containers
        if (checkIsTop && (!IsAtContainerTop(card1) || !IsAtContainerTop(card2)))
            return false;

        if (card1.CardRank != card2.CardRank)
            return false;

        return CompareComplimentarySuits(card1.CardSuitIndex, card2.CardSuitIndex);
    }

    /// <summary>
    /// Compares two game objects to see if they contain the same suit.
    /// The objects must be either a card or a reactor.
    /// </summary>
    /// <param name="object1">Must be either a Card or Reactor GameObject.</param>
    /// <param name="object2">Must be either a Card or Reactor GameObject.</param>
    public static bool CompareSameSuitObjects(GameObject object1, GameObject object2)
    {
        return GetSuit(object1) == GetSuit(object2);
    }

    /// <summary>
    /// Compares two game objects to see if they contain complimentary suits. Or, in other words, can the suits can match together.
    /// </summary>
    /// <param name="object1">Must be either a Card or Reactor GameObject.</param>
    /// <param name="object2">Must be either a Card or Reactor GameObject.</param>
    public static bool CompareComplimentarySuitObjects(GameObject object1, GameObject object2)
    {
        return CompareComplimentarySuits(GetSuit(object1), GetSuit(object2));
    }

    /// <summary>
    /// Compares two bytes to see if they are complimentary suits. Or, in other words, can the suits can match together.
    /// </summary>
    public static bool CompareComplimentarySuits(byte suit1, byte suit2)
    {
        return suit1 == GetComplimentarySuit(suit2);
    }

    /// <summary>
    /// Returns the complimentary suit to the given suit.
    /// </summary>
    public static byte GetComplimentarySuit(byte suit)
    {
        return suit switch
        {
            Constants.Suits.Spades.index => Constants.Suits.Clubs.index,
            Constants.Suits.Clubs.index => Constants.Suits.Spades.index,
            Constants.Suits.Diamonds.index => Constants.Suits.Hearts.index,
            Constants.Suits.Hearts.index => Constants.Suits.Diamonds.index,
            _ => throw new System.ArgumentException("suit not found to be valid"),
        };
    }

    /// <summary>
    /// Checks if the card is at the top of its container's cardList. Currently only works on foundation cards.
    /// </summary>
    private static bool IsAtContainerTop(CardScript card)
    {
        // hitboxes are disabled for all cards not on the top for the reactor, wastepile, and deck
        // since they can't be picked up, only foundation cards need to be checked

        if (card.Container.CompareTag(Constants.Tags.foundation) &&
            card.Container.GetComponent<FoundationScript>().CardList[^1].GetComponent<CardScript>() != card)
        {
            return false;
        }

        /*
        if (card.container.CompareTag(Constants.reactorTag) &&
            card.container.GetComponent<ReactorScript>().cardList[^1].GetComponent<CardScript>() != card)
        {
            return false;
        }

        if (card.container.CompareTag(Constants.wastepileTag) &&
            card.container.GetComponent<WastepileScript>().cardList[^1].GetComponent<CardScript>() != card)
        {
            return false;
        }

        if (card.container.CompareTag(Constants.deckTag) &&
            card.container.GetComponent<DeckScript>().cardList[^1].GetComponent<CardScript>() != card)
        {
            return false;
        }*/

        return true;
    }

    private static byte GetSuit(GameObject suitObject)
    {
        if (suitObject.CompareTag(Constants.Tags.card))
        {
            return suitObject.GetComponent<CardScript>().CardSuitIndex;
        }

        if (suitObject.CompareTag(Constants.Tags.reactor))
        {
            return suitObject.GetComponent<ReactorScript>().ReactorSuitIndex;
        }

        throw new System.ArgumentException("suitObject must have a suit variable");
    }
}
