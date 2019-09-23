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
    public bool apearSelected;
        
    void Update()
    {
        SetCardAppearance();
        container = transform.parent.gameObject;
    }

    public void SetCardAppearance()
    {
        //shows card back if it's hidden
        if (hidden)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = cardBackSprite;
        }

        //shows card if it's not hidden
        else if (hidden == false)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = cardFrontSprite;
        }

        //makes card larger and first in sorting order if the card is selected
        if (apearSelected)
        {
            gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "SelectedCard";
        }

        //makes card normal if not selected
        else if (apearSelected == false)
        {
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        }
    }

    public void AddCard()
    {
        return;
    }

    public void RemoveCard()
    {
        return;
    }

    public void MoveCard(GameObject newCardLocation, List<GameObject> cardList)
    {
        return;
    }


}