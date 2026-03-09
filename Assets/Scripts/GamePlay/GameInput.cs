using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private CardScript selectedCardScript;
    private CardScript topSelectedCopyCardScript;

    [SerializeField]
    private GameObject hoveringOver;
    [SerializeField]
    private bool changedHologramColor, wasOnMatch, changedSuitGlowColor, hidFoodHologram;

    [SerializeField]
    private bool _inputStopped, _draggingCard;
    [SerializeField]
    private int inputStopRequests;

    private InputAction clickAction, positionAction;

    private Vector2 clickPosition, oldPointerPosition;
    private float clickStartTime;
    private ShowPossibleMoves showPossibleMoves;

    private bool autoPlacing;

    // Initialize the singleton instance.
    void Awake()
    {
        if (Instance != null)
            throw new System.ArgumentException("there should not already be an instance of this");
        Instance = this;

        selectedCards = new(GameValues.GamePlay.rankCount);
        selectedCardsCopy = new(GameValues.GamePlay.rankCount);
        CardPlacement = true;
        showPossibleMoves = new ShowPossibleMoves();
    }

    void Start()
    {
        InputStopped = true;
        clickAction = InputSystem.actions.FindAction("Click");
        positionAction = InputSystem.actions.FindAction("Position");
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
                    PauseGameScript.Instance.DisablePause();
                }
            }
            else
            {
                inputStopRequests--;
                if (inputStopRequests == 0)
                {
                    _inputStopped = false;
                    EndGame.Instance.TrySetInteraction(true);
                    PauseGameScript.Instance.EnablePause();
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
                WastepileScript.Instance.DraggingCard = false;
                selectedCards.ForEach(c => c.GetComponent<CardScript>().Dragging = false);
                selectedCards.Clear();
                selectedCardScript = null;
                selectedCardsCopy.ForEach(c => Destroy(c));
                selectedCardsCopy.Clear();
                topSelectedCopyCardScript = null;
                DraggingStack = false;
            }
        }
    }

    void Update()
    {
        if (autoPlacing) return;
        if (!InputStopped && clickAction.WasPressedThisFrame())
        {
            InputStart();
        }
        else if (DraggingCard)
        {
            if (clickAction.WasReleasedThisFrame())
                InputStop();
            else
                InputContinue();
        }
    }

    private void InputStart()
    {
        Vector2 pointerPosition = Camera.main.ScreenToWorldPoint(positionAction.ReadValue<Vector2>());
        oldPointerPosition = pointerPosition;

        if (AutoPlacement.Enabled)
        {
            clickPosition = pointerPosition;
            clickStartTime = Time.time;
        }

        RaycastHit2D hit = Physics2D.Raycast(
            pointerPosition,
            pointerPosition,
            0,
            Constants.LayerMaskIDs.cards);

        // the deck's mask has a card layerID with a deck tag
        if (hit.collider == null ||
            !hit.collider.gameObject.CompareTag(Constants.Tags.card)) return;

        DraggingCard = true;
        SelectCards(hit);
        DragSelectedCards(pointerPosition, hit);
    }

    private void InputContinue()
    {
        Vector2 pointerPosition = Camera.main.ScreenToWorldPoint(positionAction.ReadValue<Vector2>());
        if (pointerPosition == oldPointerPosition) return;
        RaycastHit2D hit = GetCardPlacementHit(pointerPosition);
        DragSelectedCards(pointerPosition, hit);
        oldPointerPosition = pointerPosition;
    }

    private void InputStop()
    {
        Vector2 pointerPosition = Camera.main.ScreenToWorldPoint(positionAction.ReadValue<Vector2>());
        RaycastHit2D hit = GetCardPlacementHit(pointerPosition);

        DragGlowRevert(isPlacing: true);
        if (hit.collider != null && TryToPlaceCards(hit.collider.gameObject))
        {
            // do nothing
        }
        else if (AutoPlacement.Enabled)
        {
            float movementDistance = Vector2.Distance(pointerPosition, clickPosition);
            float pressDuration = Time.time - clickStartTime;
            //Debug.Log($"Move Distance: {movementDistance}, Duration: {pressDuration}");

            if (movementDistance <= AutoPlacement.DistanceValue &&
                pressDuration <= AutoPlacement.Time)
            {
                AutoPlace();
                return;
            }
        }

        DraggingCard = false;
    }

    private RaycastHit2D GetCardPlacementHit(Vector3 position)
    {
        return Physics2D.Raycast(
            position,
            position,
            0,
            Constants.LayerMaskIDs.cards | Constants.LayerMaskIDs.cardContainers);
    }

    private void SelectCards(RaycastHit2D hit)
    {
        GameObject hitGameObject = hit.collider.gameObject;
        selectedCards.Add(hitGameObject);
        selectedCardScript = hitGameObject.GetComponent<CardScript>();

        switch (selectedCardScript.CurrentContainerType)
        {
            case Constants.CardContainerType.WastePile:
                // disable wastepile scrolling as dragging its cards can cause scrolling
                WastepileScript.Instance.DraggingCard = true;
                break;
            case Constants.CardContainerType.Foundation:
                List<GameObject> foundationCardList = selectedCardScript.Container.GetComponent<FoundationScript>().CardList;
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

        CardSounds.Instance.CardPressSound(hit.point, selectedCards.Count);

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
        if (selectedCardScript.CurrentContainerType == Constants.CardContainerType.Reactor)
        {
            // enable dragged reactor tokens holograms as they are off
            topSelectedCopyCardScript.EnableHologramImmediately();
            topSelectedCopyCardScript.Hologram = true;
        }

        // show everything that we can interact with
        showPossibleMoves.ShowMoves(selectedCardScript);

        changedHologramColor = false;
        wasOnMatch = false;
        changedSuitGlowColor = false;
        hidFoodHologram = false;
    }

    private bool TryToPlaceCards(GameObject newContainer)
    {
        if (!CardPlacement) return false;
        if (!newContainer.TryGetComponent<IGlow>(out var glowObject)
            || !glowObject.Glowing) return false;

        if (newContainer.CompareTag(Constants.Tags.card))
        {
            CardScript hitCardScript = newContainer.GetComponent<CardScript>();
            if (hitCardScript.GlowColor.ColorLevel == Constants.ColorLevel.Match)
            {
                matchCards.Match(selectedCardScript, hitCardScript, selectedCardsCopy[0]);
                return true;
            }
            newContainer = hitCardScript.Container;
        }

        if (!newContainer.TryGetComponent<ICardContainer>(out var cardContainer)) return false;

        // if the card was from a foundation and moved into a non foundation container
        bool checkGameOver = selectedCardScript.CurrentContainerType == Constants.CardContainerType.Foundation &&
            cardContainer.ContainerType != Constants.CardContainerType.Foundation;

        MoveAllSelectedCards(cardContainer.ContainerType, newContainer);
        Actions.MoveUpdate(checkGameOver);

        switch (cardContainer.ContainerType)
        {
            case Constants.CardContainerType.Reactor:
                CardSounds.Instance.CardToReactorSound(newContainer.transform.position);
                break;
            case Constants.CardContainerType.Foundation:
                CardSounds.Instance.CardStackSound(newContainer.transform.position, selectedCards.Count);
                break;
            default:
                throw new System.ArgumentException($"{newContainer} is an unexpected card container");
        }

        return true;
    }

    private void MoveAllSelectedCards(Constants.CardContainerType newContainerType, GameObject destination)
    {
        switch (selectedCards.Count)
        {
            case 0:
                Debug.LogError("tried to move an empty selected cards list");
                break;
            case 1:
                selectedCardScript.MoveCard(newContainerType, destination);
                break;
            default:
                selectedCardScript.MoveCard(newContainerType, destination, isStack: true, showHolo: false);

                int bottomCardCount = selectedCards.Count - 1;
                for (int i = 1; i < bottomCardCount; i++)
                    selectedCards[i].GetComponent<CardScript>().MoveCard(newContainerType, destination, isStack: true, showHolo: false);
                
                selectedCards[^1].GetComponent<CardScript>().MoveCard(newContainerType, destination, isStack: true, showHolo: true);
                break;
        }
    }

    private void DragSelectedCards(Vector3 position, RaycastHit2D hit)
    {
        if (DraggingStack)
        {
            foreach (GameObject card in selectedCardsCopy)
            {
                card.transform.position = position;
                position.y += GameValues.Transforms.draggedCardYOffset;
                position.z += GameValues.Transforms.draggedCardZOffset;
            }
        }
        else
        {
            selectedCardsCopy[0].transform.position = position;
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

    private void AutoPlace()
    {
        GameObject target = null;
        Vector2 endPosition = Vector2.zero;
        if (showPossibleMoves.matchTokensAreGlowing)
        {
            target = showPossibleMoves.cardMatch;
            endPosition = target.transform.position;
        }
        else if (showPossibleMoves.moveTokensAreGlowing)
        {
            target = showPossibleMoves.cardMoves[0];
            endPosition = target.transform.position;
            endPosition.y += 0.4f;
        }
        else if (showPossibleMoves.foundationIsGlowing &&
            (selectedCardScript.CurrentContainerType != Constants.CardContainerType.Foundation ||
            selectedCardScript.Container.GetComponent<FoundationScript>().CardList.Count != selectedCards.Count))
        {
            // are the cards not all the cards in a foundation?
            target = showPossibleMoves.foundationMoves[0];
            endPosition = target.transform.position;
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
            DraggingCard = false;
            return;
        }

        autoPlacing = true;
        StartCoroutine(MoveCards(target, endPosition));
    }

    private IEnumerator MoveCards(GameObject target, Vector2 endPosition)
    {
        if (AutoPlacement.SpeedValue == 0)
        {
            Vector3 newPosition = endPosition;
            foreach (GameObject card in selectedCardsCopy)
            {
                card.transform.position = newPosition;
                newPosition.y += GameValues.Transforms.draggedCardYOffset;
                newPosition.z += GameValues.Transforms.draggedCardZOffset;
            }
        }
        else if (selectedCardsCopy.Count == 1)
        {
            yield return Animate.MoveTransformSmoothDamp(selectedCardsCopy[0].transform,
                endPosition,
                AutoPlacement.SpeedValue);
        }
        else
        {
            Transform[] cardTransforms = new Transform[selectedCardsCopy.Count];
            for (int i = 0; i < selectedCardsCopy.Count; i++)
                cardTransforms[i] = selectedCardsCopy[i].transform;

            yield return Animate.MoveTransformsSmoothDamp(cardTransforms,
                endPosition,
                AutoPlacement.SpeedValue);
        }

        UpdateDragGlow(target);
        DragGlowRevert(isPlacing: true);
        TryToPlaceCards(target);
        DraggingCard = false;
        autoPlacing = false;
    }
}
