using System.Collections;
using UnityEngine;

public class CardScript : MonoBehaviour, IGlow
{
    public GameObject container;
    
    public int cardVal; //cardVal is what the card is worth to the reactor jack, queen, king are all 10
    public int cardNum; //cardNum is the number on the card, ace is 1 jack is 11 queen is 12 king is 13
    public string suit;

    public Sprite cardFrontSprite;
    public Sprite cardBackSprite;

    public GameObject values;
    public GameObject suitObject, rankObject;
    public GameObject hologramFood, hologram;
    public Sprite hologramFoodSprite, hologramComboSprite;
    public GameObject glow;

    public Color originalColor;

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

    private bool isHidden;
    public bool IsHidden
    {
        get { return isHidden; }
    }

    public void SetCollider(bool enable, bool tutorialOverride = false, bool visualOnly = false)
    {
        if (!Config.Instance.tutorialOn || tutorialOverride || visualOnly)
        {
            this.gameObject.GetComponent<BoxCollider2D>().enabled = enable;
        }
    }

    public void SetObstructed(bool obstructed, bool showHologram = true)
    {
        SetCollider(!obstructed);
        if (obstructed)
        {
            HideHologram();
            SetColor(Config.GameValues.cardObstructedColor);
        }
        else
        {
            if (showHologram)
            {
                ShowHologram();
            }
            SetColor();
        }
    }

    public void SetColor()
    {
        SetColor(originalColor);
    }

    public void SetColor(Color setTo)
    {
        this.gameObject.GetComponent<SpriteRenderer>().color = setTo;
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

        UpdateCardVisibility(show);
    }

    public void SetEnabled(bool enabled)
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = enabled;
        UpdateCardVisibility(enabled);
    }

    private void UpdateCardVisibility(bool show)
    {
        SetCollider(show);
        values.SetActive(show);

        if (!show)
        {
            HideHologram();
        }
    }

    public void SetDragging(bool dragging)
    {
        if (dragging)
        {
            Color newColor = originalColor;
            newColor.a = Config.GameValues.selectedCardOpacity;

            foreach (Renderer renderers in this.gameObject.GetComponentsInChildren<Renderer>())
            {
                renderers.material.color = newColor;
            }
            SetCollider(false);
        }
        else
        {
            foreach (Renderer renderers in this.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                renderers.material.color = originalColor;
            }
            SetCollider(true);
        }
    }

    public void MakeVisualOnly()
    {
        gameObject.transform.localScale = new Vector3(Config.GameValues.draggedCardScale, Config.GameValues.draggedCardScale, 1);
        container = null;

        values.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingLayerID = UtilsScript.Instance.SelectedCardsLayer;

        foreach (Renderer renderers in this.gameObject.GetComponentsInChildren<Renderer>())
        {
            renderers.sortingLayerID = UtilsScript.Instance.SelectedCardsLayer;
            // TODO : Unity bug requires this
            renderers.material.color = renderers.material.color;
        }

        SetCollider(false, visualOnly : true);

        // this shouldn't be needed but for some reason it is
        // without it, after dragging a card once any clone will match its parents color
        //SetDragging(false);
    }

    private byte currentHologramColorLevel;
    public void ChangeHologramColorLevel(byte level)
    {
        if (currentHologramColorLevel == level) return;

        hologram.GetComponent<SpriteRenderer>().color = Config.GameValues.highlightColors[level];

        SpriteRenderer hologramFoodSP = hologramFood.GetComponent<SpriteRenderer>();
        hologramFoodSP.color = Config.GameValues.highlightColors[level];

        if (level == 1) // match
        {
            hologramFoodSP.sprite = hologramComboSprite;
            hologramFood.transform.localScale = new Vector3(0.6f, 0.6f, 1);
        }
        else if (currentHologramColorLevel == 1) // no more match
        {
            hologramFoodSP.sprite = hologramFoodSprite;
            hologramFood.transform.localScale = new Vector3(1.2f, 1.2f, 1);
        }

        currentHologramColorLevel = level;
    }

    private bool hologramOn = false;
    private Coroutine holoCoroutine;
    public void ShowHologram()
    {
        if (hologramOn == false)
        {
            hologramOn = true;
            holoCoroutine = StartCoroutine(StartHologram());
        }
    }

    IEnumerator StartHologram()
    {
        hologram.SetActive(true);
        hologramFood.SetActive(true);
        
        // start the animation at a random frame
        hologram.GetComponent<Animator>().Play(0, -1, Random.Range(0.0f, 1.0f));

        SpriteRenderer holoSR = hologram.GetComponent<SpriteRenderer>();
        SpriteRenderer objectSR = hologramFood.GetComponent<SpriteRenderer>();
        Color holoColor = holoSR.color;
        holoColor.a = 0;
        holoSR.color = holoColor;
        objectSR.color = holoColor;

        while (holoColor.a < 1)
        {
            yield return new WaitForSeconds(0.05f);
            holoColor.a += 0.05f;
            holoSR.color = holoColor;
            objectSR.color = holoColor;
        }
    }

    public void HideHologram()
    {
        if (hologramOn)
        {
            hologramOn = false;

            if (holoCoroutine != null)
            {
                StopCoroutine(holoCoroutine);
            }

            hologram.SetActive(false);
            hologramFood.SetActive(false);
        }
    }

    private bool _glowing;
    public bool Glowing
    {
        get { return _glowing; }
        set
        {
            if (value && !_glowing)
            {
                _glowing = true;
                glow.SetActive(true);
            }
            else if (!value && _glowing)
            {
                _glowing = false;
                glow.SetActive(false);
            }
        }
    }

    private byte _glowLevel;
    public byte GlowLevel
    {
        get { return _glowLevel; }
        set
        {
            if (value != _glowLevel)
            {
                _glowLevel = value;
                glow.GetComponent<SpriteRenderer>().color = Config.GameValues.highlightColors[value];
            }

            Glowing = true;
        }
    }

    public void MoveCard(GameObject destination, bool doLog = true, bool isAction = true, bool isCycle = false, bool isStack = false, bool showHolo = true)
    {
        bool nextCardWasHidden = false;
        if (container != null)
        {
            switch (container.tag)
            {
                case Constants.foundationTag:
                    if (doLog)
                    {
                        FoundationScript foundationScript = container.GetComponent<FoundationScript>();
                        if (foundationScript.cardList.Count > 1 && foundationScript.cardList[1].GetComponent<CardScript>().IsHidden)
                        {
                            nextCardWasHidden = true;
                        }
                    }
                    container.GetComponent<FoundationScript>().RemoveCard(gameObject, showHolo: showHolo);
                    break;
                case Constants.reactorTag:
                    container.GetComponent<ReactorScript>().RemoveCard(gameObject);
                    break;
                case Constants.wastepileTag:
                    if (!doLog || destination.CompareTag(Constants.deckTag))
                    {
                        WastepileScript.Instance.RemoveCard(gameObject, undoingOrDeck: true, showHolo: showHolo);
                    }
                    else
                    {
                        WastepileScript.Instance.RemoveCard(gameObject, showHolo: showHolo);
                    }
                    break;
                case Constants.deckTag:
                    DeckScript.Instance.RemoveCard(gameObject);
                    break;
                case Constants.matchedPileTag:
                    MatchedPileScript.Instance.RemoveCard(gameObject);
                    break;
                case Constants.loadPileTag:
                    LoadPileScript.Instance.RemoveCard(gameObject);
                    break;
            }
        }

        switch (destination.tag)
        {
            case Constants.foundationTag:
                if (doLog)
                {
                    if (isStack)
                    {
                        UndoScript.Instance.LogMove(Constants.stackLogMove, gameObject, isAction, nextCardWasHidden);
                    }
                    else
                    {
                        UndoScript.Instance.LogMove(Constants.moveLogMove, gameObject, isAction, nextCardWasHidden);
                    }
                }
                destination.GetComponent<FoundationScript>().AddCard(gameObject, showHolo: showHolo);
                break;
            case Constants.reactorTag:
                if (doLog)
                {
                    if (isCycle)
                    {
                        UndoScript.Instance.LogMove(Constants.cycleLogMove, gameObject, true, nextCardWasHidden);
                    }
                    else
                    {
                        UndoScript.Instance.LogMove(Constants.moveLogMove, gameObject, isAction, nextCardWasHidden);
                    }
                }
                destination.GetComponent<ReactorScript>().AddCard(gameObject);
                break;
            case Constants.wastepileTag:
                if (doLog)
                {
                    // for undoing a match that goes into the wastepile
                    if (container.CompareTag(Constants.foundationTag))
                    {
                        UndoScript.Instance.LogMove(Constants.moveLogMove, gameObject, isAction, nextCardWasHidden);
                    }
                    else
                    {
                        UndoScript.Instance.LogMove(Constants.drawLogMove, gameObject, isAction);
                    }
                }

                WastepileScript.Instance.AddCard(gameObject, showHolo: showHolo);
                break;
            case Constants.deckTag:
                if (doLog)
                {
                    UndoScript.Instance.LogMove(Constants.deckresetLogMove, gameObject, isAction);
                }
                DeckScript.Instance.AddCard(gameObject);
                break;
            case Constants.matchedPileTag:
                if (doLog)
                {
                    UndoScript.Instance.LogMove(Constants.matchLogMove, gameObject, isAction, nextCardWasHidden);
                }
                MatchedPileScript.Instance.AddCard(gameObject);
                break;
            case Constants.loadPileTag:
                if (doLog)
                {
                    UndoScript.Instance.LogMove(Constants.matchLogMove, gameObject, isAction, nextCardWasHidden);
                }
                LoadPileScript.Instance.AddCard(gameObject);
                break;
        }

        container = destination;
    }
}
