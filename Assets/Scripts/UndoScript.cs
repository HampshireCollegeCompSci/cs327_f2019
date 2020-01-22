using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UndoScript : MonoBehaviour
{
    public static UndoScript undoScript;
    public Stack<Move> moveLog;
    public UtilsScript utils;

    private void Start()
    {
        //moveLog = Config.config.moveLog;
        moveLog = new Stack<Move>();
    }
    /*
     *logMove takes a number of  paramaters, detects if the card below the moved card was hidden, then logs the move.
     */
    public void logMove(string moveType, GameObject card, bool isAction = true, int actionsRemaining = 1, bool nextCardWasHidden = false)
    {
        if (utils == null)
        {
            utils = UtilsScript.global;
        }

        GameObject origin = card.GetComponent<CardScript>().container; //get the cards original location

        //create the log of the move
        Move move = new Move()
        {
            moveType = moveType,
            card = card,
            origin = origin,
            nextCardWasHidden = nextCardWasHidden,
            isAction = isAction,
            remainingActions = actionsRemaining
        };
        moveLog.Push(move); //push the log to the undo stack
        return;
    }

    /*
     *undo is the function which reads from the moveLog and resets cards, score, moves, etc to their old state. 
     */
    public void undo()
    {
        if (moveLog.Count > 0) //only run if there's something in the stack
        {
            if (utils.IsInputStopped())
            {
                return;
            }

            Move lastMove = null;
            if (moveLog.Peek().moveType == "stack")
            {
                // list goes from bottom token to top in original stack
                List<Move> undoList = new List<Move>
                {
                    moveLog.Pop() // this is the top token of the stack
                };

                int actionTracker = undoList[0].remainingActions;
                while (moveLog.Count != 0 && moveLog.Peek().remainingActions == actionTracker && moveLog.Peek().moveType == "stack")
                {
                    undoList.Insert(0, moveLog.Pop());
                }

                GameObject newFoundation = undoList[0].origin;

                // cards are removed bottom to top when moving stacks and
                // nextCardWasHidden expects that it's token was on the top (index 0) of the stack
                // therefore, the top of the stack is the only card that will know if the stack sat on a hidden card
                if (undoList[undoList.Count - 1].nextCardWasHidden)
                {
                    newFoundation.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().SetVisibility(false);
                }

                for (int i = 0; i < undoList.Count - 1; i++) // move the tokens back without updating holograms yet
                {
                    undoList[i].card.GetComponent<CardScript>().MoveCard(newFoundation, doLog: false, removeUpdateHolo: false, addUpdateHolo: false);
                }
                // for the final top token in this stack, update the foundation's hologram (hide the previous one and show the new top one)
                undoList[undoList.Count - 1].card.GetComponent<CardScript>().MoveCard(newFoundation, doLog: false);
                
                utils.UpdateActionCounter(undoList[0].remainingActions, setAsValue: true);
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
                    utils.UpdateActionCounter(lastMove.remainingActions, true);
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

                utils.UpdateScore(-Config.config.matchPoints);
                return;
            }
            else if (moveLog.Peek().moveType == "draw") //move the last three drawn cards back to the deck (assuming the last action was to draw from the deck)
            {
                lastMove = moveLog.Pop();
                lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false, removeUpdateHolo: false);
                while (moveLog.Count != 0 && moveLog.Peek().moveType == "draw" && moveLog.Peek().remainingActions == lastMove.remainingActions)
                {
                    lastMove = moveLog.Pop();
                    lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false, removeUpdateHolo: false);
                }
                Config.config.wastePile.GetComponent<WastepileScript>().CheckHologram(false);
                utils.UpdateActionCounter(lastMove.remainingActions, true);
                return;
            }
            else if (moveLog.Peek().moveType == "deckreset") //move the entire deck back into the wastepile
            {
                lastMove = moveLog.Pop();
                lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false, removeUpdateHolo: false, addUpdateHolo: false);
                while (moveLog.Count != 0 && moveLog.Peek().moveType == "deckreset" && moveLog.Peek().remainingActions == lastMove.remainingActions)
                {
                    lastMove = moveLog.Pop();
                    lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false, removeUpdateHolo: false, addUpdateHolo: false);
                }
                Config.config.wastePile.GetComponent<WastepileScript>().CheckHologram(false);
                utils.UpdateActionCounter(lastMove.remainingActions, true);
                return;
            }
            else if (moveLog.Peek().moveType == "cycle") //undo a cycle turning over, resets all tokens moved up, along with the move counter
            {
                while (moveLog.Count != 0 && moveLog.Peek().moveType == "cycle")
                {
                    lastMove = moveLog.Pop();
                    if (lastMove.nextCardWasHidden)
                    {
                        lastMove.origin.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().SetVisibility(false);
                    }
                    lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false);
                }

                if (lastMove.remainingActions == Config.config.actionMax)
                {
                    undo();
                }
                else
                {
                    utils.UpdateActionCounter(lastMove.remainingActions, true);
                }

                return;
            }
            StateLoader.saveSystem.writeState();
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