using System.Collections;
using UnityEngine;

public class CardScript : MonoBehaviour
{
    public GameObject container;
    
    public int cardVal; //cardVal is what the card is worth to the reactor jack, queen, king are all 10
    public int cardNum; //cardNum is the number on the card, ace is 1 jack is 11 queen is 12 king is 13
    public string suit;

    public GameObject values;
    public GameObject suitObject, rankObject;
    public GameObject hologramFood, hologram;
    public Sprite hologramFoodSprite, hologramComboSprite;
    public GameObject glow;

    public Color originalColor;
    private Color newColor;
    void Start()
    {
        hologramFood.GetComponent<SpriteRenderer>().sprite = hologramFoodSprite;

        if (Config.Instance.prettyColors)
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
    private bool isHidden;
    public bool IsHidden
    {
        get { return isHidden; }
    }

    public void SetFoundationVisibility(bool show, bool isNotForNextCycle = true)
    {
        if (show)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = cardFrontSprite;

            // for undo to work right:
            // during a Next Cycle we manually reveal the card before it's automatically done
            // this could cause the isHidden flag to be set to false before undo records it, when in reality it should've been true
            if (isNotForNextCycle)
            {
                isHidden = false;
            }
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = cardBackSprite;
            isHidden = true;
        }

        UpdateTokenVisibility(show);
    }

    public void SetGameplayVisibility(bool show)
    {
        if (show)
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }

        UpdateTokenVisibility(show);
    }

    private void UpdateTokenVisibility(bool show)
    {
        if (show)
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = true;
            values.SetActive(true);
        }
        else
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            values.SetActive(false);
            HideHologram();
        }
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            newColor = gameObject.GetComponent<SpriteRenderer>().material.color;
            newColor.a = Config.GameValues.selectedCardOpacity;
            gameObject.GetComponent<SpriteRenderer>().material.color = newColor;
            suitObject.GetComponent<SpriteRenderer>().material.color = newColor;

            Color rankColor = rankObject.GetComponent<TextMesh>().color;
            rankColor.a = Config.GameValues.selectedCardOpacity;
            rankObject.GetComponent<TextMesh>().color = rankColor;

            hologram.GetComponent<SpriteRenderer>().color = newColor;
            hologramFood.GetComponent<SpriteRenderer>().color = newColor;
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().material.color = originalColor;
            suitObject.GetComponent<SpriteRenderer>().material.color = originalColor;

            Color rankColor = rankObject.GetComponent<TextMesh>().color;
            rankColor.a = 1;
            rankObject.GetComponent<TextMesh>().color = rankColor;

            hologram.GetComponent<SpriteRenderer>().color = originalColor;
            hologramFood.GetComponent<SpriteRenderer>().color = originalColor;
        }
    }

    public void MakeVisualOnly()
    {
        gameObject.transform.localScale = new Vector3(Config.GameValues.draggedCardScale, Config.GameValues.draggedCardScale, 1);
        container = null;

        gameObject.GetComponent<SpriteRenderer>().sortingLayerID = UtilsScript.Instance.SelectedCardsLayer;
        values.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingLayerID = UtilsScript.Instance.SelectedCardsLayer;
        hologram.GetComponent<SpriteRenderer>().sortingLayerID = UtilsScript.Instance.SelectedCardsLayer;
        hologramFood.GetComponent<SpriteRenderer>().sortingLayerID = UtilsScript.Instance.SelectedCardsLayer;
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

        if (newColor == Config.GameValues.cardMatchHighlightColor)
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
                glow.GetComponent<SpriteRenderer>().color = Config.GameValues.cardMatchHighlightColor;
            }
            else
            {
                glow.GetComponent<SpriteRenderer>().color = Config.GameValues.cardMoveHighlightColor;
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
        if (container != null)
        {
            if (container.CompareTag("Foundation"))
            {
                if (doLog)
                {
                    FoundationScript foundationScript = container.GetComponent<FoundationScript>();
                    if (foundationScript.cardList.Count > 1 && foundationScript.cardList[1].GetComponent<CardScript>().IsHidden)
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
        }

        if (destination.CompareTag("Foundation"))
        {
            if (doLog)
            {
                if (isStack)
                    UndoScript.Instance.LogMove("stack", gameObject, isAction, nextCardWasHidden);
                else
                    UndoScript.Instance.LogMove("move", gameObject, isAction, nextCardWasHidden);
            }

            destination.GetComponent<FoundationScript>().AddCard(gameObject, showHolo: showHolo);
        }
        else if (destination.CompareTag("Reactor"))
        {
            if (doLog)
            {
                if (isCycle)
                    UndoScript.Instance.LogMove("cycle", gameObject, true, nextCardWasHidden);
                else
                    UndoScript.Instance.LogMove("move", gameObject, isAction, nextCardWasHidden);
            }

            destination.GetComponent<ReactorScript>().AddCard(gameObject);
        }
        else if (destination.CompareTag("Wastepile"))
        {
            if (doLog)
            {
                if (container.CompareTag("Foundation")) // for undoing a match that goes into the wastepile
                    UndoScript.Instance.LogMove("move", gameObject, isAction, nextCardWasHidden);
                else
                    UndoScript.Instance.LogMove("draw", gameObject, isAction);
            }

            destination.GetComponent<WastepileScript>().AddCard(gameObject, showHolo: showHolo);
        }
        else if (destination.CompareTag("Deck"))
        {
            if (doLog)
                UndoScript.Instance.LogMove("deckreset", gameObject, isAction);

            destination.GetComponent<DeckScript>().AddCard(gameObject);
        }
        else if (destination.CompareTag("MatchedPile"))
        {
            if (doLog)
                UndoScript.Instance.LogMove("match", gameObject, isAction, nextCardWasHidden);

            destination.GetComponent<MatchedPileScript>().AddCard(gameObject);
        }
        else if (destination.CompareTag("LoadPile"))
        {
            if (doLog)
                UndoScript.Instance.LogMove("match", gameObject, isAction, nextCardWasHidden);

            destination.GetComponent<LoadPileScript>().AddCard(gameObject);
        }

        container = destination;
    }
}

