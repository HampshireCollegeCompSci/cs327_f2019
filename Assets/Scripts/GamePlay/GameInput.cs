using System.Collections;
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
    private GameObject hoveringOver;
    [SerializeField]
    private bool changedHologramColor, wasOnMatch, changedSuitGlowColor, hidFoodHologram;

    [SerializeField]
    private bool _inputStopped, _draggingCard;
    [SerializeField]
    private int inputStopRequests;

    private Vector3 oldPointerPosition, currentPointerPosition, clickPosition;
    private ShowPossibleMoves showPossibleMoves;

    private bool autoPlacing, autoPlacingOutOfRange;
    private static readonly WaitForSeconds autoPlacementDelay = new(GameValues.AnimationDurataions.autoPlacementDelaySec);

    // Initialize the singleton instance.
    void Awake()
    {
        if (Instance != null)
        {
            throw new System.ArgumentException("there should not already be an instance of this");
        }

        Instance = this;
        selectedCards = new(GameValues.GamePlay.rankCount);
        selectedCardsCopy = new(GameValues.GamePlay.rankCount);
        CardPlacement = true;
        showPossibleMoves = new ShowPossibleMoves();
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

    public bool DraggingStack { get; private set; }

    private bool DraggingCard
    {
        get => _draggingCard;
        set
        {
            _draggingCard = value;
            InputStopped = value;
            if (!value)
            {
                showPossibleMoves.HideMoves();
                UnselectCards();
                DraggingStack = false;
            }
        }
    }

    void Update()
    {
        if (autoPlacing) return;
        if (DraggingCard)
        {
            bool continueDragging = !Input.GetMouseButtonUp(0);
            currentPointerPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // no movement detected
            if (continueDragging && currentPointerPosition == oldPointerPosition) return;

            if (Config.Instance.AutoPlacementEnabled && !autoPlacingOutOfRange)
            {
                // disable auto placing if the distance threshold is ever exceeded
                if (Vector2.Distance(currentPointerPosition, clickPosition) >
                    GameValues.Settings.autoPlacementDistance)
                {
                    autoPlacingOutOfRange = true;
                }
                if (!continueDragging && !autoPlacingOutOfRange)
                {
                    AutoPlacement();
                    return;
                }
            }

            RaycastHit2D hit = Physics2D.Raycast(
                currentPointerPosition,
                currentPointerPosition,
                0,
                Constants.LayerMaskIDs.cards | Constants.LayerMaskIDs.cardContainers);

            // keep dragging
            if (continueDragging)
            {
                oldPointerPosition = currentPointerPosition;
                DragSelectedCards(hit);
                return;
            }

            // try placing the cards
            DragGlowRevert(isPlacing: true);
            if (hit.collider != null)
                TryToPlaceCards(hit.collider.gameObject);
            DraggingCard = false;
        }
        else if (!InputStopped && Input.GetMouseButtonDown(0))
        {
            currentPointerPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            oldPointerPosition = currentPointerPosition;

            if (Config.Instance.AutoPlacementEnabled)
            {
                clickPosition = currentPointerPosition;
                autoPlacingOutOfRange = false;
            }

            RaycastHit2D hit = Physics2D.Raycast(
                currentPointerPosition,
                currentPointerPosition,
                0,
                Constants.LayerMaskIDs.cards);

            // the deck's mask has a card layerID with a deck tag
            if (hit.collider == null ||
                !hit.collider.gameObject.CompareTag(Constants.Tags.card)) return;

            DraggingCard = true;
            SelectCards(hit);
            DragSelectedCards(hit);
        }
    }

    private void SelectCards(RaycastHit2D hit)
    {
        SoundEffectsController.Instance.CardPressSound();
        GameObject hitGameObject = hit.collider.gameObject;
        selectedCards.Add(hitGameObject);
        CardScript hitCardScript = hitGameObject.GetComponent<CardScript>();

        switch (hitCardScript.CurrentContainerType)
        {
            case Constants.CardContainerType.WastePile:
                // disable wastepile scrolling as dragging its cards can cause scrolling
                WastepileScript.Instance.DraggingCard = true;
                break;
            case Constants.CardContainerType.Foundation:
                List<GameObject> foundationCardList = hitCardScript.Container.GetComponent<FoundationScript>().CardList;
                // select all cards above the hit one
                for (int i = foundationCardList.LastIndexOf(hitGameObject) + 1; i < foundationCardList.Count; i++)
                {
                    selectedCards.Add(foundationCardList[i]);
                }
                if (selectedCards.Count > 1)
                {
                    DraggingStack = true;
                }
                break;
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
        if (hitCardScript.CurrentContainerType == Constants.CardContainerType.Reactor)
        {
            // enable dragged reactor tokens holograms as they are off
            topSelectedCopyCardScript.EnableHologramImmediately();
            topSelectedCopyCardScript.Hologram = true;
        }

        // show everything that we can interact with
        showPossibleMoves.ShowMoves(hitCardScript);

        changedHologramColor = false;
        wasOnMatch = false;
        changedSuitGlowColor = false;
        hidFoodHologram = false;
    }

    private void TryToPlaceCards(GameObject newContainer)
    {
        if (!CardPlacement) return;
        if (newContainer.Equals(selectedCardsCopy[0].GetComponent<CardScript>().gameObject))
        {
            Debug.LogError("tried to place card on its own copy");
            return;
        }

        if (!newContainer.TryGetComponent<IGlow>(out var glowObject)
            || !glowObject.Glowing) return;

        if (newContainer.CompareTag(Constants.Tags.card))
        {
            CardScript hitCardScript = newContainer.GetComponent<CardScript>();
            if (hitCardScript.GlowColor.ColorLevel == Constants.ColorLevel.Match)
            {
                CardScript selectedCardScript = selectedCards[0].GetComponent<CardScript>();
                matchCards.Match(selectedCardScript, hitCardScript, selectedCardsCopy[0]);
                return;
            }
            newContainer = hitCardScript.Container;
        }

        if (!newContainer.TryGetComponent<ICardContainer>(out var cardContainer)) return;

        Constants.CardContainerType oldContainerType = selectedCards[0].GetComponent<CardScript>().CurrentContainerType;
        Constants.CardContainerType newContainerType = cardContainer.ContainerType;

        MoveAllSelectedCards(newContainerType, newContainer);

        // if the card was from a foundation and moved into a non foundation container
        bool checkGameOver = oldContainerType == Constants.CardContainerType.Foundation &&
            newContainerType != Constants.CardContainerType.Foundation;
        Actions.MoveUpdate(checkGameOver);

        switch (newContainerType)
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

    private void MoveAllSelectedCards(Constants.CardContainerType newContainerType, GameObject destination)
    {
        switch (selectedCards.Count)
        {
            case 0:
                Debug.LogError("tried to move an empty selected cards list");
                break;
            case 1:
                selectedCards[0].GetComponent<CardScript>().MoveCard(newContainerType, destination);
                break;
            default:
                int bottomCardCount = selectedCards.Count - 1;
                for (int i = 0; i < bottomCardCount; i++)
                {
                    selectedCards[i].GetComponent<CardScript>().MoveCard(newContainerType, destination, isStack: true, showHolo: false);
                }
                selectedCards[^1].GetComponent<CardScript>().MoveCard(newContainerType, destination, isStack: true, showHolo: true);
                break;
        }
    }

    private void UnselectCards()
    {
        WastepileScript.Instance.DraggingCard = false;
        selectedCards.ForEach(c => c.GetComponent<CardScript>().Dragging = false);
        selectedCards.Clear();
        selectedCardsCopy.ForEach(c => Destroy(c));
        selectedCardsCopy.Clear();
        topSelectedCopyCardScript = null;
    }

    private void DragSelectedCards(RaycastHit2D hit)
    {
        if (DraggingStack)
        {
            foreach (GameObject card in selectedCardsCopy)
            {
                card.transform.position = currentPointerPosition;
                currentPointerPosition.y += GameValues.Transforms.draggedCardYOffset;
                currentPointerPosition.z += GameValues.Transforms.draggedCardXOffset;
            }
        }
        else
        {
            selectedCardsCopy[0].transform.position = currentPointerPosition;
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
        UpdateDragGlow(hit.collider.gameObject);
    }

    private void UpdateDragGlow(GameObject target)
    {
        hoveringOver = target;
        if (!target.TryGetComponent<IGlow>(out var glowObject)
            || !glowObject.Glowing) return;

        topSelectedCopyCardScript.HologramColor = glowObject.GlowColor;
        changedHologramColor = true;

        switch (target.tag)
        {
            case Constants.Tags.card:
                CardScript targetCard = target.GetComponent<CardScript>();

                if (targetCard.GlowColor.ColorLevel != Constants.ColorLevel.Match) return;
                topSelectedCopyCardScript.MatchChangeFoodHologram(true);
                wasOnMatch = true;

                if (targetCard.CurrentContainerType == Constants.CardContainerType.Reactor) return;
                targetCard.Hologram = false;
                hidFoodHologram = true;
                break;
            case Constants.Tags.reactor:
                ReactorScript targetReactor = target.GetComponent<ReactorScript>();
                targetReactor.ChangeSuitGlow(Config.Instance.CurrentColorMode.Notify);
                changedSuitGlowColor = true;
                break;
            case Constants.Tags.foundation:
                break;
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

    private void AutoPlacement()
    {
        GameObject target = null;
        Vector2 endPosition = Vector2.zero;
        if (showPossibleMoves.matchTokensAreGlowing)
        {
            target = showPossibleMoves.cardMatch;
        }
        else if (showPossibleMoves.moveTokensAreGlowing)
        {
            target = showPossibleMoves.cardMoves[0];
            endPosition = target.transform.position;
            endPosition.y += 0.4f;
        }
        else if (showPossibleMoves.foundationIsGlowing)
        {
            target = showPossibleMoves.foundationMoves[0];
        }
        else if (showPossibleMoves.reactorIsGlowing)
        {
            ReactorScript reactor = showPossibleMoves.reactorMove.GetComponent<ReactorScript>();
            if (reactor.GlowColor.ColorLevel != Constants.ColorLevel.Over)
            {
                target = showPossibleMoves.reactorMove;
                endPosition = reactor.GetNextCardPosition();
            }
        }

        if (target == null)
        {
            DragGlowRevert();
            DraggingCard = false;
            return;
        }
        if (endPosition == Vector2.zero)
        {
            endPosition = target.transform.position;
        }

        autoPlacing = true;
        if (selectedCardsCopy.Count == 0)
        {
            StartCoroutine(MoveCard(selectedCardsCopy[0], target, endPosition));
        }
        else
        {
            StartCoroutine(MoveCards(target, endPosition));
        }
    }

    private IEnumerator MoveCard(GameObject card, GameObject target, Vector2 endPosition)
    {
        yield return Animate.SmoothstepTransform(card.transform,
                card.transform.position,
                endPosition,
                GameValues.AnimationDurataions.autoPlacementDuration);

        yield return EndAutoPlacement(target);
    }

    private IEnumerator MoveCards(GameObject target, Vector2 endPosition)
    {
        Transform[] cardTransforms = new Transform[selectedCardsCopy.Count];
        for (int i = 0; i < selectedCardsCopy.Count; i++)
        {
            cardTransforms[i] = selectedCardsCopy[i].transform;
        }

        yield return Animate.SmoothstepTransformCards(cardTransforms,
            selectedCardsCopy[0].transform.position,
            endPosition,
            GameValues.AnimationDurataions.autoPlacementDuration);

        yield return EndAutoPlacement(target);
    }

    private IEnumerator EndAutoPlacement(GameObject target)
    {
        UpdateDragGlow(target);
        yield return autoPlacementDelay;
        DragGlowRevert(isPlacing: true);
        TryToPlaceCards(target);
        DraggingCard = false;
        autoPlacing = false;
    }
}
