using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationScript : MonoBehaviour
{
    public List<GameObject> cardList;
    public bool addCard;
    public bool removeCard;
    public GameObject cardToAdd;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (removeCard == true)
        {
            cardList.Remove(cardList[0]);
            removeCard = false;
        }

        if (addCard == true)
        {
            cardList.Insert(0, cardToAdd);
            addCard = false;
        }
    }
}
