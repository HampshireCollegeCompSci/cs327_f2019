using System.Collections;
using UnityEngine;

public class CardScript : MonoBehaviour, IGlow
{
    [SerializeField]
    private Sprite cardFrontSprite, cardBackSprite, hologramFoodSprite, hologramComboSprite;
    [SerializeField]
    private GameObject values, suitObject, rankObject, glow, hologram, hologramFood;

    [SerializeField]
    private byte _cardID;

    [SerializeField]
    private bool _enabled, _hidden, _hitBox, _obstructed, _dragging;
    [SerializeField]
    private bool _hologram;
    [SerializeField]
    private int _hologramColorLevel;
    [SerializeField]
    private bool _glowing;
    [SerializeField]
    private int _glowLevel;

    private Coroutine holoCoroutine;
    private Color originalColor;
    private GameObject container;

    [SerializeField]
    private Card _card;
    public Card Card => _card;

    public byte CardID => _cardID;

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
    public int HologramColorLevel
    {
        get => _hologramColorLevel;
        set
        {
            if (_hologramColorLevel == value) return;

            hologram.GetComponent<SpriteRenderer>().color = Config.GameValues.highlightColors[value];

            SpriteRenderer hologramFoodSP = hologramFood.GetComponent<SpriteRenderer>();
            hologramFoodSP.color = Config.GameValues.highlightColors[value];

            if (value == Constants.HighlightColorLevel.match)
            {
                hologramFoodSP.sprite = hologramComboSprite;
            }
            else if (_hologramColorLevel == Constants.HighlightColorLevel.match)
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
    public int GlowLevel
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

    public void SetUp(Card card, Sprite suitSprite, Sprite hologramFoodSprite, Sprite hologramComboSprite)
    {
        _card = card;

        _cardID = (byte) (card.Rank.Value + card.Suit.Index * 13);
        this.name = $"{card.Suit.Name}_{card.Rank.Value}";

        hologramFood.GetComponent<SpriteRenderer>().sprite = hologramFoodSprite;
        this.hologramFoodSprite = hologramFoodSprite;
        this.hologramComboSprite = hologramComboSprite;

        // in-game appearance of the card's rank
        TextMesh rankText = rankObject.GetComponent<TextMesh>();
        rankText.color = card.Suit.Color;
        rankText.text = card.Rank.Name;

        // setting up the in-game appearance of the card's suit
        suitObject.GetComponent<SpriteRenderer>().sprite = suitSprite;

        if (Config.Instance.prettyColors)
        {
            originalColor = new Color(Random.Range(0.4f, 1), Random.Range(0.4f, 1f), Random.Range(0.4f, 1f), 1);
                1);
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
                case Constants.Tags.foundation:
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
                case Constants.Tags.reactor:
                    Container.GetComponent<ReactorScript>().RemoveCard(gameObject);
                    break;
                case Constants.Tags.wastepile:
                    if (!doLog || destination.CompareTag(Constants.Tags.deck))
                    {
                        WastepileScript.Instance.RemoveCard(gameObject, undoingOrDeck: true, showHolo: showHolo);
                    }
                    else
                    {
                        WastepileScript.Instance.RemoveCard(gameObject, showHolo: showHolo);
                    }
                    break;
                case Constants.Tags.deck:
                    DeckScript.Instance.RemoveCard(gameObject);
                    break;
                case Constants.Tags.matchedPile:
                    MatchedPileScript.Instance.RemoveCard(gameObject);
                    break;
                case Constants.Tags.loadPile:
                    LoadPileScript.Instance.RemoveCard(gameObject);
                    break;
                default:
                    throw new System.Exception($"the card conatainer of {Container} is not supported");
            }
        }

        switch (destination.tag)
        {
            case Constants.Tags.foundation:
                if (doLog)
                {
                    if (isStack)
                    {
                        UndoScript.Instance.LogMove(gameObject, Constants.LogMoveTypes.stack, isAction, nextCardWasHidden);
                    }
                    else
                    {
                        UndoScript.Instance.LogMove(gameObject, Constants.LogMoveTypes.move, isAction, nextCardWasHidden);
                    }
                }
                destination.GetComponent<FoundationScript>().AddCard(gameObject, showHolo: showHolo);
                break;
            case Constants.Tags.reactor:
                if (doLog)
                {
                    if (isCycle)
                    {
                        UndoScript.Instance.LogMove(gameObject, Constants.LogMoveTypes.cycle, true, nextCardWasHidden);
                    }
                    else
                    {
                        UndoScript.Instance.LogMove(gameObject, Constants.LogMoveTypes.move, isAction, nextCardWasHidden);
                    }
                }
                destination.GetComponent<ReactorScript>().AddCard(gameObject);
                break;
            case Constants.Tags.wastepile:
                if (doLog)
                {
                    // for undoing a match that goes into the wastepile
                    if (Container.CompareTag(Constants.Tags.foundation))
                    {
                        UndoScript.Instance.LogMove(gameObject, Constants.LogMoveTypes.move, isAction, nextCardWasHidden);
                    }
                    else
                    {
                        UndoScript.Instance.LogMove(gameObject, Constants.LogMoveTypes.draw, isAction);
                    }
                }

                WastepileScript.Instance.AddCard(gameObject, showHolo: showHolo);
                break;
            case Constants.Tags.deck:
                if (doLog)
                {
                    UndoScript.Instance.LogMove(gameObject, Constants.LogMoveTypes.deckreset, isAction);
                }
                DeckScript.Instance.AddCard(gameObject);
                break;
            case Constants.Tags.matchedPile:
                if (doLog)
                {
                    UndoScript.Instance.LogMove(gameObject, Constants.LogMoveTypes.match, isAction, nextCardWasHidden);
                }
                MatchedPileScript.Instance.AddCard(gameObject);
                break;
            case Constants.Tags.loadPile:
                if (doLog)
                {
                    UndoScript.Instance.LogMove(gameObject, Constants.LogMoveTypes.match, isAction, nextCardWasHidden);
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
        hologram.GetComponent<Animator>().Play(0, -1, UnityEngine.Random.Range(0.0f, 1.0f));

        SpriteRenderer holoSR = hologram.GetComponent<SpriteRenderer>();
        SpriteRenderer objectSR = hologramFood.GetComponent<SpriteRenderer>();
        Color holoColor = holoSR.color;

        float duration = GameValues.AnimationDurataions.cardHologramFadeIn;
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            holoColor.a = Mathf.Lerp(0, 1, timeElapsed / duration);
            holoSR.color = holoColor;
            objectSR.color = holoColor;
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        holoColor.a = 1;
        holoSR.color = holoColor;
        objectSR.color = holoColor;
    }
}
