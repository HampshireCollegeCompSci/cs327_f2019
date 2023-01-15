using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UtilsScript : MonoBehaviour
{
    // Singleton instance.
    public static UtilsScript Instance;

    public GameObject[] reactors;
    public ReactorScript[] reactorScripts;

    public GameObject[] foundations;
    public FoundationScript[] foundationScripts;

    [SerializeField]
    private GameObject gameUI;

    [SerializeField]
    private List<GameObject> selectedCards, selectedCardsCopy;
    private CardScript topSelectedCopyCardScript;

    [SerializeField]
    private GameObject matchPrefab, matchPointsPrefab;

    [SerializeField]
    private bool dragOn;
    [SerializeField]
    private GameObject hoveringOver;
    [SerializeField]
    private bool changedHologramColor, changedSuitGlowColor, hidFoodHologram;

    [SerializeField]
    private bool _isNextCycle, _inputStopped;
    [SerializeField]
    private int inputStopRequests;

    private ShowPossibleMoves showPossibleMoves;

    // Initialize the singleton instance.
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            selectedCards = new(13);
            selectedCardsCopy = new(13);
            InputStopped = true;
            showPossibleMoves = new ShowPossibleMoves();
        }
        else if (Instance != this)
        {
            throw new System.ArgumentException("there should not already be an instance of this");
        }
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
                }
            }
            else
            {
                inputStopRequests--;
                if (inputStopRequests == 0)
                {
                    _inputStopped = false;
                }
            }
        }
    }

    void Update()
    {
        if (dragOn)
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (Input.GetMouseButtonUp(0))
            {
                TryToPlaceCards(hit);
                UnselectCards();
                showPossibleMoves.HideMoves();
                dragOn = false;
                InputStopped = false;
            }
            else
            {
                DragSelectedCards(hit);
            }
        }
        else if (Input.GetMouseButtonDown(0) && !InputStopped)
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (TrySelectCards(hit))
            {
                dragOn = true;
                InputStopped = true;
                SoundEffectsController.Instance.CardPressSound();
                DragSelectedCards(hit);
            }
        }
    }

    private bool TrySelectCards(RaycastHit2D hit)
    {
        if (hit.collider == null) return false;
        GameObject hitGameObject = hit.collider.gameObject;
        if (!hitGameObject.CompareTag(Constants.Tags.card)) return false;

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
        changedSuitGlowColor = false;
        hidFoodHologram = false;
        return true;
    }

    private void TryToPlaceCards(RaycastHit2D hit)
    {
        if (hit.collider == null) return;

        Constants.CardContainerType oldContainer = selectedCards[0].GetComponent<CardScript>().CurrentContainerType;
        // hit object is what the card will attempt to go into
        GameObject newContainer = hit.collider.gameObject;

        if (newContainer == selectedCardsCopy[0].GetComponent<CardScript>().gameObject)
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
                    if (hitCardScript.GlowColor.Equals(GameValues.Colors.Highlight.match))
                    {
                        Match(selectedCards[0].GetComponent<CardScript>(), hitCardScript);
                        OtherActions(oldContainer, hitCardScript.CurrentContainerType, isMatch: true);
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
                Debug.LogWarning("tried to place card in an invalid object");
                break;
        }
    }

    private void OtherActions(Constants.CardContainerType oldContainer, Constants.CardContainerType newContainer, bool isMatch = false)
    {
        // if the card has matched into a foundation card,
        // or if the card was from a foundation and moved into a non foundation container
        bool checkGameOver = (isMatch && newContainer == Constants.CardContainerType.Foundation) ||
            (oldContainer == Constants.CardContainerType.Foundation && newContainer != Constants.CardContainerType.Foundation);

        Actions.UpdateActions(isMatch ? 0 : 1, checkGameOver: checkGameOver, isMatch: isMatch);

        if (!isMatch)
        {
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
        Vector3 cardPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                                                                          Input.mousePosition.y,
                                                                          1));
        foreach (GameObject card in selectedCardsCopy)
        {
            card.transform.position = cardPosition;
            cardPosition.y += GameValues.Transforms.draggedCardYOffset;
            cardPosition.z -= 0.05f;
        }

        // glow time
        // if the tutorial is not on and there is no stuff glowing, stop
        if (!showPossibleMoves.AreThingsGlowing()) return;

        if (hit.collider != null)
        {
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
                // if it's a match
                if (hoveringOverCS.GlowColor.Equals(GameValues.Colors.Highlight.match))
                {
                    if (hoveringOverCS.CurrentContainerType != Constants.CardContainerType.Reactor)
                    {
                        // hide the hover over card food hologram
                        hoveringOverCS.Hologram = false;
                        hidFoodHologram = true;
                    }
                }
            }
            // else if we are hovering over a glowing reactor
            else if (showPossibleMoves.reactorIsGlowing &&
                hoveringOver.CompareTag(Constants.Tags.reactor))
            {
                ReactorScript hoveringOverRS = hoveringOver.GetComponent<ReactorScript>();
                if (!hoveringOverRS.Glowing) return;

                topSelectedCopyCardScript.HologramColor = hoveringOverRS.GlowColor;
                changedHologramColor = true;

                hoveringOverRS.ChangeSuitGlow(GameValues.Colors.Highlight.match);
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
        else
        {
            DragGlowRevert();
            hoveringOver = null;
        }
    }

    private void DragGlowRevert()
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
            topSelectedCopyCardScript.HologramColor = GameValues.Colors.Highlight.none;
            changedHologramColor = false;
        }

        // if we where hovering over a matching glowing token
        if (hidFoodHologram)
        {
            hoveringOver.GetComponent<CardScript>().Hologram = true;
            hidFoodHologram = false;
        }
    }

    private void Match(CardScript card1Script, CardScript card2Script)
    {
        // stop the hologram fade in coroutine so that its alpha value doesn't change anymore
        card1Script.StopAllCoroutines();
        card2Script.StopAllCoroutines();

        int points = GameValues.Points.matchPoints + (Config.Instance.consecutiveMatches * GameValues.Points.scoreMultiplier);
        StartCoroutine(MatchEffect(points));

        card2Script.MoveCard(Constants.CardContainerType.MatchedPile, MatchedPileScript.Instance.gameObject);
        card1Script.MoveCard(Constants.CardContainerType.MatchedPile,MatchedPileScript.Instance.gameObject);

        ScoreScript.Instance.UpdateScore(points);
        SoundEffectsController.Instance.FoodMatch(card1Script.Card.Suit);
        SpaceBabyController.Instance.BabyEat();
    }

    private IEnumerator MatchEffect(int points)
    {
        // get the hologram object and make sure it stays where it is
        GameObject comboHologram = selectedCardsCopy[0].GetComponent<CardScript>().HologramFood;
        comboHologram.transform.parent = null;
        SpriteRenderer comboSR = comboHologram.GetComponent<SpriteRenderer>();
        comboSR.color = Color.white;

        Vector3 position = selectedCardsCopy[0].transform.position;
        GameObject matchExplosion = Instantiate(matchPrefab, position, Quaternion.identity);
        matchExplosion.transform.localScale = new Vector3(GameValues.Transforms.matchExplosionScale, GameValues.Transforms.matchExplosionScale);

        // instantiate the points slightly below
        position.y += 0.25f;
        GameObject matchPointsEffect = Instantiate(matchPointsPrefab, position, Quaternion.identity, gameUI.transform);

        // set the points readout
        Text pointText = matchPointsEffect.GetComponent<Text>();
        pointText.text = $"+{points} ";
        // set the combo readout
        Text comboText = matchPointsEffect.transform.GetChild(0).GetComponent<Text>();

        comboText.text = (Config.Instance.consecutiveMatches > 1) switch
        {
            true => $"X{Config.Instance.consecutiveMatches} COMBO",
            false => "",
        };

        // set the color of the points and combo
        pointText.color = GameValues.Colors.pointColor;
        comboText.color = GameValues.Colors.pointColor;

        // get the start and end values for the points
        Color pointFadeColor = pointText.color;
        Vector3 pointScaleStart = Vector3.zero;
        Vector3 pointScaleEnd = Vector3.one;

        // lerp scale up and fade in only the points
        float duration = GameValues.AnimationDurataions.comboPointsFadeIn;
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;

            matchPointsEffect.transform.localScale = Vector3.Lerp(pointScaleStart, pointScaleEnd, t);

            pointFadeColor.a = Mathf.Lerp(0, 1, t);
            pointText.color = pointFadeColor;
            comboText.color = pointFadeColor;

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(GameValues.AnimationDurataions.comboWait);

        // how much the scale up will be
        Vector2 scaleMulti = new(1.2f, 1.2f);

        // get the start and end values for the combo object
        Color comboFadeColor = comboSR.color;
        Vector3 comboScaleStart = comboSR.transform.localScale;
        Vector3 comboScaleEnd = Vector3.Scale(comboScaleStart, scaleMulti);

        // get the start and end values for the points
        pointScaleStart = pointScaleEnd;
        pointScaleEnd = Vector3.Scale(pointScaleStart, scaleMulti);

        // lerp scale up and fade out both points and combo object
        duration = GameValues.AnimationDurataions.comboFadeOut;
        timeElapsed = 0;
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            float alpha = Mathf.Lerp(1, 0, t);
            // scale up
            matchPointsEffect.transform.localScale = Vector3.Lerp(pointScaleStart, pointScaleEnd, t);
            comboSR.transform.localScale = Vector3.Lerp(comboScaleStart, comboScaleEnd, t); ;
            // point color
            pointFadeColor.a = alpha;
            pointText.color = pointFadeColor;
            comboText.color = pointFadeColor;
            // combo object color
            comboFadeColor.a = alpha;
            comboSR.color = comboFadeColor;

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        // destroy the temporary objects
        Destroy(matchPointsEffect);
        Destroy(matchExplosion);
        Destroy(comboHologram);
    }
}
