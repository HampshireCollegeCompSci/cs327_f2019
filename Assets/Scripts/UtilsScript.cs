using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilsScript : MonoBehaviour
{
    public static UtilsScript global; //Creates a new instance if one does not yet exist
    public List<GameObject> selectedCards;
    public GameObject matchedPile;
    public int indexCounter;
    public RaycastHit2D hit;

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

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Click();
        }

    }

    public void SelectCard(GameObject inputCard)
    {
        selectedCards.Add(inputCard);
        inputCard.GetComponent<CardScript>().appearSelected = true;
        inputCard.GetComponent<CardScript>().SetCardAppearance();
    }

    public void DeselectCard(GameObject inputCard)
    {
        inputCard.GetComponent<CardScript>().appearSelected = false;
        inputCard.GetComponent<CardScript>().SetCardAppearance();
        selectedCards.Remove(inputCard);
    }

    public void SelectMultipleCards(int cardsToCount) //selects multipule cards
    {
        for (indexCounter = cardsToCount; indexCounter + 1 > 0; indexCounter--)
        {
            SelectCard(hit.collider.gameObject.GetComponent<CardScript>().container.GetComponent<FoundationScript>().cardList[indexCounter]);
        }
    }

    public int countCardsToSelect(GameObject selectedCard) //takes the input of a selected card and counts how many cards are below it in a foundation
    {
        for (indexCounter = hit.collider.gameObject.GetComponent<CardScript>().container.GetComponent<FoundationScript>().cardList.Count - 1; indexCounter > 0; indexCounter--)
        {
            if (selectedCard == hit.collider.gameObject.GetComponent<CardScript>().container.GetComponent<FoundationScript>().cardList[indexCounter])
            {
                Debug.Log("indexCounter " + (indexCounter + 1));
                return indexCounter;
            }
        }

        return 0;
    }

    //sends out a raycast to see you selected something
    public void Click()
    {
        hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)), Vector2.zero);

        if (!hit.collider.gameObject.CompareTag("Card"))
        {
            if (selectedCards.Count != 0)
            {
                selectedCards[0].GetComponent<CardScript>().container.SendMessage("ProcessAction", hit.collider.gameObject);
                int selectedCardsLength = selectedCards.Count;
                for (int i = 0; i < selectedCardsLength; i++)
                {
                    DeselectCard(selectedCards[0]);
                }
            }
            return;
        }

        // if we click a car in the deck call deck clicked and deselect all cards
        else if (hit.collider.gameObject.GetComponent<CardScript>().container.CompareTag("Deck"))
        {
            hit.collider.gameObject.GetComponent<CardScript>().container.SendMessage("ProcessAction", hit.collider.gameObject);
            int selectedCardsLength = selectedCards.Count;
            for (int i = 0; i < selectedCardsLength; i++)
            {
                DeselectCard(selectedCards[0]);
            }
            return;
        }

        else if (hit.collider.gameObject.GetComponent<CardScript>().container.CompareTag("Wastepile") && selectedCards.Count == 0)
        {
            if (hit.collider.gameObject.GetComponent<CardScript>().container.GetComponent<WastepileScript>().cardList[0] == hit.collider.gameObject)
            {
                SelectCard(hit.collider.gameObject);
            }
        }

        else if (selectedCards.Count == 0 && !hit.collider.gameObject.GetComponent<CardScript>().hidden)
        {
            SelectMultipleCards(countCardsToSelect(hit.collider.gameObject));
        }

        else if (selectedCards[0] == hit.collider.gameObject)
        {
            DeselectCard(hit.collider.gameObject);
        }

        else
        {
            selectedCards[0].GetComponent<CardScript>().container.SendMessage("ProcessAction", hit.collider.gameObject);
            //we are no longer changing a list that we are also iterating over
            int selectedCardsLength = selectedCards.Count;
            for (int i = 0; i < selectedCardsLength; i++)
            {
                DeselectCard(selectedCards[0]);
            }
        }
    }


    public void Match(GameObject card1, GameObject card2)
    {
        card1.GetComponent<CardScript>().MoveCard(matchedPile);
        card2.GetComponent<CardScript>().MoveCard(matchedPile);
    }

    //checks if suit match AND value match
    public bool IsMatch(GameObject card1, GameObject card2)
    {
        //just to make it cleaner because this utils.blah blah blah is yucky
        //basically a string of if/else cases for matching
        string card1Suit = card1.GetComponent<CardScript>().cardSuit;
        string card2Suit = card2.GetComponent<CardScript>().cardSuit;
        int card1Num = card1.GetComponent<CardScript>().cardNum;
        int card2Num = card2.GetComponent<CardScript>().cardNum;
        if (card1Num != card2Num)
        {
            Debug.Log("Numbers don't match");
            return false;
        }
        else { 
        //hearts diamond combo #1
            if (card1Suit.Equals("hearts") && card2Suit.Equals("diamonds"))
            {
                return true;
            }
            //hearts diamond combo #2
            else if (card1Suit.Equals("diamonds") && card2Suit.Equals("hearts"))
            {
                return true;
            }
            //spades clubs combo #1
            else if (card1Suit.Equals("spades") && card2Suit.Equals("clubs"))
            {
                return true;
            }
            //spades clubs combo #2
            else if (card1Suit.Equals("clubs") && card2Suit.Equals("spades"))
            {
                return true;
            }
            //otherwise not a match 
            else
            {
                Debug.Log("Suits don't match");
                return false;
            }
        }
    }

    public bool IsSameSuit(GameObject card1, GameObject card2)
    {
        string card1Suit = card1.GetComponent<CardScript>().cardSuit;
        string card2Suit = card2.GetComponent<CardScript>().cardSuit;

        if (card1Suit.Equals("hearts") && card2Suit.Equals("hearts"))
        {
            return true;
        }
        else if (card1Suit.Equals("diamonds") && card2Suit.Equals("diamonds"))
        {
            return true;
        }
        else if (card1Suit.Equals("spades") && card2Suit.Equals("spades"))
        {
            return true;
        }
        else if (card1Suit.Equals("clubs") && card2Suit.Equals("clubs"))
        {
            return true;
        }
        //otherwise not a match 
        else
        {
            Debug.Log("Suits don't match");
            return false;
        }
    }

}
