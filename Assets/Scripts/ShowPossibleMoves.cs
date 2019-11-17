using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPossibleMoves : MonoBehaviour
{
    public static ShowPossibleMoves showPossibleMoves;

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

    private List<GameObject> FindMoves()
    {
        return new List<GameObject>();
    }

    public void ShowMoves()
    {
        foreach (GameObject card in FindMoves())
        {
            card.GetComponent<CardScript>().GlowOn();
        }
    }

    public void HideMoves()
    {
        foreach (GameObject card in FindMoves())
        {
            card.GetComponent<CardScript>().GlowOff();
        }
    }
}
