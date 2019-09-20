using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilsScript : MonoBehaviour
{
    public static UtilsScript global; //Creates a new instance if one does not yet exist

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




    public GameObject[] selectedCards;
    // Start is called before the first frame update
    void Start()
    {

    }

    //sends out a raycast to see you selected something
    void Click()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)), Vector2.zero);

        if (hit.collider.CompareTag("Foundation"))
        {
            HitFoundation(hit.collider.GetComponent<FoundationScript>());
        }
    }

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

    
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Click();
        }
    }
    
}
