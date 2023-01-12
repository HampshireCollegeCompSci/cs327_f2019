﻿using System.Collections;
using UnityEngine;

public class CardScript : MonoBehaviour, IGlow
{
    [SerializeField]
    private Card _card;

    [SerializeField]
    private Sprite cardFrontSprite, cardBackSprite, hologramFoodSprite, hologramComboSprite;
    [SerializeField]
    private GameObject values, suitObject, rankObject, glow, hologram, hologramFood;

    private SpriteRenderer thisSR, hologramSR, hologramFoodSR, glowSR;
    private Renderer[] renderers;
    private BoxCollider2D thisBC2D;
    private Animator hologramAnimator;

    [SerializeField]
    private byte _cardID;

    [SerializeField]
    private bool _enabled, _hidden, _hitBox, _obstructed, _dragging;
    [SerializeField]
    private bool _hologram;
    [SerializeField]
    private HighLightColor _hologramColor;
    [SerializeField]
    private bool _glowing;
    [SerializeField]
    private HighLightColor _glowColor;

    private Coroutine holoCoroutine;
    private Color originalColor;
    private Color draggingColor;
    private Constants.CardContainerType _currentContainerType;
    private GameObject _container;

    void Awake()
    {
        thisSR = gameObject.GetComponent<SpriteRenderer>();
        renderers = gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
        thisBC2D = gameObject.GetComponent<BoxCollider2D>();
        hologramSR = hologram.GetComponent<SpriteRenderer>();
        hologramFoodSR = hologramFood.GetComponent<SpriteRenderer>();
        glowSR = glow.GetComponent<SpriteRenderer>();
        hologramAnimator = hologram.GetComponent<Animator>();
    }

    public Card Card => _card;

    public byte CardID => _cardID;

    public Constants.CardContainerType CurrentContainerType => _currentContainerType;

    public GameObject Container => _container;

    public GameObject Values => values;

    public GameObject HologramFood => hologramFood;

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
            thisSR.enabled = value;
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
                thisSR.sprite = cardBackSprite;
                HitBox = false;
            }
            else
            {
                thisSR.sprite = cardFrontSprite;
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
            thisBC2D.enabled = value;
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
                SetColor(GameValues.Colors.cardObstructedColor);
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
                foreach (Renderer renderers in renderers)
                {
                    renderers.material.color = draggingColor;
                }
            }
            else
            {
                foreach (Renderer renderers in renderers)
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
    public HighLightColor HologramColor
    {
        get => _hologramColor;
        set
        {
            if (_hologramColor.Equals(value)) return;

            hologramSR.color = value.Color;
            hologramFoodSR.color = value.Color;

            if (value.Equals(GameValues.Colors.Highlight.match))
            {
                hologramFoodSR.sprite = hologramComboSprite;
            }
            else if (_hologramColor.Equals(GameValues.Colors.Highlight.match))
            {
                hologramFoodSR.sprite = hologramFoodSprite;
            }

            // do this at the end
            _hologramColor = value;
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
    public HighLightColor GlowColor
    {
        get => _glowColor;
        set
        {
            Glowing = true;
            if (!_glowColor.Equals(value))
            {
                _glowColor = value;
                glowSR.color = value.Color;
            }
        }
    }

    public void SetUp(Card card, Sprite suitSprite, Sprite hologramFoodSprite, Sprite hologramComboSprite)
    {
        _card = card;
        _currentContainerType = Constants.CardContainerType.None;
        _container = null;

        _cardID = (byte)(card.Rank.Value + card.Suit.Index * 13);
        this.name = $"{card.Suit.Name}_{card.Rank.Value}";

        hologramFoodSR.sprite = hologramFoodSprite;
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
            originalColor = new Color(
                Random.Range(0.4f, 1),
                Random.Range(0.4f, 1),
                Random.Range(0.4f, 1));
            // TODO: this needs to change
            SetColor(originalColor);
        }
        else
        {
            originalColor = Color.white;
        }
        draggingColor = originalColor;
        draggingColor.a = GameValues.Colors.selectedCardOpacity;

        // initialized default values
        _enabled = true;
        _hidden = false;
        _hitBox = false;
        _obstructed = false;
        _hologram = false;
        _dragging = false;
        _glowing = false;
        _glowColor = GameValues.Colors.Highlight.none;
        _hologramColor = GameValues.Colors.Highlight.none;
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
        thisSR.color = setTo;
    }

    /// <summary>
    /// For the cloned cards that are being dragged.
    /// </summary>
    public void MakeVisualOnly()
    {
        gameObject.transform.localScale = new Vector3(GameValues.Transforms.draggedCardScale, GameValues.Transforms.draggedCardScale, 1);
        _container = null;

        values.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingLayerID = Config.Instance.SelectedCardsLayer;

        foreach (Renderer renderers in this.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true))
        {
            renderers.sortingLayerID = Config.Instance.SelectedCardsLayer;
            // TODO : A Unity bug requires this
            renderers.material.color = renderers.material.color;
        }

        // when picking up a card that has a hologram that's just starting up
        // make the dragged card's hologram have full alpha
        Color holoColor = hologramSR.color;
        holoColor.a = 1;
        hologramSR.color = holoColor;
        hologramFoodSR.color = holoColor;

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
    public void MoveCard(Constants.CardContainerType newContainerType, GameObject destination, bool doLog = true, bool isAction = true, bool isCycle = false, bool isStack = false, bool showHolo = true)
    {
        bool nextCardWasHidden = false;

        switch (_currentContainerType)
        {
            case Constants.CardContainerType.None:
                break;
            case Constants.CardContainerType.Foundation:
                FoundationScript foundationScript = _container.GetComponent<FoundationScript>();
                if (doLog && foundationScript.CardList.Count > 1 && foundationScript.CardList[^2].GetComponent<CardScript>().Hidden)
                {
                    nextCardWasHidden = true;
                }
                foundationScript.RemoveCard(gameObject, showHolo: showHolo);
                break;
            case Constants.CardContainerType.Reactor:
                _container.GetComponent<ReactorScript>().RemoveCard(gameObject);
                break;
            case Constants.CardContainerType.WastePile:
                if (!doLog || newContainerType == Constants.CardContainerType.Deck)
                {
                    WastepileScript.Instance.RemoveCard(gameObject, undoingOrDeck: true, showHolo: showHolo);
                }
                else
                {
                    WastepileScript.Instance.RemoveCard(gameObject, showHolo: showHolo);
                }
                break;
            case Constants.CardContainerType.Deck:
                DeckScript.Instance.RemoveCard(gameObject);
                break;
            case Constants.CardContainerType.MatchedPile:
                MatchedPileScript.Instance.RemoveCard(gameObject);
                break;
            case Constants.CardContainerType.Loadpile:
                LoadPileScript.Instance.RemoveCard(gameObject);
                break;
            default:
                throw new System.Exception($"the card conatainer of {_currentContainerType} is not supported");
        }

        switch (newContainerType)
        {
            case Constants.CardContainerType.Foundation:
                if (doLog)
                {
                    UndoScript.Instance.LogMove(gameObject, _currentContainerType, _container, (isStack ? Constants.LogMoveType.Stack : Constants.LogMoveType.Move), isAction, nextCardWasHidden);
                }
                destination.GetComponent<FoundationScript>().AddCard(gameObject, showHolo: showHolo);
                break;
            case Constants.CardContainerType.Reactor:
                if (doLog)
                {
                    UndoScript.Instance.LogMove(gameObject, _currentContainerType, _container, (isCycle ? Constants.LogMoveType.Cycle : Constants.LogMoveType.Move), true, nextCardWasHidden);
                }
                destination.GetComponent<ReactorScript>().AddCard(gameObject);
                break;
            case Constants.CardContainerType.WastePile:
                if (doLog)
                {
                    // for undoing a match that goes into the wastepile
                    if (_currentContainerType == Constants.CardContainerType.Foundation)
                    {
                        UndoScript.Instance.LogMove(gameObject, _currentContainerType, _container, Constants.LogMoveType.Move, isAction, nextCardWasHidden);
                    }
                    else
                    {
                        UndoScript.Instance.LogMove(gameObject, _currentContainerType, _container, Constants.LogMoveType.Draw, isAction);
                    }
                }

                WastepileScript.Instance.AddCard(gameObject, showHolo: showHolo);
                break;
            case Constants.CardContainerType.Deck:
                if (doLog)
                {
                    UndoScript.Instance.LogMove(gameObject, _currentContainerType, _container, Constants.LogMoveType.Deckreset, isAction);
                }
                DeckScript.Instance.AddCard(gameObject);
                break;
            case Constants.CardContainerType.MatchedPile:
                if (doLog)
                {
                    UndoScript.Instance.LogMove(gameObject, _currentContainerType, _container, Constants.LogMoveType.Match, isAction, nextCardWasHidden);
                }
                MatchedPileScript.Instance.AddCard(gameObject);
                break;
            case Constants.CardContainerType.Loadpile:
                LoadPileScript.Instance.AddCard(gameObject);
                break;
            default:
                throw new System.Exception($"the card destination of {newContainerType} is not supported");
        }

        _currentContainerType = newContainerType;
        _container = destination;
    }

    /// <summary>
    /// Starts the card's hologram at a random frame and fades it in.
    /// </summary>
    private IEnumerator StartHologram()
    {
        hologram.SetActive(true);
        hologramFood.SetActive(true);

        // start the animation at a random frame
        hologramAnimator.Play(0, -1, Random.Range(0.0f, 1.0f));

        Color holoColor = hologramSR.color;

        float duration = GameValues.AnimationDurataions.cardHologramFadeIn;
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            holoColor.a = Mathf.Lerp(0, 1, timeElapsed / duration);
            hologramSR.color = holoColor;
            hologramFoodSR.color = holoColor;
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        holoColor.a = 1;
        hologramSR.color = holoColor;
        hologramFoodSR.color = holoColor;
    }
}
