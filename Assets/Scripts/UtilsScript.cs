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

        //check if collider is null
        if (hit.collider != null)
        {
            GameObject container = hit.collider.GetComponent<CardScript>().container;

            if (container.CompareTag("Foundation"))
            {
                container.GetComponent<FoundationScript>().Clicked();
            }

            else if (container.CompareTag("Reactor"))
            {
                container.GetComponent<ReactorScript>().Clicked();
            }

            else if (container.CompareTag("Wastepile"))
            {
                container.GetComponent<WastepileScript>().Clicked();
            }

            else if (container.CompareTag("Deck"))
            {
                container.GetComponent<DeckScript>().Clicked();
            }
        }
    }


    public void Match()
    {
        selectedCards[0].GetComponent<CardScript>().MoveCard(matchedPile, matchedPile.GetComponent<MatchedPileScript>().cardList);
        clickedCard.GetComponent<CardScript>().MoveCard(matchedPile, matchedPile.GetComponent<MatchedPileScript>().cardList);
    }


    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Click();
        }

        int forCounter;
        for (forCounter = 0; forCounter < selectedCards.Count; forCounter++)
        {
            selectedCards[forCounter].GetComponent<CardScript>().apearSelected = true;
            selectedCards[forCounter].GetComponent<CardScript>().SetCardAppearance();
        }
    }
}
