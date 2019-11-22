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
    public bool glowOn = false;

    private GameObject hologramObject;
    private GameObject hologram;

    Vector3 originalTransform;

    void Start()
    {
        //originalTransform = Config.config.cardScale * .1f;
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
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = cardFrontSprite;
        }

        //makes card larger and first in sorting order if the card is selected
        if (appearSelected)
        {
            //gameObject.transform.localScale = originalTransform * 1.1f;
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
            //gameObject.transform.localScale = originalTransform;
            gameObject.GetComponent<SpriteRenderer>().material.color = originalColor;
            if (hologram != null && hologramObject != null)
            {
                hologram.GetComponent<SpriteRenderer>().color = originalColor;
                hologramObject.GetComponent<SpriteRenderer>().color = originalColor;
            }
        }

        if (glowOn)
        {
            gameObject.transform.Find("Glow").gameObject.SetActive(true);
        }

        else
        {
            gameObject.transform.Find("Glow").gameObject.SetActive(false);
        }

        //set collider scale to match sprite scale
        //gameObject.GetComponent<BoxCollider2D>().size = gameObject.GetComponent<SpriteRenderer>().sprite.bounds.size;
        //gameObject.GetComponent<BoxCollider2D>().offset = gameObject.GetComponent<SpriteRenderer>().sprite.bounds.center;
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

    public void MoveCard(GameObject destination, bool doLog = true, bool isAction = true, bool isCycle = false, bool updateHolograms = true)
    {
        if (destination.CompareTag("Foundation"))
        {
            if (doLog)
            {
                if (isAction)
                {
                    UndoScript.undoScript.logMove("move", gameObject, actionsRemaining: Config.config.actions);
                }
                else
                {
                    UndoScript.undoScript.logMove("move", gameObject, false, actionsRemaining: Config.config.actions);
                }
            }
            container.SendMessage("RemoveCard", gameObject);
            destination.GetComponent<FoundationScript>().AddCard(gameObject);
        }

        else if (destination.CompareTag("Reactor"))
        {
            if (doLog)
            {
                if (isAction && !isCycle)
                {
                    UndoScript.undoScript.logMove("move", gameObject, actionsRemaining: Config.config.actions);
                }
                else if (isCycle)
                {
                    UndoScript.undoScript.logMove("cycle", gameObject, actionsRemaining: Config.config.actions);
                }
                else
                {
                    UndoScript.undoScript.logMove("move", gameObject, false, actionsRemaining: Config.config.actions);
                }
            }
            container.SendMessage("RemoveCard", gameObject);
            destination.GetComponent<ReactorScript>().AddCard(gameObject);
        }

        else if (destination.CompareTag("Wastepile"))
        {
            if (doLog)
            {
                if (isAction)
                {
                    UndoScript.undoScript.logMove("draw", gameObject, actionsRemaining: Config.config.actions);
                }
                else
                {
                    UndoScript.undoScript.logMove("draw", gameObject, false, actionsRemaining: Config.config.actions);
                }
            }
            container.SendMessage("RemoveCard", gameObject);
            destination.GetComponent<WastepileScript>().AddCard(gameObject);
        }
        else if (destination.CompareTag("Deck"))
        {
            container.SendMessage("RemoveCard", gameObject);
            destination.GetComponent<DeckScript>().AddCard(gameObject);
        }
        else if (destination.CompareTag("MatchedPile"))
        {
            if (doLog)
            {
                if (isAction)
                {
                    UndoScript.undoScript.logMove("match", gameObject, actionsRemaining: Config.config.actions);
                }
                else
                {
                    UndoScript.undoScript.logMove("match", gameObject, false, actionsRemaining: Config.config.actions);
                }
            }
            container.SendMessage("RemoveCard", gameObject);
            destination.GetComponent<MatchedPileScript>().AddCard(gameObject);
        }

        container.SendMessage("SetCardPositions");
        container = destination;
        destination.SendMessage("SetCardPositions");
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
            return;
        }

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
        //Debug.Log("Try HideHologram");
        if (hologram != null)
        {
            //Debug.Log("HideHologram");
            Destroy(hologram);
            Destroy(hologramObject);
            return true;
        }
        return false;

    }

    public bool GlowOn()
    {
        if (!glowOn)
        {
            glowOn = true;
            SetCardAppearance();
            return true;
        }
        return false;
    }

    public bool GlowOff()
    {
        if (glowOn)
        {
            glowOn = false;
            SetCardAppearance();
        }
        return false;
    }
}

