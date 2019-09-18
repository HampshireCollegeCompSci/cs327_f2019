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
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
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
    }
}
