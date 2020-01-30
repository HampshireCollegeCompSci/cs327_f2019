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
    public Color originalColor;
    public Color newColor;
    public bool glowing;

    public GameObject hologramFood, hologram;
    public GameObject glow;
    public GameObject number;

    private int selectedLayer;

    void Start()
    {
        glowing = false;

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
            number.SetActive(true);
            gameObject.GetComponent<SpriteRenderer>().sprite = cardFrontSprite;
            hidden = false;
        }
        else
        {
            //Debug.Log("hiding card" + cardNum + cardSuit);
            number.SetActive(false);
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
        if (selected)
        {
            newColor = gameObject.GetComponent<SpriteRenderer>().material.color;
            newColor.a = Config.config.selectedCardOpacity;
            gameObject.GetComponent<SpriteRenderer>().material.color = newColor;
            number.GetComponent<SpriteRenderer>().material.color = newColor;
            hologram.GetComponent<SpriteRenderer>().color = newColor;
            hologramFood.GetComponent<SpriteRenderer>().color = newColor;
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().material.color = originalColor;
            number.GetComponent<SpriteRenderer>().material.color = originalColor;
            hologram.GetComponent<SpriteRenderer>().color = originalColor;
            hologramFood.GetComponent<SpriteRenderer>().color = originalColor;
        }
    }

    public void UpdateMaskInteraction(SpriteMaskInteraction update)
    {
        gameObject.GetComponent<SpriteRenderer>().maskInteraction = update;
        number.GetComponent<SpriteRenderer>().maskInteraction = update;
        if (hologram != null && hologramFood != null)
        {
            hologram.GetComponent<SpriteRenderer>().maskInteraction = update;
            hologramFood.GetComponent<SpriteRenderer>().maskInteraction = update;
        }
    }

    public void MakeVisualOnly()
    {
        gameObject.transform.localScale = new Vector3(0.2f, 0.2f, 1);
        container = null;
        
        if (selectedLayer == 0)
        {
            selectedLayer = SortingLayer.NameToID("SelectedCards");
        }

        gameObject.GetComponent<SpriteRenderer>().sortingLayerID = selectedLayer;
        number.GetComponent<SpriteRenderer>().sortingLayerID = selectedLayer;
        hologram.GetComponent<SpriteRenderer>().sortingLayerID = selectedLayer;
        hologramFood.GetComponent<SpriteRenderer>().sortingLayerID = selectedLayer;
        Destroy(gameObject.GetComponent<BoxCollider2D>());
    }

    public void ShowHologram()
    {
        hologram.SetActive(true);
        hologramFood.SetActive(true);
        hologram.GetComponent<Animator>().speed = Random.Range(0.6f, 1f);
        UpdateMaskInteraction(gameObject.GetComponent<SpriteRenderer>().maskInteraction);
        StartCoroutine(ResetHologramSpeed());
    }

    IEnumerator ResetHologramSpeed()
    {
        yield return new WaitForSeconds(1);
        hologram.GetComponent<Animator>().speed = 1;
    }

    public bool HideHologram()
    {
        if (isHidden())
        {
            return true;
        }

        if (hologram.activeSelf)
        {
            hologram.SetActive(false);
            hologramFood.SetActive(false);
            return true;
        }

        return false;
    }

    public bool GlowOn(bool match)
    {
        if (!glowing)
        {
            glow.SetActive(true);
            if (match)
            {
                glow.GetComponent<SpriteRenderer>().color = Config.config.cardMatchHighlightColor;
            }
            else
            {
                glow.GetComponent<SpriteRenderer>().color = Config.config.cardMoveHighlightColor;
            }

            glowing = true;
            return true;
        }
        return false;
    }

    public bool GlowOff()
    {
        if (glowing)
        {
            glow.gameObject.SetActive(false);
            glowing = false;
            return true;
        }
        return false;
    }

    public void MoveCard(GameObject destination, bool doLog = true, bool isAction = true, bool isCycle = false, bool isStack = false, bool removeUpdateHolo = true, bool addUpdateHolo = true)
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
        else if (container.CompareTag("LoadPile"))
        {
            container.GetComponent<LoadPileScript>().RemoveCard(gameObject);
        }

        if (destination.CompareTag("Foundation"))
        {
            if (doLog)
            {
                if (isStack)
                {
                    UndoScript.undoScript.logMove("stack", gameObject, isAction, Config.config.actions, nextCardWasHidden);
                }
                else
                {
                    UndoScript.undoScript.logMove("move", gameObject, isAction, Config.config.actions, nextCardWasHidden);
                }
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
                if (container.CompareTag("Foundation")) // for undoing a match that goes into the wastepile
                {
                    UndoScript.undoScript.logMove("move", gameObject, isAction, Config.config.actions, nextCardWasHidden);
                }
                else
                {
                    UndoScript.undoScript.logMove("draw", gameObject, isAction, Config.config.actions);
                }
            }

            destination.GetComponent<WastepileScript>().AddCard(gameObject, addUpdateHolo);
        }
        else if (destination.CompareTag("Deck"))
        {
            if (doLog)
            {
                UndoScript.undoScript.logMove("deckreset", gameObject, isAction, Config.config.actions);
            }
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
        else if (destination.CompareTag("LoadPile"))
        {
            if (doLog)
            {
                UndoScript.undoScript.logMove("match", gameObject, isAction, Config.config.actions, nextCardWasHidden);
            }

            destination.GetComponent<LoadPileScript>().AddCard(gameObject);
        }

        container = destination;
        StateLoader.saveSystem.writeState();
    }
}

