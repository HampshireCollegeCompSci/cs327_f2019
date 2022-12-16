﻿using System.Collections.Generic;
using UnityEngine;

public class UndoScript : MonoBehaviour
{
    // Singleton instance.
    public static UndoScript Instance;

    private Stack<Move> moveLog;

    // Initialize the singleton instance.
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            throw new System.Exception("two of these scripts should not exist at the same time");
        }
    }

    private void Start()
    {
        moveLog = new();
    }

    public void SetMoveLog(Stack<Move> newMoves)
    {
        moveLog = newMoves;
    }

    public void ClearMoveLog()
    {
        moveLog.Clear();
        StateLoader.Instance.ClearSaveMoveLog();
    }

    /*
     *logMove takes a number of  paramaters, detects if the card below the moved card was hidden, then logs the move.
     */
    public void LogMove(GameObject card, byte moveType, bool isAction = true, bool nextCardWasHidden = false)
    {
        //create the log of the move
        Move move = new()
        {
            card = card,
            origin = card.GetComponent<CardScript>().Container,
            moveType = moveType,
            nextCardWasHidden = nextCardWasHidden,
            isAction = isAction,
            remainingActions = Config.Instance.actions,
            score = Config.Instance.score,
            moveNum = Config.Instance.moveCounter,
        };
        moveLog.Push(move); //push the log to the undo stack
        StateLoader.Instance.AddMove(move);
        return;
    }

    /*
     *undo is the function which reads from the moveLog and resets cards, score, moves, etc to their old state. 
     */
    public void Undo()
    {
        if (moveLog.Count != 0) //only run if there's something in the stack
        {
            Debug.Log("undoing move");
            Move lastMove;
            switch (moveLog.Peek().moveType)
            {
                case Constants.LogMoveTypes.stack:
                    // the undoList is ordered such that [0] is the top of the stack
                    Move topCardMove = moveLog.Pop();

                    List<Move> undoList = new(13) { topCardMove };
                    StateLoader.Instance.RemoveMove();

                    int moveNumber = topCardMove.moveNum;
                    while (moveLog.Count != 0 && moveLog.Peek().moveNum == moveNumber)
                    {
                        undoList.Add(moveLog.Pop());
                        StateLoader.Instance.RemoveMove();
                    }

                    GameObject newFoundation = topCardMove.origin;
                    if (topCardMove.nextCardWasHidden)
                    {
                        newFoundation.GetComponent<FoundationScript>().CardList[^1].GetComponent<CardScript>().Hidden = true;
                    }

                    // move the tokens back
                    for (int i = undoList.Count - 1; i > 0; i--)
                    {
                        undoList[i].card.GetComponent<CardScript>().MoveCard(newFoundation, doLog: false, showHolo: false);
                    }
                    topCardMove.card.GetComponent<CardScript>().MoveCard(newFoundation, doLog: false);

                    UtilsScript.Instance.UpdateActions(topCardMove.remainingActions, setAsValue: true);
                    break;
                case Constants.LogMoveTypes.move:
                    // standard behavior, move a single token back where it was
                    lastMove = moveLog.Pop();
                    StateLoader.Instance.RemoveMove();
                    MoveFoundationCard(lastMove);

                    if (lastMove.isAction)
                    {
                        UtilsScript.Instance.UpdateActions(lastMove.remainingActions, setAsValue: true);
                    }
                    break;
                case Constants.LogMoveTypes.match:
                    // undo a match, removing the score gained and moving both cards back to their original locations
                    MoveFoundationCard(moveLog.Pop());
                    StateLoader.Instance.RemoveMove();

                    lastMove = moveLog.Pop();
                    StateLoader.Instance.RemoveMove();
                    MoveFoundationCard(lastMove);

                    ScoreScript.Instance.SetScore(lastMove.score);
                    UtilsScript.Instance.UpdateActions(-1);
                    break;
                case Constants.LogMoveTypes.draw:
                    // move the drawn cards back to the deck (assuming the last action was to draw from the deck)
                    while (true)
                    {
                        lastMove = moveLog.Pop();
                        StateLoader.Instance.RemoveMove();

                        if (moveLog.Count == 0 || moveLog.Peek().moveNum != lastMove.moveNum)
                        {
                            lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false);
                            UtilsScript.Instance.UpdateActions(lastMove.remainingActions, setAsValue: true);
                            break;
                        }

                        lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false, showHolo: false);
                    }
                    break;
                case Constants.LogMoveTypes.cycle:
                    // undo a cycle turning over, resets all tokens moved up, along with the move counter
                    lastMove = moveLog.Pop();
                    StateLoader.Instance.RemoveMove();
                    MoveFoundationCard(lastMove);

                    while (moveLog.Count != 0 && moveLog.Peek().moveNum == lastMove.moveNum)
                    {
                        // undo the cycle moves until the move that triggered it is reached.
                        // then trigger undo again so that the moveset is properly undone.
                        // note that a undo of a manual trigger of a cycle will not cause undo to be called again.
                        if (moveLog.Peek().moveType.Equals(Constants.LogMoveTypes.cycle))
                        {
                            lastMove = moveLog.Pop();
                            StateLoader.Instance.RemoveMove();
                            MoveFoundationCard(lastMove);
                        }
                        else
                        {
                            Undo();
                            return;
                        }
                    }
                    UtilsScript.Instance.UpdateActions(lastMove.remainingActions, setAsValue: true);
                    break;
                default:
                    throw new System.Exception("invalid move log move type");
            }
        }
    }

    private void MoveFoundationCard(Move toMove)
    {
        if (toMove.nextCardWasHidden)
        {
            toMove.origin.GetComponent<FoundationScript>().CardList[^1].GetComponent<CardScript>().Hidden = true;
        }

        toMove.card.GetComponent<CardScript>().MoveCard(toMove.origin, doLog: false);
    }
}
