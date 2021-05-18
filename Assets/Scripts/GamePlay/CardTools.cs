using UnityEngine;

public static class CardTools
{
    /// <summary>
    /// Checks if two cards can match together.
    /// </summary>
    /// <param name="card1"></param>
    /// <param name="card2"></param>
    /// <param name="checkIsTop">To check if the card is at the top of it's container.
    /// Currently, everything but foundation cards can have this be false</param>
    /// <returns></returns>
    public static bool CanMatch(CardScript card1, CardScript card2, bool checkIsTop = true)
    {
        // checks if the cards are at the top of their containers
        if (checkIsTop && (!IsAtContainerTop(card1) || !IsAtContainerTop(card2)))
            return false;

        if (card1.cardNum != card2.cardNum)
            return false;

        if ((card1.cardSuit.Equals("hearts") && card2.cardSuit.Equals("diamonds")) ||
            (card1.cardSuit.Equals("diamonds") && card2.cardSuit.Equals("hearts")) ||
            (card1.cardSuit.Equals("spades") && card2.cardSuit.Equals("clubs")) ||
            (card1.cardSuit.Equals("clubs") && card2.cardSuit.Equals("spades")))
            return true;

        //otherwise not a match 
        return false;
    }

    /// <summary>
    /// Checks if the card is at the top of its container's cardList. Currently only works on foundation cards.
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    private static bool IsAtContainerTop(CardScript card)
    {
        // hitboxes are disabled for all cards not on the top for the reactor, wastepile, and deck
        // since they can't be picked up, only foundation cards need to be checked

        if (card.container.CompareTag("Foundation") &&
            card.container.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>() != card)
        {
            return false;
        }

        /*
        if (card.container.CompareTag("Reactor") &&
            card.container.GetComponent<ReactorScript>().cardList[0].GetComponent<CardScript>() != card)
        {
            return false;
        }

        if (card.container.CompareTag("Wastepile") &&
            card.container.GetComponent<WastepileScript>().cardList[0].GetComponent<CardScript>() != card)
        {
            return false;
        }

        if (card.container.CompareTag("Deck") &&
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
    /// <param name="object1"></param>
    /// <param name="object2"></param>
    /// <returns></returns>
    public static bool IsSameSuit(GameObject object1, GameObject object2)
    {
        return (GetSuit(object1) == GetSuit(object2));
    }

    public static string GetSuit(GameObject suitObject)
    {
        if (suitObject.CompareTag("Card"))
            return suitObject.GetComponent<CardScript>().cardSuit;
        if (suitObject.CompareTag("Reactor"))
            return suitObject.GetComponent<ReactorScript>().suit;

        throw new System.ArgumentException("suitObject must have a suit variable");
    }
}
