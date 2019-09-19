using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectScript : MonoBehaviour
{
    public GameObject[] selectedCards;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)), Vector2.zero);

            if (hit.collider.CompareTag("Foundation") && selectedCards[0] == null)
            {
                selectedCards[0] = hit.collider.transform.gameObject.GetComponent<FoundationScript>().cardList[0];
                hit.collider.transform.gameObject.GetComponent<FoundationScript>().cardList[0].gameObject.GetComponent<CardScript>().isSelected = true;
            }

            else if(hit.collider.CompareTag("Foundation") && selectedCards[0] == hit.collider.transform.gameObject.GetComponent<FoundationScript>().cardList[0])
            {
                selectedCards[0] = null;
                hit.collider.transform.gameObject.GetComponent<FoundationScript>().cardList[0].gameObject.GetComponent<CardScript>().isSelected = false;
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)), Vector2.zero);

            if (hit.collider.CompareTag("Foundation") && selectedCards[0] != null)
            {
                hit.collider.transform.gameObject.GetComponent<FoundationScript>().AddCard(selectedCards[0],0);
               selectedCards[0].transform.parent.transform.gameObject.GetComponent<FoundationScript>().RemoveCard(0);
            }
        }
    }
}
