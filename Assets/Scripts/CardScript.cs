using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
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
    public Color originalColor;
    public Color newColor;

    private GameObject hologramObject;
    private GameObject hologram;

    Vector3 originalTransform;

    void Start()
    {
        originalTransform = Config.config.cardScale * .1f;
        if (Config.config.prettyColors)
        {
            originalColor = new Color(Random.Range(0.4f, 1), Random.Range(0.4f, 1f), Random.Range(0.4f, 1f), 1);
        }

        else
        {
            originalColor = new Color(1, 1, 1, 1);
        }
        SetCardAppearance();
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
            newColor = gameObject.GetComponent<SpriteRenderer>().material.color;
            newColor.a = Config.config.selectedCardOpacity;
            gameObject.GetComponent<SpriteRenderer>().material.color = newColor;
            if (hologram != null && hologramObject != null)
            {
                hologram.GetComponent<SpriteRenderer>().color = newColor;
                hologramObject.GetComponent<SpriteRenderer>().color = newColor;
            }
            //gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        }

        //makes card normal if not selected
        else if (appearSelected == false)
        {
            gameObject.transform.localScale = originalTransform;
            gameObject.GetComponent<SpriteRenderer>().material.color = originalColor;
            if (hologram != null && hologramObject != null)
            {
                hologram.GetComponent<SpriteRenderer>().color = originalColor;
                hologramObject.GetComponent<SpriteRenderer>().color = originalColor;
            }
            //gameObject.transform.localScale = new Vector3(1, 1, 1);
        }

        //set collider scale to match sprite scale
        gameObject.GetComponent<BoxCollider2D>().size = gameObject.GetComponent<SpriteRenderer>().sprite.bounds.size;
        gameObject.GetComponent<BoxCollider2D>().offset = gameObject.GetComponent<SpriteRenderer>().sprite.bounds.center;
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
            destination.GetComponent<ReactorScript>().soundController.CardToReactorSound();
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

    public void ShowHologram()
    {
        if (hologram == null)
        {
            GameObject hologramPrefab = Resources.Load<GameObject>("Prefabs/Holograms/hologram");
            hologram = Instantiate(hologramPrefab, gameObject.transform.position - new Vector3(0, -0.5f, 0), gameObject.transform.rotation);

            if (cardSuit == "spades")
            {
                GameObject hologramObjectPrefab = Resources.Load<GameObject>("Prefabs/Holograms/spades");
                hologramObject = Instantiate(hologramObjectPrefab, gameObject.transform.position - new Vector3(0, -1.1f, 0), gameObject.transform.rotation);
            }
            else if (cardSuit == "diamonds")
            {
                GameObject hologramObjectPrefab = Resources.Load<GameObject>("Prefabs/Holograms/rhombus");
                hologramObject = Instantiate(hologramObjectPrefab, gameObject.transform.position - new Vector3(0, -1.1f, 0), gameObject.transform.rotation);
            }
            else if (cardSuit == "hearts")
            {
                GameObject hologramObjectPrefab = Resources.Load<GameObject>("Prefabs/Holograms/hearts");
                hologramObject = Instantiate(hologramObjectPrefab, gameObject.transform.position - new Vector3(0, -1.1f, 0), gameObject.transform.rotation);
            }
            else if (cardSuit == "clubs")
            {
                GameObject hologramObjectPrefab = Resources.Load<GameObject>("Prefabs/Holograms/clubs");
                hologramObject = Instantiate(hologramObjectPrefab, gameObject.transform.position - new Vector3(0, -1.1f, 0), gameObject.transform.rotation);
            }
        }
        else
        {
            hologram.SetActive(true);
            hologramObject.SetActive(true);
            hologram.transform.position = gameObject.transform.position - new Vector3(0, -0.5f, 0);
            hologramObject.transform.position = gameObject.transform.position - new Vector3(0, -1.1f, 0);
        }
    }

    public void DestroyHologram()
    {
        if (hologram != null)
        {
            hologram.SetActive(false);
            hologramObject.SetActive(false);
        }
    }
}

