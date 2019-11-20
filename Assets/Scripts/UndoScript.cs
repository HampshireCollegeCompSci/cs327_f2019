﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UndoScript : MonoBehaviour
{
    public static UndoScript undoScript;
    public Stack<Move> moveLog;

    private void Start()
    {
        moveLog = new Stack<Move>();
    }
    public void logMove(string moveType, GameObject card, bool isAction = true, int actionsRemaining = 1)
    {
        print("Is action: " + isAction);
        GameObject origin = card.GetComponent<CardScript>().container;
        bool nextCardWasHidden = false;
        if (origin.TryGetComponent(typeof(FoundationScript), out Component component))
        {
            if (card.GetComponent<CardScript>().container.GetComponent<FoundationScript>().cardList.Count == 1)
            {
                nextCardWasHidden = false;
            }
            else if (card.GetComponent<CardScript>().container.GetComponent<FoundationScript>().cardList[1].GetComponent<CardScript>().hidden)
            {
                nextCardWasHidden = true;
            }
        }
        else if (origin.TryGetComponent(typeof(ReactorScript), out component))
        {
            if (card.GetComponent<CardScript>().container.GetComponent<ReactorScript>().cardList.Count == 1)
            {
                nextCardWasHidden = false;
            }
            else if (card.GetComponent<CardScript>().container.GetComponent<ReactorScript>().cardList[1].GetComponent<CardScript>().hidden)
            {
                nextCardWasHidden = true;
            }
        }
        else if (origin.TryGetComponent(typeof(WastepileScript), out component))
        {
            if (card.GetComponent<CardScript>().container.GetComponent<WastepileScript>().cardList.Count == 1)
            {
                nextCardWasHidden = false;
            }
            else if (card.GetComponent<CardScript>().container.GetComponent<WastepileScript>().cardList[1].GetComponent<CardScript>().hidden)
            {
                nextCardWasHidden = true;
            }
        }

        print("next card was hidden: " + nextCardWasHidden);

        Move move = new Move(moveType, card, origin, nextCardWasHidden, isAction, actionsRemaining);
        moveLog.Push(move);
        print("There are " + moveLog.Count + " moves logged.");
    }

    public void undo()
    {
        if (moveLog.Count > 0)
        {
            Move lastMove = null;
            if (!moveLog.Peek().isAction)
            {
                Stack<Move> stackUndo = new Stack<Move>();
                stackUndo.Push(moveLog.Pop());
                while (true)
                {
                    if (stackUndo.Peek().isAction)
                    {
                        break;
                    }
                    else
                    {
                        stackUndo.Push(moveLog.Pop());
                    }
                }
                int cardsMoved = 0;
                while (stackUndo.Count != 0)
                {
                    lastMove = stackUndo.Pop();
                    lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false);
                    cardsMoved++;
                }

                if (lastMove.nextCardWasHidden)
                {
                    lastMove.origin.GetComponent<FoundationScript>().cardList[cardsMoved].GetComponent<CardScript>().hidden = true;
                    lastMove.origin.GetComponent<FoundationScript>().cardList[cardsMoved].GetComponent<CardScript>().SetCardAppearance();
                }
                Config.config.actions = lastMove.remainingActions;
                return;
            }
            else if (moveLog.Peek().moveType == "move")
            {
                lastMove = moveLog.Pop();
                if (lastMove.nextCardWasHidden)
                {
                    lastMove.origin.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().hidden = true;
                    lastMove.origin.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().SetCardAppearance();
                }
                lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false);
                if (lastMove.isAction)
                {
                    Config.config.actions = lastMove.remainingActions;
                }
                return;
            }
            else if (moveLog.Peek().moveType == "match")
            {
                for (int i = 0; i < 2; i++)
                {
                    lastMove = moveLog.Pop();
                    if (lastMove.nextCardWasHidden)
                    {
                        lastMove.origin.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().hidden = true;
                        lastMove.origin.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().SetCardAppearance();
                    }
                    lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false);
                    
                }
                Config.config.score -= Config.config.matchPoints;
                return;
            }
            else if (moveLog.Peek().moveType == "draw")
            {
                for (int i = 0; i < Config.config.cardsToDeal; i++)
                {
                    lastMove = moveLog.Pop();
                    lastMove.card.GetComponent<CardScript>().hidden = true;
                    lastMove.card.GetComponent<CardScript>().SetCardAppearance();
                    lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false);
                    if (lastMove.isAction)
                    {
                        Config.config.actions = lastMove.remainingActions;
                    }
                }
                return;
                
            }
            else if (moveLog.Peek().moveType == "cycle")
            {
                while (moveLog.Peek().moveType == "cycle")
                {
                    lastMove = moveLog.Pop();
                    if (lastMove.nextCardWasHidden)
                    {
                        lastMove.origin.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().hidden = true;
                        lastMove.origin.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().SetCardAppearance();
                    }
                    lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false);
                }
                Config.config.actions = lastMove.remainingActions;
            }
        }
    }

    private void Awake()
    {
        if (undoScript == null)
        {
            DontDestroyOnLoad(gameObject); //makes instance persist across scenes
            undoScript = this;
        }
        else if (undoScript != this)
        {
            Destroy(gameObject); //deletes copies of global which do not need to exist, so right version is used to get info from
        }
    }
}
