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
        moveLog = new Stack<Move>();
    }
    public void logMove(string moveType, GameObject card)
    {
        GameObject origin = card.GetComponent<CardScript>().container;
        bool nextCardWasHidden;
        if (card.GetComponent<CardScript>().container.GetComponent<FoundationScript>().cardList.Count == 1)
        {
            nextCardWasHidden = false;
        }
        else if (card.GetComponent<CardScript>().container.GetComponent<FoundationScript>().cardList[1].GetComponent<CardScript>().hidden)
        {
            nextCardWasHidden = true;
        }
        else
        {
            nextCardWasHidden = false;
        }
        Move move = new Move(moveType, card, origin, nextCardWasHidden);
        moveLog.Push(move);
        print("There are " + moveLog.Count + " moves logged.");
    }

    public void undo()
    {
        if (moveLog.Count > 0)
        {
            Move lastMove = moveLog.Pop();
            if (lastMove.moveType == "move")
            {
                if (lastMove.nextCardWasHidden)
                {
                    lastMove.origin.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().hidden = true;
                    lastMove.origin.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().SetCardAppearance();
                }
                lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin);
            }
            else if (lastMove.moveType == "match")
            {

            }
            else if (lastMove.moveType == "draw")
            {

            }
            else if (lastMove.moveType == "waste")
            {

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
