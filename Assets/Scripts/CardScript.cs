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
   
    void Update()
    {
        if (hidden == true)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = cardBackSprite;
        }

        if (hidden == false)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = cardSprite;
        }

        if (isSelected == true)
        {
            gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        }

        if (isSelected == false)
        {
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }
}
