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
    public void logMove(string moveType, GameObject card, bool isAction = true, int actionsRemaining = 1)
    {
        GameObject origin = card.GetComponent<CardScript>().container; //get the cards original location
        bool nextCardWasHidden = false; //default nextCardWasHidden to false
        if (origin.TryGetComponent(typeof(FoundationScript), out Component component)) //see what the origin was and check the appropriate location for hidden card.
        {
            if (card.GetComponent<CardScript>().container.GetComponent<FoundationScript>().cardList.Count == 1)
            {
                nextCardWasHidden = false;
            }
            else if (card.GetComponent<CardScript>().container.GetComponent<FoundationScript>().cardList[1].GetComponent<CardScript>().isHidden())
            {
                nextCardWasHidden = true;
            }
        }
        //else if (origin.TryGetComponent(typeof(ReactorScript), out component))
        //{
        //    if (card.GetComponent<CardScript>().container.GetComponent<ReactorScript>().cardList.Count == 1)
        //    {
        //        nextCardWasHidden = false;
        //    }
        //    else if (card.GetComponent<CardScript>().container.GetComponent<ReactorScript>().cardList[1].GetComponent<CardScript>().isHidden())
        //    {
        //        nextCardWasHidden = true;
        //    }
        //}
        //else if (origin.TryGetComponent(typeof(WastepileScript), out component))
        //{
        //    if (card.GetComponent<CardScript>().container.GetComponent<WastepileScript>().cardList.Count == 1)
        //    {
        //        nextCardWasHidden = false;
        //    }
        //    else if (card.GetComponent<CardScript>().container.GetComponent<WastepileScript>().cardList[1].GetComponent<CardScript>().isHidden())
        //    {
        //        nextCardWasHidden = true;
        //    }
        //}
        else
        {
            nextCardWasHidden = false;
        }

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
                Stack<Move> stackUndo = new Stack<Move>();
                stackUndo.Push(moveLog.Pop());
                while (true) //invert the stack
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
                for (int i = 0; i < Config.config.cardsToDeal; i++)
                {
                    lastMove = moveLog.Pop();
                    lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false, removeUpdateHolo: false);
                }
                Config.config.wastePile.GetComponent<WastepileScript>().CheckHologram(false);
                if (lastMove.isAction)
                {
                    Config.config.actions = lastMove.remainingActions;
                }
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