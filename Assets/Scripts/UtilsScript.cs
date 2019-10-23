using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UtilsScript : MonoBehaviour
{
    public static UtilsScript global; //Creates a new instance if one does not yet exist
    public List<GameObject> selectedCards;
    private List<GameObject> selectedCardsCopy = new List<GameObject>();
    public GameObject matchedPile;
    public GameObject gameUI;
    public SoundController soundController;
    public int indexCounter;
    public RaycastHit2D hit;
    private bool dragOn;
    private GameObject newGameObject;


    public int matchPoints = Config.config.matchPoints;
    public int emptyReactorPoints = Config.config.emptyReactorPoints;
    public int PerfectGamePoints = Config.config.perfectGamePoints;


    public void SetCards()
    {
        matchedPile = GameObject.Find("MatchedPile");
        gameUI = GameObject.Find("GameUI");
    }

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
        if (!Config.config.gameOver && !Config.config.gamePaused)
        {
            if (Input.GetMouseButtonDown(0) && dragOn == false && SceneManager.GetActiveScene().buildIndex == 2)
            {

                Click();
                if (selectedCards.Count > 0)
                {
                    soundController.CardPressSound();
                    dragOn = true;
                }

                //sets the reactor scores
                gameUI.GetComponent<ReactorScoreSetScript>().SetReactorScore();

                //checks if the game has been won

                /*this code is if we want to check cards in the deck and the wastepile as well as the foundations to see if you can win the game
                 * if (Config.config.CountFoundationCards() + Config.config.wastePile.GetComponent<WastepileScript>().cardList.Count +
                   Config.config.deck.GetComponent<DeckScript>().cardList.Count == 0)*/
            }

            if (Input.GetMouseButtonUp(0) && selectedCardsCopy.Count > 0 && SceneManager.GetActiveScene().buildIndex == 2)
            {

                Click();
                gameUI.GetComponent<ReactorScoreSetScript>().SetReactorScore();

                foreach (GameObject card in selectedCardsCopy)
                {
                    Destroy(card);
                }

                selectedCardsCopy.Clear();
                dragOn = false;
            }

            if (dragOn == true && SceneManager.GetActiveScene().buildIndex == 2)
            {

                ClickAndDrag(selectedCardsCopy);
            }

            if (Config.config.CountFoundationCards() == 0)
            {
                SetEndGameScore();
                Config.config.gameOver = true;
                Config.config.gameWin = true;
            }
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

    public void SelectMultipleCards(int cardsToCount)
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
        //raycast to see what we clicked
        hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)), Vector2.zero);

        //!!!
        //BUG: UtilScript still raycast in other scenes and throws NUllPointerException.
        //!!!

        //if we clicked a button activates the button
        /*
        if (hit.collider.gameObject.CompareTag("Button"))
        {
            hit.collider.gameObject.SendMessage("ProcessAction", hit.collider.gameObject);
        }*/

        //if we click a deck activates deck and deselected our cards
        if (!hit.collider.gameObject.CompareTag("Card"))
        {
            if (hit.collider.gameObject.CompareTag("Deck"))
            {
                hit.collider.gameObject.GetComponent<DeckScript>().ProcessAction(hit.collider.gameObject);
            }

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

        // if we click a card in the deck call deck clicked and deselect all cards
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

        //if we click a card in the wastepile and we don't have any card selected select the card in the wastepile
        else if (hit.collider.gameObject.GetComponent<CardScript>().container.CompareTag("Wastepile") && selectedCards.Count == 0)
        {
            if (hit.collider.gameObject.GetComponent<CardScript>().container.GetComponent<WastepileScript>().cardList[0] == hit.collider.gameObject)
            {
                SelectCard(hit.collider.gameObject);
            }
        }

        //if we click a card in a reactor and we don't have any card selected select the card in the reactor
        else if (hit.collider.gameObject.GetComponent<CardScript>().container.CompareTag("Reactor") && selectedCards.Count == 0)
        {
            if (hit.collider.gameObject.GetComponent<CardScript>().container.GetComponent<ReactorScript>().cardList[0] == hit.collider.gameObject)
            {
                SelectCard(hit.collider.gameObject);
            }
        }

        //if we click a card in a foundation and we don't have any card selected and the card we're trying to select is not hidden select the card in the foundation
        else if (selectedCards.Count == 0 && !hit.collider.gameObject.GetComponent<CardScript>().hidden &&
            hit.collider.gameObject.GetComponent<CardScript>().container.CompareTag("Foundation"))
        {
            SelectMultipleCards(countCardsToSelect(hit.collider.gameObject));
        }


        //if we click on our first selected card deselect all cards
        else if (selectedCards[0] == hit.collider.gameObject)
        {
            int selectedCardsLength = selectedCards.Count;
            for (int i = 0; i < selectedCardsLength; i++)
            {
                Debug.Log(i);
                DeselectCard(selectedCards[0]);
            }
        }

        //if we click on something else tries to move the selected cards 
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
        soundController.CardMatchSound();
        card1.GetComponent<CardScript>().MoveCard(matchedPile);
        card2.GetComponent<CardScript>().MoveCard(matchedPile);

        Config.config.score += matchPoints;
        //check to see if the board is clear
    }

    //checks if suit match AND value match
    public bool IsMatch(GameObject card1, GameObject card2)
    {

        Debug.Log(card1.GetComponent<CardScript>().cardSuit + card1.GetComponent<CardScript>().cardNum);
        Debug.Log(card2.GetComponent<CardScript>().cardSuit + card2.GetComponent<CardScript>().cardNum);
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
        else
        {
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

    public void SetEndGameScore()
    {
        if (matchedPile.GetComponent<MatchedPileScript>().cardList.Count == 52)
        {
            Config.config.score += PerfectGamePoints;
        }

        if (Config.config.reactor1.GetComponent<ReactorScript>().cardList.Count == 0)
        {
            Config.config.score += emptyReactorPoints;
        }

        if (Config.config.reactor2.GetComponent<ReactorScript>().cardList.Count == 0)
        {
            Config.config.score += emptyReactorPoints;
        }

        if (Config.config.reactor3.GetComponent<ReactorScript>().cardList.Count == 0)
        {
            Config.config.score += emptyReactorPoints;
        }

        if (Config.config.reactor4.GetComponent<ReactorScript>().cardList.Count == 0)
        {
            Config.config.score += emptyReactorPoints;
        }
    }

    public void ClickAndDrag(List<GameObject> cards)
    {

        if (cards.Count.Equals(0))
        {
            foreach (GameObject card in selectedCards)
            {
                newGameObject = (GameObject)Instantiate(card, Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)), Quaternion.identity);
                newGameObject.GetComponent<CardScript>().MakeVisualOnly();
                cards.Add(newGameObject);
            }
        }

        foreach (GameObject card in cards)
        {
            card.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y + card.transform.position.y, 1));
        }
    }




}
