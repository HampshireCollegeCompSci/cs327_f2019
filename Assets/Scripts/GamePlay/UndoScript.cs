using System.Collections.Generic;
using UnityEngine;

public class UndoScript : MonoBehaviour
{
    public static UndoScript undoScript;
    public Stack<Move> moveLog;
    public UtilsScript utils;

    private void Awake()
    {
        if (undoScript == null)
        {
            DontDestroyOnLoad(gameObject); //makes instance persist across scenes
            undoScript = this;
        }
        else if (undoScript != this)
            //deletes copies of global which do not need to exist, so right version is used to get info from
            Destroy(gameObject);
    }

    private void Start()
    {
        //moveLog = Config.config.moveLog;
        moveLog = new Stack<Move>();
    }
    /*
     *logMove takes a number of  paramaters, detects if the card below the moved card was hidden, then logs the move.
     */
    public void logMove(string moveType, GameObject card, bool isAction = true, bool nextCardWasHidden = false)
    {
        GameObject origin = card.GetComponent<CardScript>().container; //get the cards original location

        //create the log of the move
        Move move = new Move()
        {
            moveType = moveType,
            card = card,
            origin = origin,
            nextCardWasHidden = nextCardWasHidden,
            isAction = isAction,
            remainingActions = Config.config.actions,
            score = Config.config.score,
            moveNum = Config.config.moveCounter,
        };
        moveLog.Push(move); //push the log to the undo stack
        return;
    }

    private void MoveFoundationCard(Move toMove)
    {
        if (toMove.nextCardWasHidden)
            toMove.origin.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().SetFoundationVisibility(false);

        toMove.card.GetComponent<CardScript>().MoveCard(toMove.origin, doLog: false);
    }

    /*
     *undo is the function which reads from the moveLog and resets cards, score, moves, etc to their old state. 
     */
    public void Undo()
    {
        if (utils.IsInputStopped())
            return;

        if (moveLog.Count != 0) //only run if there's something in the stack
        {
            //Config.config.moves++;
            Move lastMove;
            if (moveLog.Peek().moveType == "stack")
            {
                // list goes from bottom token to top in original stack
                List<Move> undoList = new List<Move>
                {
                    moveLog.Pop() // this is the top token of the stack
                };

                int moveNumber = undoList[0].moveNum;
                while (moveLog.Count != 0 && moveLog.Peek().moveNum == moveNumber)
                    undoList.Insert(0, moveLog.Pop());

                GameObject newFoundation = undoList[0].origin;

                // cards are removed bottom to top when moving stacks and
                // nextCardWasHidden expects that it's token was on the top (index 0) of the stack
                // therefore, the top of the stack is the only card that will know if the stack sat on a hidden card
                if (undoList[undoList.Count - 1].nextCardWasHidden)
                    newFoundation.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().SetFoundationVisibility(false);

                for (int i = 0; i < undoList.Count - 1; i++) // move the tokens back
                    undoList[i].card.GetComponent<CardScript>().MoveCard(newFoundation, doLog: false, showHolo: false);
                undoList[undoList.Count - 1].card.GetComponent<CardScript>().MoveCard(newFoundation, doLog: false);

                utils.UpdateActions(undoList[0].remainingActions, setAsValue: true);
                return;
            }
            else if (moveLog.Peek().moveType == "move") //standard behavior, move a single token back where it was
            {
                lastMove = moveLog.Pop();
                MoveFoundationCard(lastMove);

                if (lastMove.isAction)
                    utils.UpdateActions(lastMove.remainingActions, setAsValue: true);

                return;
            }
            else if (moveLog.Peek().moveType == "match") //undo a match, removing the score gained and moving both cards back to their original locations
            {
                MoveFoundationCard(moveLog.Pop());

                lastMove = moveLog.Pop();
                MoveFoundationCard(lastMove);

                utils.UpdateScore(lastMove.score, setAsValue: true);
                utils.UpdateActions(-1);
                return;
            }
            else if (moveLog.Peek().moveType == "draw") //move the last three drawn cards back to the deck (assuming the last action was to draw from the deck)
            {
                while (true)
                {
                    lastMove = moveLog.Pop();

                    if (moveLog.Count == 0 || moveLog.Peek().moveNum != lastMove.moveNum)
                    {
                        lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false);
                        utils.UpdateActions(lastMove.remainingActions, setAsValue: true);
                        return;
                    }

                    lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, doLog: false, showHolo: false);
                }
            }
            else if (moveLog.Peek().moveType == "cycle") //undo a cycle turning over, resets all tokens moved up, along with the move counter
            {
                lastMove = moveLog.Pop();
                MoveFoundationCard(lastMove);

                while (moveLog.Count != 0 && moveLog.Peek().moveNum == lastMove.moveNum)
                {
                    lastMove = moveLog.Pop();
                    MoveFoundationCard(lastMove);
                }

                utils.UpdateActions(lastMove.remainingActions, setAsValue: true);

                return;
            }
        }
    }
}