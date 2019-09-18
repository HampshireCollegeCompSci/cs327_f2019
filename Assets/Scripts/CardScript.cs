using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardScript : MonoBehaviour
{
    public bool hidden;
    public Sprite cardSprite;
    public Sprite cardBackSprite;
    public int cardNum;
    public string cardSuit;
    public bool isSelected;
   
    void Update()
    {
        if(hidden == true)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = cardBackSprite;
        }

        if (hidden == false)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = cardSprite;
        }

        if(isSelected == true)
        {
            gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        }

        if (isSelected == false)
        {
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }
}
