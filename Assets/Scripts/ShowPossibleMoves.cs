﻿using System.Collections;
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
        CardScript selectedCardScript = selectedCard.GetComponent<CardScript>();

        int selectedCardNum = selectedCardScript.cardNum;

        bool cardIsFromFoundation = selectedCardScript.container.CompareTag("Foundation");
        bool cardIsFromWastepile = selectedCardScript.container.CompareTag("Wastepile");
        
        bool cardCanBeMatched = true;
        // if the card is in a foundation and not at the top of it
        if (cardIsFromFoundation && selectedCard != selectedCardScript.container.GetComponent<FoundationScript>().cardList[0])
            cardCanBeMatched = false;

        if (cardCanBeMatched)
        {
            foreach (GameObject reactor in Config.config.reactors)
            {
                // if the card matches the card in the top of the reactor
                if (reactor.GetComponent<ReactorScript>().cardList.Count != 0 && 
                    UtilsScript.global.CanMatch(reactor.GetComponent<ReactorScript>().cardList[0].GetComponent<CardScript>(),
                                                selectedCardScript, checkIsTop: false))
                    cardMatches.Add(reactor.GetComponent<ReactorScript>().cardList[0]);
            }

            // if the card is not in the reactor
            if (!selectedCard.GetComponent<CardScript>().container.CompareTag("Reactor"))
                // get the reactor that we can match into
                foreach (GameObject reactor in Config.config.reactors)
                    if (UtilsScript.global.IsSameSuit(selectedCard, reactor))
                    {
                        reactorMove = reactor;
                        break;
                    }
        }
        
        foreach (GameObject foundation in Config.config.foundations)
            if (foundation.GetComponent<FoundationScript>().cardList.Count != 0)
            {
                CardScript topFoundationCardScript = foundation.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>();

                // if the card can match and matches with the foundation top
                if (cardCanBeMatched && UtilsScript.global.CanMatch(selectedCardScript, topFoundationCardScript, checkIsTop: false))
                    cardMatches.Add(foundation.GetComponent<FoundationScript>().cardList[0]);
                // if the card is not from a reactor can it stack?
                else if ((cardIsFromFoundation || cardIsFromWastepile) &&
                    topFoundationCardScript.cardNum == selectedCardNum + 1)
                    cardMoves.Add(foundation.GetComponent<FoundationScript>().cardList[0]);
            }

        // if the card can match and matches with the wastepile top
        if (cardCanBeMatched && Config.config.wastePile.GetComponent<WastepileScript>().cardList.Count != 0)
        {
            GameObject topWastepileCard = Config.config.wastePile.GetComponent<WastepileScript>().cardList[0];
            if (UtilsScript.global.CanMatch(topWastepileCard.GetComponent<CardScript>(), selectedCardScript, checkIsTop: false))
                cardMatches.Add(topWastepileCard);
        }
    }

    public void ShowMoves(GameObject selectedCard)
    {
        FindMoves(selectedCard);
        
        foreach (GameObject card in cardMoves)
            card.GetComponent<CardScript>().GlowOn(false);

        foreach (GameObject card in cardMatches)
            card.GetComponent<CardScript>().GlowOn(true);

        if (reactorMove != null)
        {
            ReactorScript reactorMoveScript = reactorMove.GetComponent<ReactorScript>();

            if (reactorMoveScript.cardList.Count != 0)
                reactorMoveScript.cardList[0].GetComponent<BoxCollider2D>().enabled = false;

            // if moving the card into the reactor will lose us the game
            if (reactorMoveScript.CountReactorCard() +
                selectedCard.GetComponent<CardScript>().cardVal >= Config.config.maxReactorVal)
                reactorMoveScript.GlowOn(2);
            else
                reactorMoveScript.GlowOn(1);
        }
    }

    public void HideMoves()
    {
        foreach (GameObject card in cardMoves)
            card.GetComponent<CardScript>().GlowOff();

        foreach (GameObject card in cardMatches)
            card.GetComponent<CardScript>().GlowOff();

        if (reactorMove != null)
        {
            ReactorScript reactorMoveScript = reactorMove.GetComponent<ReactorScript>();

            if (reactorMoveScript.cardList.Count != 0)
                reactorMoveScript.cardList[0].GetComponent<BoxCollider2D>().enabled = true;

            reactorMoveScript.GlowOff();
        }

        reactorMove = null;
        cardMoves.Clear();
        cardMatches.Clear();
    }
}
