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
        else if (origin.TryGetComponent(typeof(DeckScript), out component))
        {
            //Special Case
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
                lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, false);
            }
            else if (lastMove.moveType == "match")
            {
                if (lastMove.nextCardWasHidden)
                {
                    lastMove.origin.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().hidden = true;
                    lastMove.origin.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().SetCardAppearance();
                }
                lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, false);
                lastMove = moveLog.Pop();
                if (lastMove.nextCardWasHidden)
                {
                    lastMove.origin.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().hidden = true;
                    lastMove.origin.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().SetCardAppearance();
                }
                lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, false);
                Config.config.score -= Config.config.matchPoints;
                Debug.Log("score" + Config.config.score);
            }
            else if (lastMove.moveType == "draw")
            {
                lastMove.card.GetComponent<CardScript>().hidden = true;
                lastMove.card.GetComponent<CardScript>().SetCardAppearance();
                lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, false);
                for (int i = 0; i < 2; i++)
                {
                    lastMove = moveLog.Pop();
                    lastMove.card.GetComponent<CardScript>().hidden = true;
                    lastMove.card.GetComponent<CardScript>().SetCardAppearance();
                    lastMove.card.GetComponent<CardScript>().MoveCard(lastMove.origin, false);
                }
                
            }
            Config.config.actions -= 1;
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
