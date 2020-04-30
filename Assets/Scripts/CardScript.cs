using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class CardScript : MonoBehaviour
{
    public GameObject container;
    
    public int cardVal; //cardVal is what the card is worth to the reactor jack, queen, king are all 10
    public int cardNum; //cardNum is the number on the card, ace is 1 jack is 11 queen is 12 king is 13
    public string cardSuit;

    public GameObject hologramFood, hologram;
    public Sprite hologramFoodSprite, hologramComboSprite;
    public GameObject glow;
    public GameObject number;

    public Color originalColor;
    private Color newColor;
    void Start()
    {
        hologramFood.GetComponent<SpriteRenderer>().sprite = hologramFoodSprite;

        if (Config.config.prettyColors)
        {
            originalColor = new Color(Random.Range(0.4f, 1), Random.Range(0.4f, 1f), Random.Range(0.4f, 1f), 1);
        }
        else
        {
            originalColor = new Color(1, 1, 1, 1);
        }
    }

    public Sprite cardFrontSprite;
    public Sprite cardBackSprite;
    public bool hidden;
    public void SetVisibility(bool show)
    {
        if (show)
        {
            //Debug.Log("showing card" + cardNum + cardSuit);
            gameObject.GetComponent<BoxCollider2D>().enabled = true;
            number.SetActive(true);
            gameObject.GetComponent<SpriteRenderer>().sprite = cardFrontSprite;
            hidden = false;
        }
        else
        {
            //Debug.Log("hiding card" + cardNum + cardSuit);
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
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

    private int selectedLayer;
    public void MakeVisualOnly()
    {
        gameObject.transform.localScale = new Vector3(0.2f, 0.2f, 1);
        container = null;

        if (selectedLayer == 0)
            selectedLayer = SortingLayer.NameToID("SelectedCards");

        gameObject.GetComponent<SpriteRenderer>().sortingLayerID = selectedLayer;
        number.GetComponent<SpriteRenderer>().sortingLayerID = selectedLayer;
        hologram.GetComponent<SpriteRenderer>().sortingLayerID = selectedLayer;
        hologramFood.GetComponent<SpriteRenderer>().sortingLayerID = selectedLayer;
        gameObject.GetComponent<BoxCollider2D>().enabled = false;

        SetSelected(false);
    }

    public bool ChangeHologram(Color newColor)
    {
        SpriteRenderer hologramFoodSP = hologramFood.GetComponent<SpriteRenderer>();

        if (hologramFoodSP.color == newColor)
            return false;

        hologram.GetComponent<SpriteRenderer>().color = newColor;
        hologramFoodSP.color = newColor;

        if (newColor == Config.config.cardMatchHighlightColor)
        {
            hologramFoodSP.sprite = hologramComboSprite;
            hologramFood.transform.localScale = new Vector3(0.6f, 0.6f, 1);
            return true;
        }
        if (hologramFoodSP.sprite != hologramFoodSprite)
        {
            hologramFoodSP.sprite = hologramFoodSprite;
            hologramFood.transform.localScale = new Vector3(1.2f, 1.2f, 1);
            return false;
        }

        return false;
    }

    public Color GetGlowColor()
    {
        return glow.GetComponent<SpriteRenderer>().color;
    }


    private bool holoOn = false;
    private Coroutine holoCoroutine;
    public void ShowHologram()
    {
        if (holoOn == false)
        {
            holoOn = true;
            holoCoroutine = StartCoroutine(StartHologram());
        }
    }

    IEnumerator StartHologram()
    {
        hologram.SetActive(true);
        hologramFood.SetActive(true);
        hologram.GetComponent<Animator>().speed = Random.Range(0.6f, 1f);
        UpdateMaskInteraction(gameObject.GetComponent<SpriteRenderer>().maskInteraction);

        SpriteRenderer holoSR = hologram.GetComponent<SpriteRenderer>();
        SpriteRenderer objectSR = hologramFood.GetComponent<SpriteRenderer>();
        Color holoColor = holoSR.color;
        holoColor.a = 0;
        holoSR.color = holoColor;
        objectSR.color = holoColor;

        while (holoSR.color.a < 1)
        {
            holoSR.color = holoColor;
            objectSR.color = holoColor;
            holoColor.a += 0.05f;
            yield return new WaitForSeconds(0.05f);
        }
        
        hologram.GetComponent<Animator>().speed = 1;
    }

    public void HideHologram()
    {
        if (holoOn)
        {
            holoOn = false;

            if (holoCoroutine != null)
                StopCoroutine(holoCoroutine);

            hologram.SetActive(false);
            hologramFood.SetActive(false);
        }
    }

    public bool glowing = false;
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

    public bool IsGlowing()
    {
        return glowing;
    }

    public void MoveCard(GameObject destination, bool doLog = true, bool isAction = true, bool isCycle = false, bool isStack = false, bool showHolo = true)
    {
        bool nextCardWasHidden = false;
        if (container.CompareTag("Foundation"))
        {
            if (doLog)
            {
                FoundationScript foundationScript = container.GetComponent<FoundationScript>();
                if (foundationScript.cardList.Count > 1 && foundationScript.cardList[1].GetComponent<CardScript>().isHidden())
                    nextCardWasHidden = true;
            }
            container.GetComponent<FoundationScript>().RemoveCard(gameObject, showHolo: showHolo);
        }
        else if (container.CompareTag("Reactor"))
        {
            container.GetComponent<ReactorScript>().RemoveCard(gameObject);
        }
        else if (container.CompareTag("Wastepile"))
        {
            if (!doLog || destination.CompareTag("Deck"))
                container.GetComponent<WastepileScript>().RemoveCard(gameObject, undoingOrDeck: true, showHolo: showHolo);
            else
                container.GetComponent<WastepileScript>().RemoveCard(gameObject, showHolo: showHolo);
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
                    UndoScript.undoScript.logMove("stack", gameObject, isAction, nextCardWasHidden);
                else
                    UndoScript.undoScript.logMove("move", gameObject, isAction, nextCardWasHidden);
            }

            destination.GetComponent<FoundationScript>().AddCard(gameObject, showHolo: showHolo);
        }
        else if (destination.CompareTag("Reactor"))
        {
            if (doLog)
            {
                if (isCycle)
                    UndoScript.undoScript.logMove("cycle", gameObject, true, nextCardWasHidden);
                else
                    UndoScript.undoScript.logMove("move", gameObject, isAction, nextCardWasHidden);
            }

            destination.GetComponent<ReactorScript>().AddCard(gameObject);
        }
        else if (destination.CompareTag("Wastepile"))
        {
            if (doLog)
            {
                if (container.CompareTag("Foundation")) // for undoing a match that goes into the wastepile
                    UndoScript.undoScript.logMove("move", gameObject, isAction, nextCardWasHidden);
                else
                    UndoScript.undoScript.logMove("draw", gameObject, isAction);
            }

            destination.GetComponent<WastepileScript>().AddCard(gameObject, showHolo: showHolo);
        }
        else if (destination.CompareTag("Deck"))
        {
            if (doLog)
                UndoScript.undoScript.logMove("deckreset", gameObject, isAction);

            destination.GetComponent<DeckScript>().AddCard(gameObject);
        }
        else if (destination.CompareTag("MatchedPile"))
        {
            if (doLog)
                UndoScript.undoScript.logMove("match", gameObject, isAction, nextCardWasHidden);

            destination.GetComponent<MatchedPileScript>().AddCard(gameObject);
        }
        else if (destination.CompareTag("LoadPile"))
        {
            if (doLog)
                UndoScript.undoScript.logMove("match", gameObject, isAction, nextCardWasHidden);

            destination.GetComponent<LoadPileScript>().AddCard(gameObject);
        }

        container = destination;
    }
}

