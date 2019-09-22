using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilsScript : MonoBehaviour
{
    public static UtilsScript global; //Creates a new instance if one does not yet exist
    public List<GameObject> selectedCards;
    public GameObject clcikedCard;
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

    public bool isMatch(GameObject card1, GameObject card2)
    {
        //chechs if two cards are a valid match
    }

<<<<<<< HEAD
    public void Match(GameObject card1, GameObject card2)
    {
        //clcikedCard.GetComponent<CardScript>().container = 
    }
    
    
=======
    public void Match()
    {
        selectedCards[0].GetComponent<CardScript>().MoveCard(matchedPile, matchedPile.GetComponent<MatchedPileScript>().cardList);
        clcikedCard.GetComponent<CardScript>().MoveCard(matchedPile, matchedPile.GetComponent<MatchedPileScript>().cardList);
    }


>>>>>>> origin/FirstBuild
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Click();
        }
<<<<<<< HEAD
    }

    /*
    This is old code that should be removed but it has pieces that could be helpful when building scripts for Countainers and Card
    //changes selections if you hit a foundation

    void HitFoundation(FoundationScript foundationScript)
    {

        if (selectedCards[0] == null)
        {
            SelectCard(foundationScript);
        }

        else if (selectedCards[0] == foundationScript.cardList[0])
        {
            DeselectCard(foundationScript);
        }

        else if (selectedCards[0] != foundationScript.cardList[0])
        {
            MoveCard(foundationScript);
        }
    }

    //sets selected card to the top card of the selected foundation
    void SelectCard(FoundationScript foundationScript)
    {
        selectedCards[0] = foundationScript.cardList[0];
        foundationScript.cardList[0].GetComponent<CardScript>().apearSelected = true;
    }

    //deselects selected card
    void DeselectCard(FoundationScript foundationScript)
    {
        selectedCards[0] = null;
        foundationScript.cardList[0].GetComponent<CardScript>().apearSelected = false;
    }

    //moves selected card from one foundation to another
    void MoveCard(FoundationScript foundationScript)
    {
        foundationScript.AddCard(selectedCards[0], 0);
        selectedCards[0].transform.parent.GetComponent<FoundationScript>().RemoveCard(0);
        selectedCards[0].gameObject.GetComponent<CardScript>().apearSelected = false;
        selectedCards[0] = null;
    }
    */

=======
    }
>>>>>>> origin/FirstBuild
}
