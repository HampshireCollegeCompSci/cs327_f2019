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

    private ShowPossibleMoves showPossibleMoves;
    private int _selectedCardsLayer, _gameplayLayer;

    // Initialize the singleton instance.
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            selectedCards = new(13);
            selectedCardsCopy = new(13);
            showPossibleMoves = new ShowPossibleMoves();
        }
        else if (Instance != this)
        {
            throw new System.ArgumentException("there should not already be an instance of this");
        }
    }

    void Start()
    {
        _selectedCardsLayer = SortingLayer.NameToID(Constants.selectedCardsSortingLayer);
        _gameplayLayer = SortingLayer.NameToID(Constants.gameplaySortingLayer);
    }

    void Update()
    {
        if (!Config.Instance.gamePaused)
        {
            if (dragOn)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    TryToPlaceCards(GetClick());
                    UnselectCards();
                }
                else
                {
                    DragSelectedTokens(GetClick());
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                if (InputStopped) return;

                TryToSelectCards(GetClick());

                if (dragOn)
                {
                    DragSelectedTokens(GetClick());
                }
            }
        }
    }

    public bool InputStopped
    {
        get => _inputStopped;
        set
        {
            if (!IsNextCycle)
            {
                _inputStopped = value;
            }
        }
    }

    public ShowPossibleMoves ShowPossibleMoves
    {
        get => showPossibleMoves;
    }

    private bool IsNextCycle
    {
        get => _isNextCycle;
        set
        {
            if (_isNextCycle)
            {
                _isNextCycle = value;
                InputStopped = value;
            }
            else
            {
                InputStopped = value;
                _isNextCycle = value;
            }
        }
    }

    public int SelectedCardsLayer
    {
        get => _selectedCardsLayer;
    }

    private int GameplayLayer
    {
        get => _gameplayLayer;
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
        if (startingGame || (!setAsValue && ((Config.Instance.actions + actionUpdate) >= Config.Instance.actionMax)))
        {
            doSaveState = false;
        }
        else
        {
            Config.Instance.moveCounter++;
        }

        bool wasInAlertThreshold = Config.Instance.actionMax - Config.Instance.actions <= Config.GameValues.turnAlertThreshold;

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
        bool isInAlertThreshold = Config.Instance.actionMax - Config.Instance.actions <= Config.GameValues.turnAlertThreshold;
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

    public void Match(CardScript card1Script, CardScript card2Script)
    {
        // stop the hologram fade in coroutine so that its alpha value doesn't change anymore
        card1Script.StopAllCoroutines();
        card2Script.StopAllCoroutines();

        GameObject comboHologram = selectedCardsCopy[0].GetComponent<CardScript>().HologramFood;
        comboHologram.transform.parent = null;

        Vector3 p = selectedCardsCopy[0].transform.position;
        p.z += 2;
        GameObject matchExplosion = Instantiate(matchPrefab, p, Quaternion.identity);
        matchExplosion.transform.localScale = new Vector3(Config.GameValues.matchExplosionScale, Config.GameValues.matchExplosionScale);

        card2Script.MoveCard(MatchedPileScript.Instance.gameObject);
        card1Script.MoveCard(MatchedPileScript.Instance.gameObject);

        int points = Config.GameValues.matchPoints + (Config.Instance.consecutiveMatches * Config.GameValues.scoreMultiplier);
        ScoreScript.Instance.UpdateScore(points);

        SoundEffectsController.Instance.FoodMatch(card1Script.CardSuitIndex);
        SpaceBabyController.Instance.BabyEat();

        StartCoroutine(FoodComboMove(comboHologram, matchExplosion));
        p.y += 0.2f;
        StartCoroutine(PointFade(points, p));
    }

    public void StartNextCycle(bool manuallyTriggered = false)
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

        if (!(manuallyTriggered && InputStopped)) // stops 2 NextCycles from happening at once
        {
            IsNextCycle = true;
            StartCoroutine(NextCycle());
        }
    }

    private RaycastHit2D GetClick()
    {
        return Physics2D.Raycast(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                                                                           Input.mousePosition.y,
                                                                           10)),
                                                               Vector2.zero);
    }

    private void TryToSelectCards(RaycastHit2D hit)
    {
        if (hit.collider == null) return;

        GameObject hitGameObject = hit.collider.gameObject;
        if (!hitGameObject.CompareTag(Constants.cardTag)) return;

        CardScript hitCardScript = hitGameObject.GetComponent<CardScript>();

        //if we click a card in the wastepile select it
        if (hitCardScript.Container.CompareTag(Constants.wastepileTag))
        {
            // all non-top wastepile tokens have their hitboxes disabled
            SelectCard(hitGameObject);
        }
        else if (hitCardScript.Container.CompareTag(Constants.reactorTag) &&
                 hitCardScript.Container.GetComponent<ReactorScript>().CardList[^1] == hitGameObject)
        {
            //if we click a card in a reactor
            SelectCard(hitGameObject);
        }
        else if (hitCardScript.Container.CompareTag(Constants.foundationTag))
        {
            //if we click a card in a foundation
            //if (!hitCardScript.isHidden()) return; // hidden cards have their hitboxes disabled
            SelectMultipleCards(hitGameObject);
        }
    }

    private void SelectCard(GameObject inputCard)
    {
        CardScript inputCardScript = inputCard.GetComponent<CardScript>();
        if (inputCardScript == null)
        {
            throw new System.ArgumentException("inputCard must be a gameObject that contains a CardScript");
        }

        if (inputCardScript.Container.CompareTag(Constants.wastepileTag))
        {
            WastepileScript.Instance.DraggingCard = true;
        }

        selectedCards.Add(inputCard);

        StartDragging();
    }

    private void SelectMultipleCards(GameObject inputCard)
    {
        CardScript inputCardScript = inputCard.GetComponent<CardScript>();
        if (inputCardScript == null || !inputCardScript.Container.CompareTag(Constants.foundationTag))
        {
            throw new System.ArgumentException("inputCard must be a gameObject that contains a CardScript that is from a foundation");
        }

        List<GameObject> cardList = inputCardScript.Container.GetComponent<FoundationScript>().CardList;

        for (int i = cardList.LastIndexOf(inputCard); i < cardList.Count; i++)
        {
            selectedCards.Add(cardList[i]);
        }

        StartDragging();
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

        // enable dragged reactor tokens holograms
        if (selectedCards.Count == 1)
        {
            selectedCardsCopy[0].GetComponent<CardScript>().Hologram = true;
        }

        // show any tokens (and reactors) that we can interact with
        showPossibleMoves.ShowMoves(selectedCards[0]);

        changedHologramColor = false;
        changedSuitGlowColor = false;
        hidFoodHologram = false;

        SoundEffectsController.Instance.CardPressSound();
    }

    private void TryToPlaceCards(RaycastHit2D hit)
    {
        // hit object is what the card will attempt to go into
        if (hit.collider == null) return;

        GameObject oldContainer = selectedCards[0].GetComponent<CardScript>().Container;
        GameObject newContainer = hit.collider.gameObject;

        if (newContainer == selectedCardsCopy[0].GetComponent<CardScript>().gameObject)
        {
            Debug.LogError("tried to place card on its own copy");
            return;
        }

        // if the destination is glowing, then something can happen
        switch (newContainer.tag)
        {
            case Constants.cardTag:
                CardScript hitCardScript = newContainer.GetComponent<CardScript>();
                if (hitCardScript.Glowing)
                {
                    if (hitCardScript.GlowLevel == Constants.matchHighlightColorLevel)
                    {
                        Match(selectedCards[0].GetComponent<CardScript>(), hitCardScript);
                        OtherActions(oldContainer.tag, hitCardScript.Container.tag, isMatch: true);
                    }
                    else
                    {
                        MoveAllSelectedCards(hitCardScript.Container);
                        OtherActions(oldContainer.tag, hitCardScript.Container.tag);
                    }
                }
                break;
            case Constants.foundationTag:
                if (newContainer.GetComponent<FoundationScript>().Glowing)
                {
                    MoveAllSelectedCards(newContainer);
                    OtherActions(oldContainer.tag, newContainer.tag);
                }
                break;
            case Constants.reactorTag:
                if (newContainer.GetComponent<ReactorScript>().Glowing)
                {
                    MoveAllSelectedCards(newContainer);
                    OtherActions(oldContainer.tag, newContainer.tag);
                }
                break;
            default:
                Debug.LogWarning("tried to place card in an invalid object");
                break;
        }
    }

    private void OtherActions(string oldContainer, string newContainer, bool isMatch = false)
    {
        // if the card has matched into a foundation card,
        // or if the card was from a foundation and moved into a non foundation container
        bool checkGameOver = (isMatch && newContainer.Equals(Constants.foundationTag)) ||
            (oldContainer.Equals(Constants.foundationTag) && !newContainer.Equals(Constants.foundationTag));

        UpdateActions(isMatch ? 0 : 1, checkGameOver: checkGameOver, isMatch: isMatch);

        if (!isMatch)
        {
            switch (newContainer)
            {
                case Constants.reactorTag:
                    SoundEffectsController.Instance.CardToReactorSound();
                    break;
                case Constants.foundationTag:
                    SoundEffectsController.Instance.CardStackSound();
                    break;
                default:
                    throw new System.Exception("this shouldn't happen");
            }
        }
    }

    private void MoveAllSelectedCards(GameObject destination)
    {
        if (selectedCards.Count > 1)
        {
            for (int i = 0; i < selectedCards.Count - 1; i++)
            {
                selectedCards[i].GetComponent<CardScript>().MoveCard(destination, isStack: true, showHolo: false);
            }
            selectedCards[^1].GetComponent<CardScript>().MoveCard(destination, isStack: true, showHolo: true);
        }
        else
        {
            selectedCards[0].GetComponent<CardScript>().MoveCard(destination);
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

        dragOn = false;
        InputStopped = false;

        showPossibleMoves.HideMoves();
    }

    private void DragSelectedTokens(RaycastHit2D hit)
    {
        Vector3 cardPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                                                                          Input.mousePosition.y,
                                                                          1));
        foreach (GameObject card in selectedCardsCopy)
        {
            card.transform.position = cardPosition;
            cardPosition.y += Config.GameValues.draggedCardOffset;
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
                hoveringOver.CompareTag(Constants.cardTag) &&
                hoveringOver.GetComponent<CardScript>().Glowing)
            {
                // change the dragged card hologram color to what it's hovering over
                byte hoverOverGlowLevel = hoveringOver.GetComponent<CardScript>().GlowLevel;
                selectedCardsCopy[^1].GetComponent<CardScript>().HologramColorLevel = hoverOverGlowLevel;
                // if it's a match
                if (hoverOverGlowLevel == 1)
                {
                    // if the hovering over card is not in the reactor
                    if (!hoveringOver.transform.parent.CompareTag(Constants.reactorTag))
                    {
                        // hide the hover over card food hologram
                        hoveringOver.GetComponent<CardScript>().Hologram = false;
                        hidFoodHologram = true;
                    }
                }

                changedHologramColor = true;
            }
            // else if we are hovering over a glowing reactor
            else if (showPossibleMoves.reactorIsGlowing &&
                hoveringOver.CompareTag(Constants.reactorTag) &&
                hoveringOver.GetComponent<ReactorScript>().Glowing)
            {
                selectedCardsCopy[0].GetComponent<CardScript>().HologramColorLevel = hoveringOver.GetComponent<ReactorScript>().GlowLevel;
                changedHologramColor = true;

                hoveringOver.GetComponent<ReactorScript>().ChangeSuitGlow(1);
                changedSuitGlowColor = true;
            }
            else if (showPossibleMoves.foundationIsGlowing && hoveringOver.CompareTag(Constants.foundationTag) &&
                hoveringOver.GetComponent<FoundationScript>().Glowing)
            {
                selectedCardsCopy[^1].GetComponent<CardScript>().HologramColorLevel = hoveringOver.GetComponent<FoundationScript>().GlowLevel;
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
            selectedCardsCopy[^1].GetComponent<CardScript>().HologramColorLevel = 0;
            changedHologramColor = false;
        }

        // if we where hovering over a matching glowing token
        if (hidFoodHologram)
        {
            hoveringOver.GetComponent<CardScript>().Hologram = true;
            hidFoodHologram = false;
        }
    }

    private IEnumerator FoodComboMove(GameObject comboHologram, GameObject matchExplosion)
    {
        // moves the object to the spacebaby
        //Vector3 target = baby.transform.position;
        //float initialDistance = Vector3.Distance(comboHologram.transform.position, target);
        //int speed = Config.config.cardsToReactorspeed / 2;
        //while (comboHologram.transform.position != target)
        //{
        //    fadeColor.a = Vector3.Distance(comboHologram.transform.position, target) / initialDistance;
        //    comboSR.color = fadeColor;
        //    comboHologram.transform.position = Vector3.MoveTowards(comboHologram.transform.position, target, Time.deltaTime * speed);
        //    yield return null;
        //}

        SpriteRenderer comboSR = comboHologram.GetComponent<SpriteRenderer>();
        Color fadeColor = Color.white;

        // wait for one frame so that the holograms's color won't be overwritten by 
        // the card script's hologram fade coroutine which will stopped during the wait
        yield return null;
        comboSR.color = fadeColor;

        yield return new WaitForSeconds(0.9f);
        Vector3 comboScale = comboSR.transform.localScale;
        while (fadeColor.a > 0)
        {
            comboScale += 0.04f * Time.deltaTime * Vector3.one;
            comboHologram.transform.localScale = comboScale;
            fadeColor.a -= Time.deltaTime * 0.8f;
            comboSR.color = fadeColor;
            yield return null;
        }

        //SoundController.Instance.CardMatchSound();
        Destroy(matchExplosion);
        Destroy(comboHologram);
    }

    private IEnumerator PointFade(int points, Vector3 position)
    {
        yield return new WaitForSeconds(0.2f);
        GameObject matchPointsEffect = Instantiate(matchPointsPrefab, position, Quaternion.identity, gameUI.transform);

        Text pointText = matchPointsEffect.GetComponent<Text>();
        pointText.text = $"+{points} ";

        Text comboText = matchPointsEffect.transform.GetChild(0).GetComponent<Text>();
        if (Config.Instance.consecutiveMatches > 1)
        {
            comboText.text = $"X{Config.Instance.consecutiveMatches} COMBO";
        }

        comboText.color = Config.GameValues.pointColor;
        pointText.color = Config.GameValues.pointColor;

        Vector3 pointsScale = new(0.2f, 0.2f, 0.2f);
        matchPointsEffect.transform.localScale = pointsScale;
        while (pointsScale.x < 1)
        {
            yield return null;
            pointsScale += 3f * Time.deltaTime * Vector3.one;
            matchPointsEffect.transform.localScale = pointsScale;
        }

        yield return new WaitForSeconds(0.5f);

        Color fadeColor = pointText.color;
        while (fadeColor.a > 0)
        {
            pointsScale += 0.3f * Time.deltaTime * Vector3.one;
            matchPointsEffect.transform.localScale = pointsScale;
            fadeColor.a -= Time.deltaTime * 0.9f;
            pointText.color = fadeColor;
            comboText.color = fadeColor;
            yield return null;
        }

        Destroy(matchPointsEffect);
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
        if (turnOnAlert || (checkAgain && !matchRelated && Config.Instance.actionMax - Config.Instance.actions == 1))
        {
            SoundEffectsController.Instance.AlertSound();
        }


        if (highAlertTurnedOn) // if the high alert was turned on during this check
        {
            // if the alert was not already on turn it on
            // or if the alert is already on and there is only 1 move left,
            // have the baby be angry and play the alert sound
            if (ActionCountScript.Instance.TurnSirenOn(2) ||
                (!matchRelated && Config.Instance.actionMax - Config.Instance.actions == 1))
            {
                SpaceBabyController.Instance.BabyReactorHigh();
            }
        }
        else if (turnOnAlert || checkAgain)
        {
            ActionCountScript.Instance.TurnSirenOn(1);
        }
        else // the action counter is not low so turn stuff off
        {
            ActionCountScript.Instance.TurnSirenOff();
        }
    }

    private bool CheckNextCycle()
    {
        if (Config.Instance.actions >= Config.Instance.actionMax)
        {
            StartNextCycle();
            return true;
        }

        return false;
    }

    private IEnumerator NextCycle()
    {
        SpaceBabyController.Instance.BabyActionCounter();

        foreach (FoundationScript foundationScript in foundationScripts)
        {
            if (foundationScript.CardList.Count == 0)
            {
                continue;
            }

            GameObject topFoundationCard = foundationScript.CardList[^1];
            CardScript topCardScript = topFoundationCard.GetComponent<CardScript>();

            foreach (ReactorScript reactorScript in reactorScripts)
            {
                if (topCardScript.CardSuitIndex != reactorScript.ReactorSuitIndex)
                {
                    continue;
                }

                topCardScript.Hologram = false;
                topFoundationCard.GetComponent<SpriteRenderer>().sortingLayerID = SelectedCardsLayer;
                topCardScript.Values.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingLayerID = SelectedCardsLayer;

                // immediately unhide the next possible top foundation card and start its hologram
                if (foundationScript.CardList.Count > 1)
                {
                    CardScript nextTopFoundationCard = foundationScript.CardList[^2].GetComponent<CardScript>();
                    if (nextTopFoundationCard.Hidden)
                    {
                        nextTopFoundationCard.NextCycleReveal();
                    }
                }

                Vector3 target = reactorScript.GetNextCardPosition();
                while (topFoundationCard.transform.position != target)
                {
                    topFoundationCard.transform.position = Vector3.MoveTowards(topFoundationCard.transform.position, target,
                        Time.deltaTime * Config.GameValues.cardsToReactorspeed);
                    yield return null;
                }

                topFoundationCard.GetComponent<SpriteRenderer>().sortingLayerID = GameplayLayer;
                topCardScript.Values.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingLayerID = GameplayLayer;

                SoundEffectsController.Instance.CardToReactorSound();
                topCardScript.MoveCard(reactorScript.gameObject, isCycle: true);

                if (Config.Instance.gameOver)
                {
                    Config.Instance.moveCounter += 1;
                    IsNextCycle = false;
                    yield break;
                }

                break;
            }
        }

        IsNextCycle = false;
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
