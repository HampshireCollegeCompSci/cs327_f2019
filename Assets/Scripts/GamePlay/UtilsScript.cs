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
            inputStopRequests = 0;
            showPossibleMoves = new ShowPossibleMoves();
        }
        else if (Instance != this)
        {
            throw new System.ArgumentException("there should not already be an instance of this");
        }
    }

    void Update()
    {
        if (!Config.Instance.gamePaused)
        {
            if (dragOn)
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (Input.GetMouseButtonUp(0))
                {
                    if (hit.collider != null)
                    {
                        TryToPlaceCards(hit);
                    }
                    UnselectCards();
                }
                else
                {
                    DragSelectedCards(hit);
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                if (InputStopped) return;
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

                if (hit.collider != null)
                {
                    TryToSelectCards(hit);
                }
            }
        }
    }

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

    public ShowPossibleMoves ShowPossibleMoves
    {
        get => showPossibleMoves;
    }

    public void UpdateActions(int actionUpdate, bool setAsValue = false, bool checkGameOver = false, bool startingGame = false, bool isMatch = false)
    {
        // so that during a game start consecutiveMatches is not set to 0 after being set from a saved game
        if (!startingGame)
        {
            if (isMatch)
            {
                Config.Instance.consecutiveMatches++;
            }
            else
            {
                Config.Instance.consecutiveMatches = 0;
            }
        }

        // so that a nextcycle trigger doesn't save the state before and after, we only need the after
        bool doSaveState = true;

        // detecting if the game is starting or if a nextcycle will be triggered
        if (startingGame || (!setAsValue && ((Config.Instance.actions + actionUpdate) >= Config.Instance.CurrentDifficulty.MoveLimit)))
        {
            doSaveState = false;
        }
        else
        {
            Config.Instance.moveCounter++;
        }

        bool wasInAlertThreshold = Config.Instance.CurrentDifficulty.MoveLimit - Config.Instance.actions <= GameValues.GamePlay.turnAlertThreshold;

        // loading a saved game triggers this
        if (startingGame)
        {
            Config.Instance.actions = actionUpdate;
        }
        // a nextcycle after it's done triggers this
        else if (setAsValue)
        {
            // if nextcycle caused a Game Over
            if (TryGameOver()) return;

            Config.Instance.actions = actionUpdate;
        }
        else if (actionUpdate == 0) // a match was made
        {
            // check if reactor alerts should be turned off
            if (wasInAlertThreshold)
            {
                Alert(false, true, true);
            }

            if (!TryGameOver()) // if a match didn't win the game
            {
                StateLoader.Instance.WriteState();
            }
            return;
        }
        else if (actionUpdate == -1) // a match undo
        {
            if (wasInAlertThreshold)
            {
                Alert(false, true, true);
            }
            StateLoader.Instance.WriteState();
            return;
        }
        else
        {
            Config.Instance.actions += actionUpdate;
        }

        ActionCountScript.Instance.UpdateActionText();

        if (CheckNextCycle()) return;

        // foundation moves trigger this as they are the only ones that can cause a gameover via winning
        // reactors trigger their own gameovers
        if (checkGameOver && TryGameOver()) return;

        if (doSaveState && !Config.Instance.gameOver)
        {
            StateLoader.Instance.WriteState();
        }

        // time to determine if the alert should be turned on
        bool isInAlertThreshold = Config.Instance.CurrentDifficulty.MoveLimit - Config.Instance.actions <= GameValues.GamePlay.turnAlertThreshold;
        if (!wasInAlertThreshold && !isInAlertThreshold)
        {
            // do nothing
        }
        else if (wasInAlertThreshold && isInAlertThreshold)
        {
            Alert(false, true);
        }
        else if (wasInAlertThreshold && !isInAlertThreshold)
        {
            Alert(false);
        }
        else if (!wasInAlertThreshold && isInAlertThreshold)
        {
            Alert(true);
        }
    }

    [SerializeField]
    private void MakeActionsMaxButton()
    {
        if (InputStopped || Config.Instance.gamePaused) return;
        Debug.Log("make actions max button");
        ActionCountScript.Instance.KnobDown();
        SoundEffectsController.Instance.VibrateMedium();
        StartNextCycle(manuallyTriggered: true);
    }

    private void StartNextCycle(bool manuallyTriggered = false)
    {
        if (Config.Instance.tutorialOn)
        {
            if (Config.Instance.nextCycleEnabled)
            {
                Config.Instance.nextCycleEnabled = false;
            }
            else
            {
                return;
            }
        }
        InputStopped = true;
        StartCoroutine(NextCycle(manuallyTriggered));
    }

    private void TryToSelectCards(RaycastHit2D hit)
    {
        GameObject hitGameObject = hit.collider.gameObject;
        if (!hitGameObject.CompareTag(Constants.Tags.card)) return;
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
        StartDragging();
        DragSelectedCards(hit);
    }

    private void StartDragging()
    {
        dragOn = true;
        InputStopped = true;

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
        showPossibleMoves.ShowMoves(selectedCards[0].GetComponent<CardScript>());

        changedHologramColor = false;
        changedSuitGlowColor = false;
        hidFoodHologram = false;

        SoundEffectsController.Instance.CardPressSound();
    }

    private void TryToPlaceCards(RaycastHit2D hit)
    {
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

        UpdateActions(isMatch ? 0 : 1, checkGameOver: checkGameOver, isMatch: isMatch);

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

        dragOn = false;
        InputStopped = false;

        showPossibleMoves.HideMoves();
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

        DragGlow(hit);
    }

    private void DragGlow(RaycastHit2D hit)
    {
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

    private void Alert(bool turnOnAlert, bool checkAgain = false, bool matchRelated = false)
    {
        bool highAlertTurnedOn = false;

        // if turning on the alert for the first time
        // or checking again if the previous move changed something
        if (turnOnAlert || checkAgain)
        {
            foreach (ReactorScript reactorScript in reactorScripts)
            {
                // if a nextcyle will overload the reactor
                if (reactorScript.OverLimitSoon())
                {
                    highAlertTurnedOn = true;
                }
                else if (checkAgain) // try turning the glow off just in case if it already on
                {
                    reactorScript.Alert = false;
                }
            }
        }
        else // we are done with the alert
        {
            MusicController.Instance.GameMusic();

            foreach (ReactorScript reactorScript in reactorScripts)
            {
                reactorScript.Alert = false;
            }
        }

        if (turnOnAlert || checkAgain)
        {
            MusicController.Instance.AlertMusic();
        }

        // if there is one move left
        if (turnOnAlert || (checkAgain && !matchRelated && Config.Instance.CurrentDifficulty.MoveLimit - Config.Instance.actions == 1))
        {
            SoundEffectsController.Instance.AlertSound();
        }


        if (highAlertTurnedOn) // if the high alert was turned on during this check
        {
            // if the alert was not already on turn it on
            // or if the alert is already on and there is only 1 move left,
            // have the baby be angry and play the alert sound
            if (ActionCountScript.Instance.TurnSirenOn(GameValues.AlertLevels.high) ||
                (!matchRelated && Config.Instance.CurrentDifficulty.MoveLimit - Config.Instance.actions == 1))
            {
                SpaceBabyController.Instance.BabyReactorHigh();
            }
        }
        else if (turnOnAlert || checkAgain)
        {
            ActionCountScript.Instance.TurnSirenOn(GameValues.AlertLevels.low);
        }
        else // the action counter is not low so turn stuff off
        {
            ActionCountScript.Instance.TurnSirenOff();
        }
    }

    private bool CheckNextCycle()
    {
        if (Config.Instance.actions >= Config.Instance.CurrentDifficulty.MoveLimit)
        {
            StartNextCycle();
            return true;
        }

        return false;
    }

    private IEnumerator NextCycle(bool manuallyTriggered)
    {
        SpaceBabyController.Instance.BabyActionCounter();

        foreach (FoundationScript foundationScript in foundationScripts)
        {
            if (foundationScript.CardList.Count == 0) continue;

            GameObject topFoundationCard = foundationScript.CardList[^1];
            CardScript topCardScript = topFoundationCard.GetComponent<CardScript>();
            ReactorScript reactorScript = reactorScripts[topCardScript.Card.Suit.Index];

            // turn off the moving cards hologram and make it appear in front of everything
            topCardScript.Hologram = false;
            topFoundationCard.GetComponent<SpriteRenderer>().sortingLayerID = Config.Instance.SelectedCardsLayer;
            topCardScript.Values.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingLayerID = Config.Instance.SelectedCardsLayer;

            // immediately unhide the next possible top foundation card and start its hologram
            if (foundationScript.CardList.Count > 1)
            {
                CardScript nextTopFoundationCard = foundationScript.CardList[^2].GetComponent<CardScript>();
                if (nextTopFoundationCard.Hidden)
                {
                    nextTopFoundationCard.NextCycleReveal();
                }
            }

            yield return Animate.SmoothstepTransform(topFoundationCard.transform,
                topFoundationCard.transform.position,
                reactorScript.GetNextCardPosition(),
                GameValues.AnimationDurataions.cardsToReactor);

            // set the sorting layer back to default
            topFoundationCard.GetComponent<SpriteRenderer>().sortingLayerID = Config.Instance.CardLayer;
            topCardScript.Values.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingLayerID = Config.Instance.CardLayer;

            SoundEffectsController.Instance.CardToReactorSound();
            topCardScript.MoveCard(Constants.CardContainerType.Reactor, reactorScript.gameObject, isCycle: true);

            // if the game is lost during the next cycle stop immediately
            if (Config.Instance.gameOver)
            {
                Config.Instance.moveCounter += 1;
                InputStopped = false;
                if (manuallyTriggered)
                {
                    ActionCountScript.Instance.KnobUp();
                }
                yield break;
            }
        }

        InputStopped = false;
        if (manuallyTriggered)
        {
            ActionCountScript.Instance.KnobUp();
        }
        UpdateActions(0, setAsValue: true);
    }

    private bool TryGameOver()
    {
        if (!Config.Instance.gameOver && AreFoundationsEmpty())
        {
            EndGame.Instance.GameOver(true);
            return true;
        }
        return false;
    }

    private bool AreFoundationsEmpty()
    {
        foreach (FoundationScript foundationScript in foundationScripts)
        {
            if (foundationScript.CardList.Count != 0)
            {
                return false;
            }
        }
        return true;
    }
}
