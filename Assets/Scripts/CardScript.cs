using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardScript : MonoBehaviour
{
    public bool hidden;
    public Sprite cardSprite;
    public Sprite cardBackSprite;
    public int cardVal;
    //cardVal is what the card is worth to the reactor jack, queen, king are all 10
    public string cardSuit;
    public string cardNum;
    //cardNum is the number on the card, ace is 1 jack is 11 queen is 12 king is 13
    public bool isSelected;

    void SetCardAppearance()
    {
        //shows card back if it's hidden
        if (hidden == true)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = cardBackSprite;
        }

        //shows card if it's not hidden
        if (hidden == false)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = cardSprite;
        }

        //makes card larger and first in sorting order if the card is selected
        if (isSelected == true)
        {
            gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "SelectedCard";
        }

        //makes card normal if not selected
        if (isSelected == false)
        {
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        }
    }
   
    void Update()
    {
        SetCardAppearance();
    }
}
