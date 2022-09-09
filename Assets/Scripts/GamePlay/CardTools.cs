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

        if (card1.cardRank != card2.cardRank)
            return false;

        return CompareComplimentarySuits(card1.cardSuitIndex, card2.cardSuitIndex);
    }

    /// <summary>
    /// Checks if the card is at the top of its container's cardList. Currently only works on foundation cards.
    /// </summary>
    private static bool IsAtContainerTop(CardScript card)
    {
        // hitboxes are disabled for all cards not on the top for the reactor, wastepile, and deck
        // since they can't be picked up, only foundation cards need to be checked

        if (card.container.CompareTag(Constants.foundationTag) &&
            card.container.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>() != card)
        {
            return false;
        }

        /*
        if (card.container.CompareTag(Constants.reactorTag) &&
            card.container.GetComponent<ReactorScript>().cardList[0].GetComponent<CardScript>() != card)
        {
            return false;
        }

        if (card.container.CompareTag(Constants.wastepileTag) &&
            card.container.GetComponent<WastepileScript>().cardList[0].GetComponent<CardScript>() != card)
        {
            return false;
        }

        if (card.container.CompareTag(Constants.deckTag) &&
            card.container.GetComponent<DeckScript>().cardList[0].GetComponent<CardScript>() != card)
        {
            return false;
        }*/

        return true;
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
            0 => 1,
            1 => 0,
            2 => 3,
            3 => 2,
            _ => throw new System.ArgumentException("suit not found to be valid"),
        };
    }

    private static byte GetSuit(GameObject suitObject)
    {
        if (suitObject.CompareTag(Constants.cardTag))
        {
            return suitObject.GetComponent<CardScript>().cardSuitIndex;
        }

        if (suitObject.CompareTag(Constants.reactorTag))
        {
            return suitObject.GetComponent<ReactorScript>().reactorSuitIndex;
        }

        throw new System.ArgumentException("suitObject must have a suit variable");
    }
}
