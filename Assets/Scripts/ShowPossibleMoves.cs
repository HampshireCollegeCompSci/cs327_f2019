using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPossibleMoves : MonoBehaviour
{
    public static ShowPossibleMoves showPossibleMoves;

    public GameObject reactorMove;
    public List<GameObject> cardMoves;
    public List<GameObject> cardMatches;

    private void Start()
    {
        cardMoves = new List<GameObject>();
        cardMatches = new List<GameObject>();
    }

    private void Awake()
    {
        if (showPossibleMoves == null)
        {
            DontDestroyOnLoad(gameObject); //makes instance persist across scenes
            showPossibleMoves = this;
        }
        else if (showPossibleMoves != this)
        {
            Destroy(gameObject); //deletes copies of global which do not need to exist, so right version is used to get info from
        }
    }

    private void FindMoves(GameObject selectedCard)
    {
        int selectedCardNum = selectedCard.GetComponent<CardScript>().cardNum;

        bool cardIsFromFoundation = (selectedCard.GetComponent<CardScript>().container.GetComponent<FoundationScript>() != null);
        bool cardIsFromWastepile = (selectedCard.GetComponent<CardScript>().container.GetComponent<WastepileScript>() != null);
        bool cardCanBeMatched = true;
        // if the card is in a foundation and not at the top of it
        if (cardIsFromFoundation && (selectedCard != selectedCard.GetComponent<CardScript>().container.GetComponent<FoundationScript>().cardList[0]))
        {
            cardCanBeMatched = false;
        }

        if (cardCanBeMatched)
        {
            foreach (GameObject reactor in Config.config.reactors)
            {
                // if the card matches the card in the top of the reactor
                if (reactor.GetComponent<ReactorScript>().cardList.Count != 0 && 
                    UtilsScript.global.IsMatch(reactor.GetComponent<ReactorScript>().cardList[0], selectedCard))
                {
                    cardMatches.Add(reactor.GetComponent<ReactorScript>().cardList[0]);
                }
            }

            // if the card is not in the reactor
            if (!selectedCard.GetComponent<CardScript>().container.CompareTag("Reactor"))
            {
                // get the reactor that we can match into
                foreach (GameObject reactor in Config.config.reactors)
                {
                    if (reactor.GetComponent<ReactorScript>().suit == selectedCard.GetComponent<CardScript>().cardSuit)
                    {
                        reactorMove = reactor;
                        break;
                    }
                }
            }
        }
        
        foreach (GameObject foundation in Config.config.foundations)
        {
            if (foundation.GetComponent<FoundationScript>().cardList.Count != 0)
            {
                GameObject topFoundationCard = foundation.GetComponent<FoundationScript>().cardList[0];

                // if the card can match and matches with the foundation top
                if (cardCanBeMatched && UtilsScript.global.IsMatch(selectedCard, topFoundationCard))
                {
                    cardMatches.Add(foundation.GetComponent<FoundationScript>().cardList[0]);
                }
                // if the card is not from a reactor can it stack?
                else if ((cardIsFromFoundation || cardIsFromWastepile) &&
                    topFoundationCard.GetComponent<CardScript>().cardNum == (selectedCardNum + 1))
                {
                    cardMoves.Add(foundation.GetComponent<FoundationScript>().cardList[0]);
                }
            }
        }

        // if the card can match and matches with the wastepile top
        if (cardCanBeMatched && Config.config.wastePile.GetComponent<WastepileScript>().GetCardList().Count != 0 &&
            UtilsScript.global.IsMatch(Config.config.wastePile.GetComponent<WastepileScript>().cardList[0], selectedCard))
        {
            cardMatches.Add(Config.config.wastePile.GetComponent<WastepileScript>().cardList[0]);
        }
    }

    public void ShowMoves(GameObject selectedCard)
    {
        FindMoves(selectedCard);
        
        foreach (GameObject card in cardMoves)
        {
            card.GetComponent<CardScript>().GlowOn(false);
        }

        foreach (GameObject card in cardMatches)
        {
            card.GetComponent<CardScript>().GlowOn(true);
        }

        if (reactorMove != null)
        {
            ReactorScript reactorMoveScript = reactorMove.GetComponent<ReactorScript>();

            // if moving the card into the reactor will lose us the game
            if (reactorMoveScript.CountReactorCard() +
                selectedCard.GetComponent<CardScript>().cardVal >= Config.config.maxReactorVal)
            {
                reactorMoveScript.GlowOn(2);
            }
            else
            {
                reactorMoveScript.GlowOn(0);
            }

            // if the reactor that is glowing has cards in it then we need to
            // turn off their hitboxes because they are above the reactor's hitbox
            // this must be done for util's card dragging to properly change the
            // hologram glow when hovering over the reactor, the hit detection
            // would register for the cards over the reactor, not the reactor itself

            // if the reactor has cards in it
            int cardCount = reactorMoveScript.cardList.Count;
            if (cardCount != 0)
            {
                for (int i = 0; i < cardCount; i++)
                {
                    reactorMoveScript.cardList[i].GetComponent<BoxCollider2D>().enabled = false;
                }
            }
        }
    }

    public void HideMoves()
    {
        foreach (GameObject card in cardMoves)
        {
            card.GetComponent<CardScript>().GlowOff();
        }

        foreach (GameObject card in cardMatches)
        {
            card.GetComponent<CardScript>().GlowOff();
        }

        if (reactorMove != null)
        {
            ReactorScript reactorMoveScript = reactorMove.GetComponent<ReactorScript>();
            reactorMoveScript.GlowOff();

            // if the reactor has cards in it
            int cardCount = reactorMoveScript.cardList.Count;
            if (cardCount != 0)
            {
                for (int i = 0; i < cardCount; i++)
                {
                    reactorMoveScript.cardList[i].GetComponent<BoxCollider2D>().enabled = true;
                }
            }
        }

        reactorMove = null;
        cardMoves.Clear();
        cardMatches.Clear();
    }
}
