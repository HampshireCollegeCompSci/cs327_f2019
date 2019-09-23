using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilsScript : MonoBehaviour
{
    public static UtilsScript global; //Creates a new instance if one does not yet exist
    public List<GameObject> selectedCards;
    public GameObject clickedCard;
    public GameObject matchedPile;

    void Awake()
    {
        if (global == null)
        {
            DontDestroyOnLoad(gameObject); //makes instance persist across scenes
            global = this;
        }
        else if (global != this)
        {
            Destroy(gameObject); //deletes copies of global which do not need to exist, so right version is used to get info from
        }
    }


    //sends out a raycast to see you selected something
    public void Click()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)), Vector2.zero);

        if (hit.collider.GetComponent<CardScript>().container.CompareTag("Foundation"))
        {
            hit.collider.GetComponent<CardScript>().container.GetComponent<FoundationScript>().Clicked();
        }

        else if (hit.collider.GetComponent<CardScript>().container.CompareTag("Reactor"))
        {
            hit.collider.GetComponent<CardScript>().container.GetComponent<ReactorScript>().Clicked();
        }

        else if (hit.collider.GetComponent<CardScript>().container.CompareTag("Wastepile"))
        {
            hit.collider.GetComponent<CardScript>().container.GetComponent<WastepileScript>().Clicked();
        }

        else if (hit.collider.GetComponent<CardScript>().container.CompareTag("Deck"))
        {
            hit.collider.GetComponent<CardScript>().container.GetComponent<DeckScript>().Clicked();
        }
    }

    public void Match()
    {
        //selectedCards[0].GetComponent<CardScript>().MoveCard(matchedPile, matchedPile.GetComponent<MatchedPileScript>().cardList);
        //clcikedCard.GetComponent<CardScript>().MoveCard(matchedPile, matchedPile.GetComponent<MatchedPileScript>().cardList);
    }

    public bool MatchSuit()
    {
        //just to make it cleaner because this utils.blah blah blah is yucky
        //basically a string of if/else cases for matching
        string selectedCardSuit = selectedCards[0].GetComponent<CardScript>().cardSuit;
        string clickedCardSuit = clickedCard.GetComponent<CardScript>().cardSuit;
        //hearts diamond combo #1
        if (selectedCardSuit.Equals("hearts") && clickedCardSuit.Equals("diamonds"))
        {
            return true;
        }
        //hearts diamond combo #2
        else if (selectedCardSuit.Equals("diamonds") && clickedCardSuit.Equals("hearts"))
        {
            return true;
        }
        //spades clubs combo #1
        else if (selectedCardSuit.Equals("spades") && clickedCardSuit.Equals("clubs"))
        {
            return true;
        }
        //spades clubs combo #2
        else if (selectedCardSuit.Equals("clubs") && clickedCardSuit.Equals("spades"))
        {
            return true;
        }
        //otherwise not a match 
        else
        {
            return false;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Click();
        }

    }



    //int forCounter;
    //for (forCounter = 0; forCounter < selectedCards.Count; forCounter++)
    //{
    //    selectedCards[forCounter].GetComponent<CardScript>().apearSelected = true;
    //    selectedCards[forCounter].GetComponent<CardScript>().SetCardAppearance();
    //}

}
