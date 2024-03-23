using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    // Singleton instance.
    public static GameInput Instance { get; private set; }

    public GameObject[] reactors;
    public ReactorScript[] reactorScripts;

    public GameObject[] foundations;
    public FoundationScript[] foundationScripts;

    [SerializeField]
    private MatchCards matchCards;

    [SerializeField]
    private List<GameObject> selectedCards, selectedCardsCopy;
    private CardScript topSelectedCopyCardScript;

    [SerializeField]
    private bool dragOn;
    [SerializeField]
    private GameObject hoveringOver;
    [SerializeField]
    private bool changedHologramColor, wasOnMatch, changedSuitGlowColor, hidFoodHologram;

    [SerializeField]
    private bool _inputStopped;
    [SerializeField]
    private int inputStopRequests;

    private Vector3 oldPointerPosition, currentPointerPosition;
    private ShowPossibleMoves showPossibleMoves;

    // Initialize the singleton instance.
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            selectedCards = new(13);
            selectedCardsCopy = new(13);
            CardPlacement = true;
            showPossibleMoves = new ShowPossibleMoves();
        }
        else if (Instance != this)
        {
            throw new System.ArgumentException("there should not already be an instance of this");
        }
    }

    void Start()
    {
        InputStopped = true;
    }

    public ShowPossibleMoves ShowPossibleMoves => showPossibleMoves;

    public bool InputStopped
    {
        get => _inputStopped;
        set
        {
            if (value)
            {
                inputStopRequests++;
                if (!_inputStopped)
                {
                    _inputStopped = true;
                    EndGame.Instance.TrySetInteraction(false);
                }
            }
            else
            {
                inputStopRequests--;
                if (inputStopRequests == 0)
                {
                    _inputStopped = false;
                    EndGame.Instance.TrySetInteraction(true);
                }
            }
        }
    }

    public bool CardPlacement { get; set; }

    void Update()
    {
        if (dragOn)
        {
            if (Input.GetMouseButtonUp(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(
                    Camera.main.ScreenToWorldPoint(Input.mousePosition),
                    Vector2.zero,
                    0,
                    Constants.LayerMaskIDs.cards | Constants.LayerMaskIDs.cardContainers);

                DragGlowRevert(isPlacing: true);
                TryToPlaceCards(hit);
                UnselectCards();
                showPossibleMoves.HideMoves();
                dragOn = false;
                InputStopped = false;
            }
            else
            {
                currentPointerPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (currentPointerPosition == oldPointerPosition) return;
                oldPointerPosition = currentPointerPosition;

                RaycastHit2D hit = Physics2D.Raycast(
                    currentPointerPosition,
                    Vector2.zero,
                    0,
                    Constants.LayerMaskIDs.cards | Constants.LayerMaskIDs.cardContainers);

                DragSelectedCards(hit);
            }
        }
        else if (Input.GetMouseButtonDown(0) && !InputStopped)
        {
            currentPointerPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            oldPointerPosition = currentPointerPosition;
            RaycastHit2D hit = Physics2D.Raycast(
                currentPointerPosition,
                Vector2.zero,
                1,
                Constants.LayerMaskIDs.cards);
            if (hit.collider == null) return;

            dragOn = true;
            InputStopped = true;
            SelectCards(hit);
            SoundEffectsController.Instance.CardPressSound();
            DragSelectedCards(hit);
        }
    }

    private void SelectCards(RaycastHit2D hit)
    {
        GameObject hitGameObject = hit.collider.gameObject;
        selectedCards.Add(hitGameObject);
        CardScript hitCardScript = hitGameObject.GetComponent<CardScript>();

        //if we click a card in the wastepile select it
        if (hitCardScript.CurrentContainerType == Constants.CardContainerType.WastePile)
        {
            // all non-top wastepile cards have their hitboxes disabled
            WastepileScript.Instance.DraggingCard = true;
        }
        else if (hitCardScript.CurrentContainerType == Constants.CardContainerType.Foundation)
        {
            //if we click a card in a foundation
            List<GameObject> cardListREF = hitCardScript.Container.GetComponent<FoundationScript>().CardList;
            // select any cards above the hit one
            for (int i = cardListREF.LastIndexOf(hitGameObject) + 1; i < cardListREF.Count; i++)
            {
                selectedCards.Add(cardListREF[i]);
            }
        }

        // make a copy of the selected cards to move around
        GameObject cardCopy;
        foreach (GameObject card in selectedCards)
        {
            cardCopy = Instantiate(card, card.transform.position, Quaternion.identity);
            cardCopy.GetComponent<CardScript>().MakeVisualOnly();
            selectedCardsCopy.Add(cardCopy);

            card.GetComponent<CardScript>().Dragging = true;
        }

        topSelectedCopyCardScript = selectedCardsCopy[^1].GetComponent<CardScript>();
        // potentially enable dragged reactor tokens holograms
        topSelectedCopyCardScript.Hologram = true;

        // show any tokens (and reactors) that we can interact with
        showPossibleMoves.ShowMoves(hitCardScript);

        changedHologramColor = false;
        wasOnMatch = false;
        changedSuitGlowColor = false;
        hidFoodHologram = false;
    }

    private void TryToPlaceCards(RaycastHit2D hit)
    {
        if (hit.collider == null || !CardPlacement) return;

        Constants.CardContainerType oldContainer = selectedCards[0].GetComponent<CardScript>().CurrentContainerType;
        // hit object is what the card will attempt to go into
        GameObject newContainer = hit.collider.gameObject;

        if (newContainer.Equals(selectedCardsCopy[0].GetComponent<CardScript>().gameObject))
        {
            Debug.LogError("tried to place card on its own copy");
            return;
        }

        // if the destination is glowing, then something can happen
        switch (newContainer.tag)
        {
            case Constants.Tags.card:
                CardScript hitCardScript = newContainer.GetComponent<CardScript>();
                if (hitCardScript.Glowing)
                {
                    if (hitCardScript.GlowColor.Equals(Config.Instance.CurrentColorMode.Match))
                    {
                        CardScript selectedCardScript = selectedCards[0].GetComponent<CardScript>();
                        bool cardFromFoundation = selectedCardScript.CurrentContainerType == Constants.CardContainerType.Foundation ||
                            hitCardScript.CurrentContainerType == Constants.CardContainerType.Foundation;
                        matchCards.Match(selectedCardScript, hitCardScript, selectedCardsCopy[0]);
                        Actions.MatchUpdate(cardFromFoundation);
                    }
                    else
                    {
                        MoveAllSelectedCards(hitCardScript.CurrentContainerType, hitCardScript.Container);
                        OtherActions(oldContainer, hitCardScript.CurrentContainerType);
                    }
                }
                break;
            case Constants.Tags.foundation:
                if (newContainer.GetComponent<FoundationScript>().Glowing)
                {
                    MoveAllSelectedCards(Constants.CardContainerType.Foundation, newContainer);
                    OtherActions(oldContainer, Constants.CardContainerType.Foundation);
                }
                break;
            case Constants.Tags.reactor:
                if (newContainer.GetComponent<ReactorScript>().Glowing)
                {
                    MoveAllSelectedCards(Constants.CardContainerType.Reactor, newContainer);
                    OtherActions(oldContainer, Constants.CardContainerType.Reactor);
                }
                break;
            default:
                break;
        }
    }

    private void OtherActions(Constants.CardContainerType oldContainer, Constants.CardContainerType newContainer)
    {
        // if the card was from a foundation and moved into a non foundation container
        bool checkGameOver = oldContainer == Constants.CardContainerType.Foundation && newContainer != Constants.CardContainerType.Foundation;
        Actions.MoveUpdate(checkGameOver);

        switch (newContainer)
        {
            case Constants.CardContainerType.Reactor:
                SoundEffectsController.Instance.CardToReactorSound();
                break;
            case Constants.CardContainerType.Foundation:
                SoundEffectsController.Instance.CardStackSound();
                break;
            default:
                throw new System.ArgumentException($"{newContainer} is an unexpected card container");
        }
    }

    private void MoveAllSelectedCards(Constants.CardContainerType newContainer, GameObject destination)
    {
        if (selectedCards.Count > 1)
        {
            for (int i = 0; i < selectedCards.Count - 1; i++)
            {
                selectedCards[i].GetComponent<CardScript>().MoveCard(newContainer, destination, isStack: true, showHolo: false);
            }
            selectedCards[^1].GetComponent<CardScript>().MoveCard(newContainer, destination, isStack: true, showHolo: true);
        }
        else
        {
            selectedCards[0].GetComponent<CardScript>().MoveCard(newContainer, destination);
        }
    }

    private void UnselectCards()
    {
        if (WastepileScript.Instance.DraggingCard)
        {
            WastepileScript.Instance.DraggingCard = false;
        }

        foreach (GameObject card in selectedCards)
        {
            card.GetComponent<CardScript>().Dragging = false;
        }
        selectedCards.Clear();

        foreach (GameObject card in selectedCardsCopy)
        {
            Destroy(card);
        }
        selectedCardsCopy.Clear();
        topSelectedCopyCardScript = null;
    }

    private void DragSelectedCards(RaycastHit2D hit)
    {
        foreach (GameObject card in selectedCardsCopy)
        {
            card.transform.position = currentPointerPosition;
            currentPointerPosition.y += GameValues.Transforms.draggedCardYOffset;
            currentPointerPosition.z -= 0.01f;
        }

        // glow time
        // if the tutorial is not on and there is no stuff glowing, stop
        if (!showPossibleMoves.AreThingsGlowing()) return;

        if (hit.collider == null)
        {
            DragGlowRevert();
            hoveringOver = null;
            return;
        }

        // are we still hovering over the same object
        if (hit.collider.gameObject == hoveringOver) return;

        DragGlowRevert();
        hoveringOver = hit.collider.gameObject;

        // if we are hovering over a glowing card
        if (showPossibleMoves.AreCardsGlowing() &&
            hoveringOver.CompareTag(Constants.Tags.card))
        {
            CardScript hoveringOverCS = hoveringOver.GetComponent<CardScript>();
            if (!hoveringOverCS.Glowing) return;

            // change the dragged card hologram color to what it's hovering over
            topSelectedCopyCardScript.HologramColor = hoveringOverCS.GlowColor;
            changedHologramColor = true;

            if (hoveringOverCS.GlowColor.ColorLevel != Constants.ColorLevel.Match) return;

            topSelectedCopyCardScript.MatchChangeFoodHologram(true);
            wasOnMatch = true;

            if (hoveringOverCS.CurrentContainerType == Constants.CardContainerType.Reactor) return;

            // hide the hover over card food hologram
            hoveringOverCS.Hologram = false;
            hidFoodHologram = true;
        }
        // else if we are hovering over a glowing reactor
        else if (showPossibleMoves.reactorIsGlowing &&
            hoveringOver.CompareTag(Constants.Tags.reactor))
        {
            ReactorScript hoveringOverRS = hoveringOver.GetComponent<ReactorScript>();
            if (!hoveringOverRS.Glowing) return;

            topSelectedCopyCardScript.HologramColor = hoveringOverRS.GlowColor;
            changedHologramColor = true;

            hoveringOverRS.ChangeSuitGlow(Config.Instance.CurrentColorMode.Notify);
            changedSuitGlowColor = true;
        }
        else if (showPossibleMoves.foundationIsGlowing &&
            hoveringOver.CompareTag(Constants.Tags.foundation))
        {
            FoundationScript hoveringOverFS = hoveringOver.GetComponent<FoundationScript>();
            if (!hoveringOverFS.Glowing) return;

            topSelectedCopyCardScript.HologramColor = hoveringOverFS.GlowColor;
            changedHologramColor = true;
        }
    }

    private void DragGlowRevert(bool isPlacing = false)
    {
        // if we where hovering over a glowing reactor
        if (changedSuitGlowColor)
        {
            hoveringOver.GetComponent<ReactorScript>().RevertSuitGlow();
            changedSuitGlowColor = false;
        }

        // if we where hovering over a glowing token
        if (changedHologramColor)
        {
            topSelectedCopyCardScript.HologramColor = GameValues.Colors.card;
            changedHologramColor = false;
        }

        if (wasOnMatch && !isPlacing)
        {
            topSelectedCopyCardScript.MatchChangeFoodHologram(false);
            wasOnMatch = false;
        }

        // if we where hovering over a matching glowing token
        if (hidFoodHologram)
        {
            hoveringOver.GetComponent<CardScript>().Hologram = true;
            hidFoodHologram = false;
        }
    }
}
