using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalScript : MonoBehaviour
{
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
            HitFoundation(hit);
        }
    }

    //changes selections if you hit a foundation
    void HitFoundation(RaycastHit2D hit)
    {

        if (selectedCards[0] == null)
        {
            SelectCard(hit);
        }

        else if (selectedCards[0] == hit.collider.GetComponent<FoundationScript>().cardList[0])
        {
            DeselectCard(hit);
        }

        else if (selectedCards[0] != hit.collider.GetComponent<FoundationScript>().cardList[0])
        {
            MoveCard(hit);
        }
    }

    //sets selected card to the top card of the selected foundation
    void SelectCard(RaycastHit2D hit)
    {
        selectedCards[0] = hit.collider.GetComponent<FoundationScript>().cardList[0];
        hit.collider.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().apearSelected = true;
    }

    //deselects selected card
    void DeselectCard(RaycastHit2D hit)
    {
        selectedCards[0] = null;
        hit.collider.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>().apearSelected = false;
    }

    //moves selected card from one foundation to another
    void MoveCard(RaycastHit2D hit)
    {
        hit.collider.GetComponent<FoundationScript>().AddCard(selectedCards[0], 0);
        selectedCards[0].transform.parent.GetComponent<FoundationScript>().RemoveCard(0);
        selectedCards[0].gameObject.GetComponent<CardScript>().apearSelected = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Click();
        }
    }
}
