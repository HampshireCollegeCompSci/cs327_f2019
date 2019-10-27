using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardScript : MonoBehaviour
{
    public GameObject container;
    public Sprite cardFrontSprite;
    public Sprite cardBackSprite;
    public int cardVal; //cardVal is what the card is worth to the reactor jack, queen, king are all 10
    public int cardNum; //cardNum is the number on the card, ace is 1 jack is 11 queen is 12 king is 13
    public string cardSuit;
    public bool hidden;
    public bool appearSelected;
    Vector3 originalTransform;

    void Start()
    {
        originalTransform = Config.config.cardScale * .1f;
        SetCardAppearance();
    }

    void Update()
    {
        return;
    }

    //all the scales in here have been modified deliberately because the cards were too small
    //this will need to be changed when the sprites for the final card designs are added
    //unless they have the same exact dimensions
    public void SetCardAppearance()
    {
        //shows card back if it's hidden
        if (hidden)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = cardBackSprite;
            //gameObject.transform.localScale = new Vector3(2.8f, 2.8f, 2.8f);
        }

        //shows card if it's not hidden
        else if (hidden == false)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = cardFrontSprite;
            //gameObject.transform.localScale = new Vector3(2.8f, 2.8f, 2.8f);
        }

        //makes card larger and first in sorting order if the card is selected
        if (appearSelected)
        {
            gameObject.transform.localScale = originalTransform * 1.1f;
            //gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        }

        //makes card normal if not selected
        else if (appearSelected == false)
        {
            gameObject.transform.localScale = originalTransform;
            //gameObject.transform.localScale = new Vector3(1, 1, 1);
        }

        //set collider scale to match sprite scale
        gameObject.GetComponent<BoxCollider2D>().size = gameObject.GetComponentInChildren<SpriteRenderer>().sprite.bounds.size;
        gameObject.GetComponent<BoxCollider2D>().offset = gameObject.GetComponentInChildren<SpriteRenderer>().sprite.bounds.center;
    }

    public void MoveCard(GameObject destination)
    {
        if (destination.CompareTag("Foundation"))
        {
            container.SendMessage("RemoveCard", gameObject);
            destination.GetComponent<FoundationScript>().cardList.Insert(0, gameObject);
        }

        else if (destination.CompareTag("Reactor"))
        {
            container.SendMessage("RemoveCard", gameObject);
            destination.GetComponent<ReactorScript>().cardList.Insert(0, gameObject);
        }

        else if (destination.CompareTag("Wastepile"))
        {
            container.SendMessage("RemoveCard", gameObject);
            destination.GetComponent<WastepileScript>().cardList.Insert(0, gameObject);
        }
        else if (destination.CompareTag("Deck"))
        {
            container.SendMessage("RemoveCard", gameObject);
            destination.GetComponent<DeckScript>().cardList.Insert(0, gameObject);
        }
        else if (destination.CompareTag("MatchedPile"))
        {
            container.SendMessage("RemoveCard", gameObject);
            destination.GetComponent<MatchedPileScript>().cardList.Insert(0, gameObject);
        }
        container.SendMessage("SetCardPositions");
        container = destination;
        destination.SendMessage("SetCardPositions");
    }

    public void MakeVisualOnly()
    {
        container = null;
        Destroy(gameObject.GetComponent<BoxCollider2D>());
    }


}

