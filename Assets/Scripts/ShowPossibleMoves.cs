using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPossibleMoves : MonoBehaviour
{
    public static ShowPossibleMoves showPossibleMoves;

    public GameObject foundation1;
    public GameObject foundation2;
    public GameObject foundation3;
    public GameObject foundation4;

    public GameObject reactor1;
    public GameObject reactor2;
    public GameObject reactor3;
    public GameObject reactor4;

    public GameObject wastepile;

    public GameObject matchedpile;

    private GameObject[] foundationList;
    private GameObject[] reactorList;

    private GameObject reactorMove;
    private List<GameObject> cardMoves;
    private List<GameObject> cardMatches;

    private bool reactorAlerted;

    private void Start()
    {
        cardMoves = new List<GameObject>();
        cardMatches = new List<GameObject>();
    }

    public void SetCards()
    {
        foundation1 = GameObject.Find("Foundation (0)");
        foundation2 = GameObject.Find("Foundation (1)");
        foundation3 = GameObject.Find("Foundation (2)");
        foundation4 = GameObject.Find("Foundation (3)");
        foundationList = new GameObject[] { foundation1, foundation2, foundation3, foundation4 };

        reactor1 = GameObject.Find("ReactorPile (0)");
        reactor2 = GameObject.Find("ReactorPile (1)");
        reactor3 = GameObject.Find("ReactorPile (2)");
        reactor4 = GameObject.Find("ReactorPile (3)");
        reactorList = new GameObject[] {reactor1, reactor2, reactor3, reactor4 };

        wastepile = GameObject.Find("Scroll View");

        matchedpile = GameObject.Find("MatchedPile");
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
            foreach (GameObject reactor in reactorList)
            {
                // if the card can go into the reactor
                if (reactor.GetComponent<ReactorScript>().suit == selectedCard.GetComponent<CardScript>().cardSuit)
                {
                    // if we already know that we can move here and that we will lose the game if we do
                    if (!reactor.GetComponent<ReactorScript>().isAlertOn())
                    {
                        reactorMove = reactor;
                    }
                }
                // if the card matches the card in the top of the reactor
                else if (reactor.GetComponent<ReactorScript>().cardList.Count != 0 && 
                    UtilsScript.global.IsMatch(reactor.GetComponent<ReactorScript>().cardList[0], selectedCard))
                {
                    cardMatches.Add(reactor.GetComponent<ReactorScript>().cardList[0]);
                }
            }
        }
        
        foreach (GameObject foundation in foundationList)
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
        if (cardCanBeMatched && wastepile.GetComponent<WastepileScript>().GetCardList().Count != 0 &&
            UtilsScript.global.IsMatch(wastepile.GetComponent<WastepileScript>().cardList[0], selectedCard))
        {
            cardMatches.Add(wastepile.GetComponent<WastepileScript>().cardList[0]);
        }
    }

    public byte ShowMoves(GameObject selectedCard)
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
            // if moving the card into the reactor will lose us the game
            if (reactorMove.GetComponent<ReactorScript>().CountReactorCard() +
                selectedCard.GetComponent<CardScript>().cardVal >= Config.config.maxReactorVal)
            {
                reactorMove.GetComponent<ReactorScript>().AlertOn();
                // so we know that show moves was the one who turned on the alert
                reactorAlerted = true;
            }
            else
            {
                reactorMove.GetComponent<ReactorScript>().GlowOn();
            }
        }

        if (cardMatches.Count != 0)
        {
            return 2;
        }
        if (cardMoves.Count != 0)
        {
            return 1;
        }
        return 0;
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
            if (reactorAlerted)
            {
                reactorMove.GetComponent<ReactorScript>().AlertOff();
                reactorAlerted = false;
            }
            else
            {
                reactorMove.GetComponent<ReactorScript>().GlowOff();
            }
        }

        reactorMove = null;
        cardMoves.Clear();
        cardMatches.Clear();
    }
}
