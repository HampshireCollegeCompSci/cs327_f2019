using System.Collections.Generic;
using UnityEngine;

public class UndoScript : MonoBehaviour
{
    private Stack<Move> moveLog;

    // Singleton instance.
    public static UndoScript Instance;

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
            origin = card.GetComponent<CardScript>().container,
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
            Move lastMove;
            switch (moveLog.Peek().moveType)
            {
                case Constants.stackLogMove:
                    // list goes from bottom token to top in original stack
                    List<Move> undoList = new()
                    {
                        moveLog.Pop() // this is the top token of the stack
                    };
                    StateLoader.Instance.RemoveMove();

                    int moveNumber = undoList[0].moveNum;
                    while (moveLog.Count != 0 && moveLog.Peek().moveNum == moveNumber)
                    {
                        undoList.Insert(0, moveLog.Pop());
                        StateLoader.Instance.RemoveMove();
                    }

                    GameObject newFoundation = undoList[0].origin;

                    // cards are removed bottom to top when moving stacks and
                    // nextCardWasHidden expects that it's token was on the top (index 0) of the stack
                    // therefore, the top of the stack is the only card that will know if the stack sat on a hidden card
                    if (undoList[^1].nextCardWasHidden)
                    {
                        newFoundation.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().Hidden = true;
                    }

                    // move the tokens back
                    for (int i = 0; i < undoList.Count - 1; i++)
                    {
                        undoList[i].card.GetComponent<CardScript>().MoveCard(newFoundation, doLog: false, showHolo: false);
                    }
                    undoList[^1].card.GetComponent<CardScript>().MoveCard(newFoundation, doLog: false);

                    UtilsScript.Instance.UpdateActions(undoList[0].remainingActions, setAsValue: true);
                    break;
                case Constants.moveLogMove:
                    // standard behavior, move a single token back where it was
                    lastMove = moveLog.Pop();
                    StateLoader.Instance.RemoveMove();
                    MoveFoundationCard(lastMove);

                    if (lastMove.isAction)
                    {
                        UtilsScript.Instance.UpdateActions(lastMove.remainingActions, setAsValue: true);
                    }
                    break;
                case Constants.matchLogMove:
                    // undo a match, removing the score gained and moving both cards back to their original locations
                    MoveFoundationCard(moveLog.Pop());
                    StateLoader.Instance.RemoveMove();

                    lastMove = moveLog.Pop();
                    StateLoader.Instance.RemoveMove();
                    MoveFoundationCard(lastMove);

                    UtilsScript.Instance.UpdateScore(lastMove.score, setAsValue: true);
                    UtilsScript.Instance.UpdateActions(-1);
                    break;
                case Constants.drawLogMove:
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
                case Constants.cycleLogMove:
                    // undo a cycle turning over, resets all tokens moved up, along with the move counter
                    lastMove = moveLog.Pop();
                    StateLoader.Instance.RemoveMove();
                    MoveFoundationCard(lastMove);

                    while (moveLog.Count != 0 && moveLog.Peek().moveNum == lastMove.moveNum)
                    {
                        // undo the cycle moves until the move that triggered it is reached.
                        // then trigger undo again so that the moveset is properly undone.
                        // note that a undo of a manual trigger of a cycle will not cause undo to be called again.
                        if (moveLog.Peek().moveType.Equals(Constants.cycleLogMove))
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
            toMove.origin.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().Hidden = true;
        }

        toMove.card.GetComponent<CardScript>().MoveCard(toMove.origin, doLog: false);
    }
}
