using System.Collections;
using UnityEngine;

public class CardScript : MonoBehaviour, IGlow
{
    [SerializeField]
    private Sprite cardFrontSprite, cardBackSprite, hologramFoodSprite, hologramComboSprite;
    [SerializeField]
    private GameObject values, suitObject, rankObject, glow, hologram, hologramFood;

    [SerializeField]
    private byte cardRank; // the number on the card, ace is 1 jack is 11 queen is 12 king is 13
    [SerializeField]
    private byte cardSuitIndex;
    [SerializeField]
    private byte cardID;
    [SerializeField]
    private byte cardReactorValue; // what the card is worth to the reactor jack, queen, king are all 10

    [SerializeField]
    private bool _enabled, _hidden, _hitBox, _obstructed, _dragging;
    [SerializeField]
    private bool _hologram;
    [SerializeField]
    private byte _hologramColorLevel;
    [SerializeField]
    private bool _glowing;
    [SerializeField]
    private byte _glowLevel;

    private Coroutine holoCoroutine;
    private Color originalColor;
    private GameObject container;

    public byte CardRank
    {
        get => cardRank;
    }

    public byte CardSuitIndex
    {
        get => cardSuitIndex;
    }

    public byte CardID
    {
        get => cardID;
    }

    public byte CardReactorValue
    {
        get => cardReactorValue;
    }

    public GameObject Container
    {
        get => container;
    }

    public GameObject Values
    {
        get => values;
    }

    public GameObject HologramFood
    {
        get => hologramFood;
    }

    /// <summary>
    /// The state of the card's existence.
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value) return;
            _enabled = value;
            gameObject.GetComponent<SpriteRenderer>().enabled = value;
            values.SetActive(value);
            if (!value)
            {
                HitBox = false;
                Hologram = false;
            }
        }
    }

    /// <summary>
    /// The state of the card's visibility.
    /// </summary>
    public bool Hidden
    {
        get => _hidden;
        set
        {
            if (value == _hidden) return;
            _hidden = value;
            if (value)
            {
                gameObject.GetComponent<SpriteRenderer>().sprite = cardBackSprite;
                HitBox = false;
            }
            else
            {
                gameObject.GetComponent<SpriteRenderer>().sprite = cardFrontSprite;
            }
            values.SetActive(!value);
        }
    }

    /// <summary>
    /// The state of the box collider for the card.
    /// </summary>
    public bool HitBox
    {
        get => _hitBox;
        set
        {
            if (_hitBox == value) return;
            _hitBox = value;
            this.gameObject.GetComponent<BoxCollider2D>().enabled = value;
        }
    }

    /// <summary>
    /// The state of both the presentation and hitbox of the card.
    /// </summary>
    public bool Obstructed
    {
        get => _obstructed;
        set
        {
            HitBox = !value;
            if (_obstructed == value) return;
            _obstructed = value;
            if (value)
            {
                SetColor(Config.GameValues.cardObstructedColor);
            }
            else
            {
                SetColor(originalColor);
            }
        }
    }

    /// <summary>
    /// The cards drag state.
    /// </summary>
    public bool Dragging
    {
        get => _dragging;
        set
        {
            if (_dragging == value) return;

            _dragging = value;
            if (value)
            {
                Color newColor = originalColor;
                newColor.a = Config.GameValues.selectedCardOpacity;

                foreach (Renderer renderers in this.gameObject.GetComponentsInChildren<Renderer>())
                {
                    renderers.material.color = newColor;
                }
            }
            else
            {
                foreach (Renderer renderers in this.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true))
                {
                    renderers.material.color = originalColor;
                }
            }

            if (Enabled)
            {
                HitBox = !value;
            }
        }
    }

    /// <summary>
    /// The state of the card's hologram.
    /// </summary>
    public bool Hologram
    {
        get => _hologram;
        set
        {
            if (_hologram == value) return;
            if (value && !_hologram)
            {
                holoCoroutine = StartCoroutine(StartHologram());
            }
            else if (!value && _hologram && holoCoroutine != null)
            {
                StopCoroutine(holoCoroutine);
                hologram.SetActive(false);
                hologramFood.SetActive(false);
            }

            _hologram = value;
        }
    }

    /// <summary>
    /// The color level (translates to rgb color) of the card's hologram.
    /// </summary>
    public byte HologramColorLevel
    {
        get => _hologramColorLevel;
        set
        {
            if (_hologramColorLevel == value) return;

            hologram.GetComponent<SpriteRenderer>().color = Config.GameValues.highlightColors[value];

            SpriteRenderer hologramFoodSP = hologramFood.GetComponent<SpriteRenderer>();
            hologramFoodSP.color = Config.GameValues.highlightColors[value];

            if (value == Constants.matchHighlightColorLevel)
            {
                hologramFoodSP.sprite = hologramComboSprite;
            }
            else if (_hologramColorLevel == Constants.matchHighlightColorLevel)
            {
                hologramFoodSP.sprite = hologramFoodSprite;
            }

            // do this at the end
            _hologramColorLevel = value;
        }
    }

    /// <summary>
    /// The glow state of the card.
    /// </summary>
    public bool Glowing
    {
        get => _glowing;
        set
        {
            if (_glowing == value) return;
            _glowing = value;
            glow.SetActive(value);
        }
    }

    /// <summary>
    /// The color level (translates to rgb color) of the card's glow.
    /// </summary>
    public byte GlowLevel
    {
        get => _glowLevel;
        set
        {
            Glowing = true;
            if (_glowLevel == value) return;
            if (value != _glowLevel)
            {
                _glowLevel = value;
                glow.GetComponent<SpriteRenderer>().color = Config.GameValues.highlightColors[value];
            }
        }
    }

    public void SetUp(byte rank, byte suitIndex, Sprite suitSprite, Sprite hologramFoodSprite, Sprite hologramComboSprite)
    {
        cardRank = rank;
        cardSuitIndex = suitIndex;

        cardID = (byte)(rank + suitIndex * 13);
        this.name = $"{Constants.suits[cardSuitIndex]}_{cardRank}";

        // reactor values, all face cards have a value of 10
        cardReactorValue = (byte)(rank < 10 ? rank : 10);

        hologramFood.GetComponent<SpriteRenderer>().sprite = hologramFoodSprite;
        this.hologramFoodSprite = hologramFoodSprite;
        this.hologramComboSprite = hologramComboSprite;

        // in-game appearance of the card's rank
        TextMesh rankText = rankObject.GetComponent<TextMesh>();
        rankText.color = suitIndex > 1 ? Color.red : Color.black;
        rankText.text = rank switch
        {
            1 => "A",
            11 => "J",
            12 => "Q",
            13 => "K",
            _ => rank.ToString()
        };

        // setting up the in-game appearance of the card's suit
        suitObject.GetComponent<SpriteRenderer>().sprite = suitSprite;

        if (Config.Instance.prettyColors)
        {
            originalColor = new Color(Random.Range(0.4f, 1), Random.Range(0.4f, 1f), Random.Range(0.4f, 1f), 1);
            // TODO: this needs to change
            SetColor(originalColor);
        }
        else
        {
            originalColor = Color.white;
        }

        // initialized default values
        _enabled = true;
        _hidden = false;
        _hitBox = false;
        _obstructed = false;
        _hologram = false;
        _dragging = false;
        _glowing = false;
        _glowLevel = 0;
        _hologramColorLevel = 0;
    }

    public void SetValuesToDefault()
    {
        Enabled = true;
        Hidden = false;
        Hologram = false;
        Dragging = false;
        Glowing = false;

        // order matters here
        Obstructed = false;
        HitBox = false;
    }

    /// <summary>
    /// Reveals the card without setting the flag that it is hidden (for undo purposes).
    /// </summary>
    public void NextCycleReveal()
    {
        Hidden = false;
        _hidden = true;
    }

    /// <summary>
    /// Sets the cards color, doesn't include hologram, glow, and values.
    /// </summary>
    public void SetColor(Color setTo)
    {
        this.gameObject.GetComponent<SpriteRenderer>().color = setTo;
    }

    /// <summary>
    /// For the cloned cards that are being dragged.
    /// </summary>
    public void MakeVisualOnly()
    {
        gameObject.transform.localScale = new Vector3(Config.GameValues.draggedCardScale, Config.GameValues.draggedCardScale, 1);
        container = null;

        values.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingLayerID = UtilsScript.Instance.SelectedCardsLayer;

        foreach (Renderer renderers in this.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true))
        {
            renderers.sortingLayerID = UtilsScript.Instance.SelectedCardsLayer;
            // TODO : A Unity bug requires this
            renderers.material.color = renderers.material.color;
        }

        // when picking up a card that has a hologram that's just starting up
        // make the dragged card's hologram have full alpha
        SpriteRenderer holoSR = hologram.GetComponent<SpriteRenderer>();
        Color holoColor = holoSR.color;
        holoColor.a = 1;
        holoSR.color = holoColor;
        hologramFood.GetComponent<SpriteRenderer>().color = holoColor;

        HitBox = false;
    }

    /// <summary>
    /// Moves the card to the given destination according to the parameters given.
    /// </summary>
    /// <param name="destination">new card container</param>
    /// <param name="doLog">log for undo</param>
    /// <param name="isAction">is a player interaction</param>
    /// <param name="isCycle">is for a next cycle</param>
    /// <param name="isStack">is part of a foundation stack move</param>
    /// <param name="showHolo">show this card's hologram on final placement</param>
    public void MoveCard(GameObject destination, bool doLog = true, bool isAction = true, bool isCycle = false, bool isStack = false, bool showHolo = true)
    {
        bool nextCardWasHidden = false;
        if (Container != null)
        {
            switch (Container.tag)
            {
                case Constants.foundationTag:
                    if (doLog)
                    {
                        FoundationScript foundationScript = Container.GetComponent<FoundationScript>();
                        if (foundationScript.CardList.Count > 1 && foundationScript.CardList[^2].GetComponent<CardScript>().Hidden)
                        {
                            nextCardWasHidden = true;
                        }
                    }
                    Container.GetComponent<FoundationScript>().RemoveCard(gameObject, showHolo: showHolo);
                    break;
                case Constants.reactorTag:
                    Container.GetComponent<ReactorScript>().RemoveCard(gameObject);
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
                default:
                    throw new System.Exception($"the card conatainer of {Container} is not supported");
            }
        }

        switch (destination.tag)
        {
            case Constants.foundationTag:
                if (doLog)
                {
                    if (isStack)
                    {
                        UndoScript.Instance.LogMove(gameObject, Constants.stackLogMove, isAction, nextCardWasHidden);
                    }
                    else
                    {
                        UndoScript.Instance.LogMove(gameObject, Constants.moveLogMove, isAction, nextCardWasHidden);
                    }
                }
                destination.GetComponent<FoundationScript>().AddCard(gameObject, showHolo: showHolo);
                break;
            case Constants.reactorTag:
                if (doLog)
                {
                    if (isCycle)
                    {
                        UndoScript.Instance.LogMove(gameObject, Constants.cycleLogMove, true, nextCardWasHidden);
                    }
                    else
                    {
                        UndoScript.Instance.LogMove(gameObject, Constants.moveLogMove, isAction, nextCardWasHidden);
                    }
                }
                destination.GetComponent<ReactorScript>().AddCard(gameObject);
                break;
            case Constants.wastepileTag:
                if (doLog)
                {
                    // for undoing a match that goes into the wastepile
                    if (Container.CompareTag(Constants.foundationTag))
                    {
                        UndoScript.Instance.LogMove(gameObject, Constants.moveLogMove, isAction, nextCardWasHidden);
                    }
                    else
                    {
                        UndoScript.Instance.LogMove(gameObject, Constants.drawLogMove, isAction);
                    }
                }

                WastepileScript.Instance.AddCard(gameObject, showHolo: showHolo);
                break;
            case Constants.deckTag:
                if (doLog)
                {
                    UndoScript.Instance.LogMove(gameObject, Constants.deckresetLogMove, isAction);
                }
                DeckScript.Instance.AddCard(gameObject);
                break;
            case Constants.matchedPileTag:
                if (doLog)
                {
                    UndoScript.Instance.LogMove(gameObject, Constants.matchLogMove, isAction, nextCardWasHidden);
                }
                MatchedPileScript.Instance.AddCard(gameObject);
                break;
            case Constants.loadPileTag:
                if (doLog)
                {
                    UndoScript.Instance.LogMove(gameObject, Constants.matchLogMove, isAction, nextCardWasHidden);
                }
                LoadPileScript.Instance.AddCard(gameObject);
                break;
            default:
                throw new System.Exception($"the card destination of {destination} is not supported");
        }
        container = destination;
    }

    /// <summary>
    /// Starts the card's hologram at a random frame and fades it in.
    /// </summary>
    private IEnumerator StartHologram()
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
        yield return null;

        while (holoColor.a < 1)
        {
            holoColor.a += Time.deltaTime * 0.5f;
            holoSR.color = holoColor;
            objectSR.color = holoColor;
            yield return null;
        }
    }
}
