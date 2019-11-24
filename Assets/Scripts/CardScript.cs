using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class CardScript : MonoBehaviour
{
    public UtilsScript utils;
    public GameObject container;
    public Sprite cardFrontSprite;
    public Sprite cardBackSprite;
    public int cardVal; //cardVal is what the card is worth to the reactor jack, queen, king are all 10
    public int cardNum; //cardNum is the number on the card, ace is 1 jack is 11 queen is 12 king is 13
    public string cardSuit;
    public bool hidden;
    public Color originalColor;
    public Color newColor;
    public bool glowing;

    private GameObject hologramObject;
    private GameObject hologram;

    void Start()
    {
        glowing = true;
        GlowOff();

        if (Config.config.prettyColors)
        {
            originalColor = new Color(Random.Range(0.4f, 1), Random.Range(0.4f, 1f), Random.Range(0.4f, 1f), 1);
        }
        else
        {
            originalColor = new Color(1, 1, 1, 1);
        }
    }


    //all the scales in here have been modified deliberately because the cards were too small
    //this will need to be changed when the sprites for the final card designs are added
    //unless they have the same exact dimensions
    //public void SetCardAppearance()
    //{
        //set collider scale to match sprite scale
        //gameObject.GetComponent<BoxCollider2D>().size = gameObject.GetComponent<SpriteRenderer>().sprite.bounds.size;
        //gameObject.GetComponent<BoxCollider2D>().offset = gameObject.GetComponent<SpriteRenderer>().sprite.bounds.center;
    //}

    public void SetVisibility(bool show)
    {
        if (show)
        {
            //Debug.Log("showing card" + cardNum + cardSuit);
            gameObject.GetComponent<SpriteRenderer>().sprite = cardFrontSprite;
            hidden = false;
        }
        else
        {
            //Debug.Log("hiding card" + cardNum + cardSuit);
            gameObject.GetComponent<SpriteRenderer>().sprite = cardBackSprite;
            HideHologram();
            hidden = true;
        }
    }

    public bool isHidden()
    {
        return hidden;
    }

    public void SetSelected(bool selected)
    {
        //makes card larger and first in sorting order if the card is selected
        if (selected)
        {
            newColor = gameObject.GetComponent<SpriteRenderer>().material.color;
            newColor.a = Config.config.selectedCardOpacity;
            gameObject.GetComponent<SpriteRenderer>().material.color = newColor;
            if (hologram != null && hologramObject != null)
            {
                hologram.GetComponent<SpriteRenderer>().color = newColor;
                hologramObject.GetComponent<SpriteRenderer>().color = newColor;
            }
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().material.color = originalColor;
            if (hologram != null && hologramObject != null)
            {
                hologram.GetComponent<SpriteRenderer>().color = originalColor;
                hologramObject.GetComponent<SpriteRenderer>().color = originalColor;
            }
        }
    }

    public void UpdateMaskInteraction(SpriteMaskInteraction update)
    {
        gameObject.GetComponent<SpriteRenderer>().maskInteraction = update;
        if (hologram != null && hologramObject != null)
        {
            hologram.GetComponent<SpriteRenderer>().maskInteraction = update;
            hologramObject.GetComponent<SpriteRenderer>().maskInteraction = update;
        }
    }

    public void MoveCard(GameObject destination, bool doLog = true, bool isAction = true, bool isCycle = false, bool removeUpdateHolo = true, bool addUpdateHolo = true)
    {
        bool nextCardWasHidden = false;
        if (container.CompareTag("Foundation"))
        {
            if (doLog)
            {
                List<GameObject> containerCardList = container.GetComponent<FoundationScript>().cardList;
                if (containerCardList.Count > 1 && containerCardList[1].GetComponent<CardScript>().isHidden())
                {
                    //Debug.Log("ncwh");
                    nextCardWasHidden = true;
                }
            }
            container.GetComponent<FoundationScript>().RemoveCard(gameObject, removeUpdateHolo);
        }
        else if (container.CompareTag("Reactor"))
        {
            container.GetComponent<ReactorScript>().RemoveCard(gameObject);
        }
        else if (container.CompareTag("Wastepile"))
        {
            container.GetComponent<WastepileScript>().RemoveCard(gameObject, removeUpdateHolo);
        }
        else if (container.CompareTag("Deck"))
        {
            container.GetComponent<DeckScript>().RemoveCard(gameObject);
        }
        else if (container.CompareTag("MatchedPile"))
        {
            container.GetComponent<MatchedPileScript>().RemoveCard(gameObject);
        }

        if (destination.CompareTag("Foundation"))
        {
            if (doLog)
            {
                UndoScript.undoScript.logMove("move", gameObject, isAction, Config.config.actions, nextCardWasHidden);
            }

            destination.GetComponent<FoundationScript>().AddCard(gameObject, addUpdateHolo);
        }
        else if (destination.CompareTag("Reactor"))
        {
            if (doLog)
            {
                if (isCycle)
                {
                    UndoScript.undoScript.logMove("cycle", gameObject, true, Config.config.actions, nextCardWasHidden);
                }
                else
                {
                    UndoScript.undoScript.logMove("move", gameObject, isAction, Config.config.actions, nextCardWasHidden);
                }
            }

            destination.GetComponent<ReactorScript>().AddCard(gameObject);
        }
        else if (destination.CompareTag("Wastepile"))
        {
            if (doLog)
            {
                UndoScript.undoScript.logMove("draw", gameObject, isAction, Config.config.actions, nextCardWasHidden);
            }

            destination.GetComponent<WastepileScript>().AddCard(gameObject, addUpdateHolo);
        }
        else if (destination.CompareTag("Deck"))
        {
            destination.GetComponent<DeckScript>().AddCard(gameObject);
        }
        else if (destination.CompareTag("MatchedPile"))
        {
            if (doLog)
            {
                UndoScript.undoScript.logMove("match", gameObject, isAction, Config.config.actions, nextCardWasHidden);
            }

            destination.GetComponent<MatchedPileScript>().AddCard(gameObject);
        }

        container = destination;
        utils.CheckGameOver();
    }

    public void MakeVisualOnly()
    {
        gameObject.transform.localScale = new Vector3(0.2f, 0.2f, 1);
        container = null;
        Destroy(gameObject.GetComponent<BoxCollider2D>());
    }

    public void ShowHologram()
    {
        if (hologram != null)
        {
            //Debug.Log("already holo" + cardNum + cardSuit);
            return;
        }
        //Debug.Log("showing holo" + cardNum + cardSuit);

        GameObject hologramPrefab = Resources.Load<GameObject>("Prefabs/Holograms/hologram");
        GameObject hologramFoodPrefab = Resources.Load<GameObject>("Prefabs/Holograms/generalFood");

        string cardHologramName;
        if (cardNum == 1)
            cardHologramName = "ace_" + cardSuit + "_food";
        else if (cardNum == 11)
            cardHologramName = "jack_" + cardSuit + "_food";
        else if (cardNum == 12)
            cardHologramName = "queen_" + cardSuit + "_food";
        else if (cardNum == 13)
            cardHologramName = "king_" + cardSuit + "_food";
        else
            cardHologramName = cardNum + "_" + cardSuit + "_food";

        hologramFoodPrefab.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/FoodHolograms/" + cardHologramName);

        hologram = Instantiate(hologramPrefab, Vector3.one, gameObject.transform.rotation);
        hologramObject = Instantiate(hologramFoodPrefab, Vector3.one, gameObject.transform.rotation);

        hologram.transform.parent = this.gameObject.transform;
        hologramObject.transform.parent = this.gameObject.transform;

        hologram.transform.localPosition = new Vector3(0.35f, 4, -1);
        hologramObject.transform.localPosition = new Vector3(0, 4.3f, 0);

        //if (cardSuit == "spades")
        //{
        //    GameObject hologramObjectPrefab = Resources.Load<GameObject>("Prefabs/Holograms/spades");
        //    hologramObject = Instantiate(hologramObjectPrefab, gameObject.transform.position - new Vector3(0, -1.1f, 0), gameObject.transform.rotation);
        //}
        //else if (cardSuit == "diamonds")
        //{
        //    GameObject hologramObjectPrefab = Resources.Load<GameObject>("Prefabs/Holograms/rhombus");
        //    hologramObject = Instantiate(hologramObjectPrefab, gameObject.transform.position - new Vector3(0, -1.1f, 0), gameObject.transform.rotation);
        //}
        //else if (cardSuit == "hearts")
        //{
        //    GameObject hologramObjectPrefab = Resources.Load<GameObject>("Prefabs/Holograms/hearts");
        //    hologramObject = Instantiate(hologramObjectPrefab, gameObject.transform.position - new Vector3(0, -1.1f, 0), gameObject.transform.rotation);
        //}
        //else if (cardSuit == "clubs")
        //{
        //    GameObject hologramObjectPrefab = Resources.Load<GameObject>("Prefabs/Holograms/clubs");
        //    hologramObject = Instantiate(hologramObjectPrefab, gameObject.transform.position - new Vector3(0, -1.1f, 0), gameObject.transform.rotation);
        //}

        UpdateMaskInteraction(gameObject.GetComponent<SpriteRenderer>().maskInteraction);
    }

    public bool HideHologram()
    {
        if (isHidden())
        {
            //Debug.Log("hey" + cardNum + cardSuit);
            Debug.Log(hologram != null);
            return true;
        }
        if (hologram != null)
        {
            //Debug.Log("HideHologram" + cardNum + cardSuit);
            Destroy(hologram);
            Destroy(hologramObject);
            hologram = null;
            hologramObject = null;
            return true;
        }
        return false;

    }

    public bool GlowOn()
    {
        if (!glowing)
        {
            gameObject.transform.Find("Glow").gameObject.SetActive(true);
            glowing = true;
            return true;
        }
        return false;
    }

    public bool GlowOff()
    {
        if (glowing)
        {
            gameObject.transform.Find("Glow").gameObject.SetActive(false);
            glowing = false;
            return true;
        }
        return false;
    }
}

