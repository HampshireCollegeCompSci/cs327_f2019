using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UndoScript : MonoBehaviour
{
    public static UndoScript undoScript;
    public Stack<Move> moveLog;

    private void Start()
    {
        moveLog = Config.config.moveLog;
    }
    /*
     *logMove takes a number of  paramaters, detects if the card below the moved card was hidden, then logs the move.
     */
    public void logMove(string moveType, GameObject card, bool isAction = true, int actionsRemaining = 1, bool nextCardWasHidden = false)
    {
        GameObject origin = card.GetComponent<CardScript>().container; //get the cards original location
        
        //create the log of the move
        Move move = new Move() {
            moveType = moveType,
            card = card,
            origin = origin,
            nextCardWasHidden = nextCardWasHidden,
            isAction = isAction,
            remainingActions = actionsRemaining
        };
        moveLog.Push(move); //push the log to the undo stack
    }

    /*
     *undo is the function which reads from the moveLog and resets cards, score, moves, etc to their old state. 
     */
    public void undo()
    {
        if (moveLog.Count > 0) //only run if there's something in the stack
        {
            Move lastMove = null;
            if (!moveLog.Peek().isAction) //if the lastMove wasn't an action that means it was a stack of tokens moved at once
            {
                Debug.Log("stack undo");

                // list goes from bottom token to top in original stack
                List<Move> undoList = new List<Move>();
                undoList.Add(moveLog.Pop());

                int actionTracker = undoList[0].remainingActions;
                while (moveLog.Peek().remainingActions == actionTracker)
                {
                    undoList.Insert(0, moveLog.Pop());
                }

                GameObject newFoundation = undoList[0].origin;
                // cards are removed bottom to top and nextCardWasHidden expects that it's on the top (index 0)
                // therefore, the top of the stack is the only card that will know if the stack sat on a hidden card
                if (undoList[undoList.Count - 1].nextCardWasHidden)
                {
                    newFoundation.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().SetVisibility(false);
                }
                for (int i = 0; i < undoList.Count - 1; i++)
                {
                    undoList[i].card.GetComponent<CardScript>().MoveCard(newFoundation, doLog: false, removeUpdateHolo: false, addUpdateHolo: false);
                }
                undoList[undoList.Count - 1].card.GetComponent<CardScript>().MoveCard(newFoundation, doLog: false);
                Config.config.actions = undoList[0].remainingActions;
                return;

                Stack<Move> stackUndo = new Stack<Move>();
                stackUndo.Push(moveLog.Pop());
                while (true) //invert the stack
                {
                    stackUndo.Push(moveLog.Pop());
                    if (stackUndo.Peek().isAction)
                    {
                        break;
                    }
                }
                int cardsMoved = 0;
                while (stackUndo.Count != 0) //move them to the the correct foundation
                {
                    lastMove = stackUndo.Pop();
                    lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false);
                    cardsMoved++;
                }

                if (lastMove.nextCardWasHidden) //if the card under the stack was hidden, re-hide it
                {
                    lastMove.origin.GetComponent<FoundationScript>().cardList[cardsMoved].GetComponent<CardScript>().SetVisibility(false);
                    //lastMove.origin.GetComponent<FoundationScript>().SetCardPositions();
                }
                Config.config.actions = lastMove.remainingActions;
                return;
            }
            else if (moveLog.Peek().moveType == "move") //standard behavior, move a single token back where it was
            {
                lastMove = moveLog.Pop();
                if (lastMove.nextCardWasHidden)
                {
                    lastMove.origin.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().SetVisibility(false);
                }
                lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false);
                if (lastMove.isAction)
                {
                    Config.config.actions = lastMove.remainingActions;
                }
                return;
            }
            else if (moveLog.Peek().moveType == "match") //undo a match, removing the score gained and moving both cards back to their original locations
            {
                for (int i = 0; i < 2; i++)
                {
                    lastMove = moveLog.Pop();
                    if (lastMove.nextCardWasHidden)
                    {
                        lastMove.origin.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().SetVisibility(false);
                    }
                    lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false);

                }
                Config.config.score -= Config.config.matchPoints;
                return;
            }
            else if (moveLog.Peek().moveType == "draw") //move the last three drawn cards back to the deck (assuming the last action was to draw from the deck)
            {
                lastMove = moveLog.Pop();
                lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false, removeUpdateHolo: false);
                while (moveLog.Peek().remainingActions == lastMove.remainingActions)
                {
                    lastMove = moveLog.Pop();
                    lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false, removeUpdateHolo: false);
                }
                Config.config.wastePile.GetComponent<WastepileScript>().CheckHologram(false);
                Config.config.score = lastMove.remainingActions;
                return;
            }
            else if (moveLog.Peek().moveType == "cycle") //undo a cycle turning over, resets all tokens moved up, along with the move counter
            {
                while (moveLog.Peek().moveType == "cycle")
                {
                    lastMove = moveLog.Pop();
                    if (lastMove.nextCardWasHidden)
                    {
                        lastMove.origin.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().SetVisibility(false);
                    }
                    lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false);
                }
                Config.config.actions = lastMove.remainingActions;
                return;
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