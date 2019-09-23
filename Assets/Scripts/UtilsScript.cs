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


    public void Match()
    {
        selectedCards[0].GetComponent<CardScript>().MoveCard(matchedPile, matchedPile.GetComponent<MatchedPileScript>().cardList);
        clcikedCard.GetComponent<CardScript>().MoveCard(matchedPile, matchedPile.GetComponent<MatchedPileScript>().cardList);
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
