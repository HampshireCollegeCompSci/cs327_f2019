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
    private bool cardIsFromFoundation;
    private bool cardIsTopOfStack;
    private bool cardIsFromWastepile;

    private List<GameObject> cardMoves;
    private GameObject reactorMove;

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

    private List<GameObject> FindMoves(GameObject selectedCard)
    {
        cardIsFromWastepile = (selectedCard.GetComponent<CardScript>().container.GetComponent<WastepileScript>() != null);
        cardIsFromFoundation = (selectedCard.GetComponent<CardScript>().container.GetComponent<FoundationScript>() != null);
        if (cardIsFromFoundation)
        {
            cardIsTopOfStack = (selectedCard == selectedCard.GetComponent<CardScript>().container.GetComponent<FoundationScript>().cardList[0]);
        }
        else
        {
            cardIsTopOfStack = true;
        }

        List<GameObject> output = new List<GameObject>();
        foreach (GameObject foundation in foundationList)
        {
            if (foundation.GetComponent<FoundationScript>().cardList.Count > 0)
            {
                if ((UtilsScript.global.IsMatch(foundation.GetComponent<FoundationScript>().cardList[0], selectedCard) && cardIsTopOfStack) || (foundation.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().cardNum == (selectedCard.GetComponent<CardScript>().cardNum + 1) && (cardIsFromFoundation || cardIsFromWastepile)))
                {
                    output.Add(foundation.GetComponent<FoundationScript>().cardList[0]);
                }
            }
        }

        foreach (GameObject reactor in reactorList)
        {
            if (reactor.GetComponent<ReactorScript>().cardList.Count > 0)
            {
                if (UtilsScript.global.IsMatch(reactor.GetComponent<ReactorScript>().cardList[0], selectedCard) && cardIsTopOfStack)
                {
                    output.Add(reactor.GetComponent<ReactorScript>().cardList[0]);
                }
            }
        }

        if (wastepile.GetComponent<WastepileScript>().GetCardList().Count != 0 && cardIsTopOfStack && UtilsScript.global.IsMatch(wastepile.GetComponent<WastepileScript>().cardList[0], selectedCard))
        {
            output.Add(wastepile.GetComponent<WastepileScript>().cardList[0]);
        }

        return output;
    }

    private GameObject FindReactorMove(GameObject selectedCard)
    {
        Debug.Log("FRM");
        cardIsFromFoundation = (selectedCard.GetComponent<CardScript>().container.GetComponent<FoundationScript>() != null);
        if (cardIsFromFoundation)
        {
            cardIsTopOfStack = (selectedCard == selectedCard.GetComponent<CardScript>().container.GetComponent<FoundationScript>().cardList[0]);
        }
        else
        {
            cardIsTopOfStack = true;
        }

        if (cardIsTopOfStack)
        {
            foreach (GameObject reactor in reactorList)
            {
                if (reactor.GetComponent<ReactorScript>().suit == selectedCard.GetComponent<CardScript>().cardSuit)
                {
                    return reactor;
                }
            }
        }


        Debug.Log("null");
        return null;
    }

    public void ShowMoves(GameObject selectedCard)
    {
        cardMoves = FindMoves(selectedCard);
        if (cardMoves.Count > 0)
        {
            foreach (GameObject card in cardMoves)
            {
                card.GetComponent<CardScript>().GlowOn();
            }
        }

        reactorMove = FindReactorMove(selectedCard);
        if (reactorMove != null)
        {
            reactorMove.GetComponent<ReactorScript>().GlowOn();
        }
    }

    public void HideMoves()
    {
        foreach (GameObject card in cardMoves)
        {
            card.GetComponent<CardScript>().GlowOff();
        }

        if (reactorMove != null)
        {
            reactorMove.GetComponent<ReactorScript>().GlowOff();
        }

        return;

        foreach (GameObject foundation in foundationList)
        {
            foreach (GameObject card in foundation.GetComponent<FoundationScript>().cardList)
            {
                if (card.GetComponent<CardScript>().isHidden() || card.GetComponent<CardScript>().GlowOff())
                {
                    break;
                }
            }
        }

        foreach (GameObject reactor in reactorList)
        {
            if (reactor.GetComponent<ReactorScript>().cardList.Count != 0)
            {
                reactor.GetComponent<ReactorScript>().cardList[0].GetComponent<CardScript>().GlowOff();
            }
            reactor.GetComponent<ReactorScript>().GlowOff();
        }

        if (wastepile.GetComponent<WastepileScript>().GetCardList().Count != 0)
        {
            wastepile.GetComponent<WastepileScript>().cardList[0].GetComponent<CardScript>().GlowOff();
        }

        foreach (GameObject card in matchedpile.GetComponent<MatchedPileScript>().cardList)
        {
            card.GetComponent<CardScript>().GlowOff();
        }
    }
}
