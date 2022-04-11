using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UtilsScript : MonoBehaviour
{
    public List<GameObject> selectedCards;
    private List<GameObject> selectedCardsCopy;
    private int selectedCardsCopyCount;

    public GameObject matchPrefab;
    public GameObject matchPointsPrefab;

    public GameObject[] reactors;
    public GameObject[] foundations;

    public GameObject gameUI;
    public Text score;
    private bool dragOn;
    private bool draggingWastepile = false;

    private bool inputStopped = false;
    private bool isNextCycle;

    private GameObject hoveringOver;
    private bool changedHologramColor;
    private bool changedSuitGlowColor;
    private bool hidFoodHologram;

    // Singleton instance.
    public static UtilsScript Instance = null;

    // Initialize the singleton instance.
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            throw new System.ArgumentException("there should not already be an instance of this");
        }
    }

    void Start()
    {
        selectedCardsCopy = new List<GameObject>();
        selectedCardsLayer = SortingLayer.NameToID(Constants.selectedCardsSortingLayer);
        gameplayLayer = SortingLayer.NameToID(Constants.gameplaySortingLayer);
    }


    private int selectedCardsLayer;
    public int SelectedCardsLayer
    {
        get { return selectedCardsLayer; }
    }

    private int gameplayLayer;
    public int GameplayLayer
    {
        get { return gameplayLayer; }
    }


    void Update()
    {
        if (!Config.Instance.gamePaused && !Config.Instance.tutorialOn && !Config.Instance.gameOver)
        {
            if (dragOn)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    TryToPlaceCards(GetClick());
                    ShowPossibleMoves.Instance.HideMoves();
                    UnselectCards();
                }
                else
                {
                    DragSelectedTokens(GetClick());
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                if (IsInputStopped()) return;

                TryToSelectCards(GetClick());

                if (dragOn)
                {
                    DragSelectedTokens(GetClick());
                }
            }
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
        if (hitCardScript.container.CompareTag(Constants.wastepileTag))
        {
            // all non-top wastepile tokens have their hitboxes disabled
            SelectCard(hitGameObject);
        }
        else if (hitCardScript.container.CompareTag(Constants.reactorTag) &&
                 hitCardScript.container.GetComponent<ReactorScript>().cardList[0] == hitGameObject)
        {
            //if we click a card in a reactor
            SelectCard(hitGameObject);
        }
        else if (hitCardScript.container.CompareTag(Constants.foundationTag))
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

        if (inputCardScript.container.CompareTag(Constants.wastepileTag))
        {
            WastepileScript.Instance.DraggingCard(inputCard, true);
            draggingWastepile = true;
        }

        selectedCards.Add(inputCard);
        inputCardScript.SetSelected(true);

        StartDragging();
    }

    private void SelectMultipleCards(GameObject inputCard)
    {
        CardScript inputCardScript = inputCard.GetComponent<CardScript>();
        if (inputCardScript == null || !inputCardScript.container.CompareTag(Constants.foundationTag))
        {
            throw new System.ArgumentException("inputCard must be a gameObject that contains a CardScript that is from a foundation");
        }

        FoundationScript inputCardFoundation = inputCardScript.container.GetComponent<FoundationScript>();

        for (int i = inputCardFoundation.cardList.IndexOf(inputCard); i >= 0; i--)
        {
            selectedCards.Add(inputCardFoundation.cardList[i]);
            inputCardFoundation.cardList[i].GetComponent<CardScript>().SetSelected(true);
        }

        StartDragging();
    }

    private void TryToPlaceCards(RaycastHit2D hit)
    {
        if (hit.collider == null) return;

        GameObject selectedContainer = selectedCards[0].GetComponent<CardScript>().container;

        switch (selectedContainer.tag)
        {
            case Constants.wastepileTag:
                WastepileScript.Instance.ProcessAction(hit.collider.gameObject);
                break;
            case Constants.foundationTag:
                selectedContainer.GetComponent<FoundationScript>().ProcessAction(hit.collider.gameObject);
                break;
            case Constants.reactorTag:
                selectedContainer.GetComponent<ReactorScript>().ProcessAction(hit.collider.gameObject);
                break;
            default:
                Debug.Log("TryToPlaceCards detected a selected card that had a weird container");
                break;
        }
    }

    private void UnselectCards()
    {
        if (draggingWastepile)
        {
            WastepileScript.Instance.DraggingCard(selectedCards[0], false);
            draggingWastepile = false;
        }

        for (int i = 0; i < selectedCards.Count; i++)
        {
            selectedCards[i].GetComponent<CardScript>().SetSelected(false);
        }
        selectedCards.Clear();

        for (int i = 0; i < selectedCardsCopyCount; i++)
        {
            Destroy(selectedCardsCopy[i]);
        }
        selectedCardsCopy.Clear();

        dragOn = false;
        SetInputStopped(false);
    }

    private void StartDragging()
    {
        dragOn = true;
        SetInputStopped(true);

        // make a copy of the selected cards to move around
        GameObject cardCopy;
        foreach (GameObject card in selectedCards)
        {
            cardCopy = Instantiate(card, card.transform.position, Quaternion.identity);
            cardCopy.GetComponent<CardScript>().MakeVisualOnly();
            selectedCardsCopy.Add(cardCopy);
        }
        selectedCardsCopyCount = selectedCardsCopy.Count;

        // enable dragged reactor tokens holograms
        if (selectedCards.Count == 1 && selectedCards[0].GetComponent<CardScript>().container.CompareTag(Constants.reactorTag))
        {
            selectedCardsCopy[0].GetComponent<CardScript>().ShowHologram();
        }

        // show any tokens (and reactors) that we can interact with
        ShowPossibleMoves.Instance.ShowMoves(selectedCards[0]);

        changedHologramColor = false;
        changedSuitGlowColor = false;
        hidFoodHologram = false;

        SoundEffectsController.Instance.CardPressSound();
    }

    private void DragSelectedTokens(RaycastHit2D hit)
    {
        // move the bottom token to our input position
        selectedCardsCopy[0].transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                                                                                 Input.mousePosition.y,
                                                                                 1));
        Vector3 lastCardsPosition = selectedCardsCopy[0].transform.position;
        // have the rest of the tokens appear above it
        for (int i = 1; i < selectedCardsCopyCount; i++)
        {
            selectedCardsCopy[i].transform.position = new Vector3(lastCardsPosition.x,
                                                                  lastCardsPosition.y + Config.GameValues.draggedCardOffset,
                                                                  lastCardsPosition.z - 0.05f);
            lastCardsPosition = selectedCardsCopy[i].transform.position;
        }

        DragGlow(hit);
    }

    private void DragGlow(RaycastHit2D hit)
    {
        // if there is no stuff glowing, stop
        if (!ShowPossibleMoves.Instance.AreThingsGlowing()) return;

        if (hit.collider != null)
        {
            // are we still hovering over the same object
            if (hit.collider.gameObject == hoveringOver) return;

            DragGlowRevert();
            hoveringOver = hit.collider.gameObject;

            // if we are hovering over a glowing token
            if (ShowPossibleMoves.Instance.AreCardsGlowing() && hoveringOver.CompareTag(Constants.cardTag) &&
                hoveringOver.GetComponent<CardScript>().IsGlowing())
            {
                // change the dragged token hologram color to what it's hovering over and check if it was a match
                if (selectedCardsCopy[selectedCardsCopyCount - 1].GetComponent<CardScript>().ChangeHologram(hoveringOver.GetComponent<CardScript>().GetGlowColor()))
                {
                    // if the hovering over token is not in the reactor
                    if (!hoveringOver.transform.parent.CompareTag(Constants.reactorTag))
                    {
                        // hide the hover over tokens food hologram
                        hoveringOver.GetComponent<CardScript>().HideHologram();
                        hidFoodHologram = true;
                    }
                }

                changedHologramColor = true;
            }
            // else if we are hovering over a glowing reactor
            else if (ShowPossibleMoves.Instance.reactorIsGlowing && hoveringOver.CompareTag(Constants.reactorTag) &&
                hoveringOver.GetComponent<ReactorScript>().IsGlowing())
            {
                selectedCardsCopy[0].GetComponent<CardScript>().ChangeHologram(hoveringOver.GetComponent<ReactorScript>().GetGlowColor());
                changedHologramColor = true;

                hoveringOver.GetComponent<ReactorScript>().ChangeSuitGlow(new Color(0, 1, 0, 0.5f));
                changedSuitGlowColor = true;
            }
            else if (ShowPossibleMoves.Instance.foundationIsGlowing && hoveringOver.CompareTag(Constants.foundationTag) &&
                hoveringOver.GetComponent<FoundationScript>().IsGlowing())
            {
                selectedCardsCopy[selectedCardsCopyCount - 1].GetComponent<CardScript>().ChangeHologram(hoveringOver.GetComponent<FoundationScript>().GetGlowColor());
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
            selectedCardsCopy[selectedCardsCopyCount - 1].GetComponent<CardScript>().ChangeHologram(Color.white);
            changedHologramColor = false;
        }

        // if we where hovering over a matching glowing token
        if (hidFoodHologram)
        {
            hoveringOver.GetComponent<CardScript>().ShowHologram();
            hidFoodHologram = false;
        }
    }

    public void Match(GameObject card1, GameObject card2)
    {
        CardScript card1Script = card1.GetComponent<CardScript>();
        CardScript card2Script = card2.GetComponent<CardScript>();

        GameObject comboHologram = selectedCardsCopy[0].GetComponent<CardScript>().hologramFood;
        comboHologram.transform.parent = null;
        comboHologram.GetComponent<SpriteRenderer>().color = Color.white;

        Vector3 p = selectedCardsCopy[0].transform.position;
        p.z += 2;
        GameObject matchExplosion = Instantiate(matchPrefab, p, Quaternion.identity);
        matchExplosion.transform.localScale = new Vector3(Config.GameValues.matchExplosionScale, Config.GameValues.matchExplosionScale);

        card2Script.MoveCard(MatchedPileScript.Instance.gameObject);
        card1Script.MoveCard(MatchedPileScript.Instance.gameObject);

        int points = Config.GameValues.matchPoints + (Config.Instance.consecutiveMatches * Config.GameValues.scoreMultiplier);
        UpdateScore(points);
        UpdateActions(0, isMatch: true);

        SoundEffectsController.Instance.FoodMatch(card1Script.suit);
        SpaceBabyController.Instance.BabyEat();

        StartCoroutine(FoodComboMove(comboHologram, matchExplosion));
        StartCoroutine(PointFade(points, p));
    }

    private IEnumerator FoodComboMove(GameObject comboHologram, GameObject matchExplosion)
    {
        yield return new WaitForSeconds(0.3f);
        SpriteRenderer comboSR = comboHologram.GetComponent<SpriteRenderer>();
        Color fadeColor = new Color(1, 1, 1, 1);

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

        Vector3 initialScale = comboSR.transform.localScale;
        float scale = 1;
        while (fadeColor.a > 0)
        {
            yield return new WaitForSeconds(0.01f);
            scale += 0.01f;
            comboHologram.transform.localScale = initialScale * scale;
            fadeColor.a -= 0.01f;
            comboSR.color = fadeColor;

            if (Config.Instance.gamePaused)
            {
                Destroy(matchExplosion);
                Destroy(comboHologram);
                yield break;
            }
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
        pointText.text = "+" + points.ToString();

        Text comboText = matchPointsEffect.transform.GetChild(0).GetComponent<Text>();
        if (Config.Instance.consecutiveMatches > 1)
        {
            comboText.text = "X" + Config.Instance.consecutiveMatches.ToString() + " COMBO";
        }

        comboText.color = Config.GameValues.pointColor;
        pointText.color = Config.GameValues.pointColor;

        float scale = 1;
        while (scale < 1.5)
        {
            yield return new WaitForSeconds(0.02f);
            scale += 0.01f;
            matchPointsEffect.transform.localScale = Vector3.one * scale;

            if (Config.Instance.gamePaused)
            {
                Destroy(matchPointsEffect);
                yield break;
            }
        }

        Color fadeColor = pointText.color;
        while (fadeColor.a > 0)
        {
            yield return new WaitForSeconds(0.02f);
            scale += 0.02f;
            matchPointsEffect.transform.localScale = Vector3.one * scale;
            fadeColor.a -= 0.05f;
            pointText.color = fadeColor;
            comboText.color = fadeColor;

            if (Config.Instance.gamePaused)
            {
                Destroy(matchPointsEffect);
                yield break;
            }
        }

        Destroy(matchPointsEffect);
    }

    public void UpdateScore(int addScore, bool setAsValue = false)
    {
        if (setAsValue)
        {
            Config.Instance.score = addScore;
        }
        else
        {
            Config.Instance.score += addScore;
        }

        score.text = Config.Instance.score.ToString();
    }

    public void UpdateActions(int actionUpdate, bool setAsValue = false, bool checkGameOver = false, bool startingGame = false, bool isMatch = false)
    {
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

        // detecting if a nextcycle will be triggered
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

        if (doSaveState)
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

    private void Alert(bool turnOnAlert, bool checkAgain = false, bool matchRelated = false)
    {
        bool highAlertTurnedOn = false;

        // if turning on the alert for the first time
        // or checking again if the previous move changed something
        if (turnOnAlert || checkAgain)
        {
            foreach (GameObject reactor in reactors)
            {
                // if a nextcyle will overload the reactor
                if (reactor.GetComponent<ReactorScript>().CountReactorCard() +
                    reactor.GetComponent<ReactorScript>().GetIncreaseOnNextCycle() >= Config.Instance.maxReactorVal)
                {
                    reactor.GetComponent<ReactorScript>().AlertOn();
                    highAlertTurnedOn = true;
                }
                else if (checkAgain) // try turning the glow off just in case if it already on
                    reactor.GetComponent<ReactorScript>().AlertOff();
            }
        }
        else // we are done with the alert
        {
            MusicController.Instance.GameMusic();

            foreach (GameObject reactor in reactors)
                reactor.GetComponent<ReactorScript>().AlertOff();
        }

        if (turnOnAlert || checkAgain)
            MusicController.Instance.AlertMusic();

        // if there is one move left
        if (turnOnAlert || (checkAgain && !matchRelated && Config.Instance.actionMax - Config.Instance.actions == 1))
            SoundEffectsController.Instance.AlertSound();


        if (highAlertTurnedOn) // if the high alert was turned on during this check
        {
            // if the alert was not already on turn it on
            // or if the alert is already on and there is only 1 move left,
            // have the baby be angry and play the alert sound
            if (ActionCountScript.Instance.TurnSirenOn(2) ||
                (!matchRelated && Config.Instance.actionMax - Config.Instance.actions == 1))
            {
                SpaceBabyController.Instance.BabyAngry();
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

    // moves all of the top foundation cards into their appropriate reactors
    public void StartNextCycle(bool manuallyTriggered = false)
    {
        if (!(manuallyTriggered && IsInputStopped())) // stops 2 NextCycles from happening at once
        {
            SetInputStopped(true, nextCycle: true);
            StartCoroutine(NextCycle());
        }
    }

    private IEnumerator NextCycle()
    {
        SpaceBabyController.Instance.BabyActionCounter();

        FoundationScript currentFoundation;
        GameObject topFoundationCard;
        CardScript topCardScript;

        foreach (GameObject foundation in foundations)
        {
            currentFoundation = foundation.GetComponent<FoundationScript>();
            if (currentFoundation.cardList.Count != 0)
            {
                topFoundationCard = currentFoundation.cardList[0];
                topCardScript = topFoundationCard.GetComponent<CardScript>();

                foreach (GameObject reactor in reactors)
                {
                    if (topCardScript.suit == reactor.GetComponent<ReactorScript>().suit)
                    {
                        topCardScript.HideHologram();
                        topFoundationCard.GetComponent<SpriteRenderer>().sortingLayerID = SelectedCardsLayer;
                        topCardScript.values.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingLayerID = SelectedCardsLayer;

                        // immediately unhide the next possible top foundation card and start its hologram
                        if (currentFoundation.cardList.Count > 1)
                        {
                            CardScript nextTopFoundationCard = currentFoundation.cardList[1].GetComponent<CardScript>();
                            if (nextTopFoundationCard.IsHidden)
                            {
                                nextTopFoundationCard.SetFoundationVisibility(true, isNotForNextCycle: false);
                                nextTopFoundationCard.ShowHologram();
                            }
                        }

                        Vector3 target = reactor.GetComponent<ReactorScript>().GetNextCardPosition();
                        while (topFoundationCard.transform.position != target)
                        {
                            topFoundationCard.transform.position = Vector3.MoveTowards(topFoundationCard.transform.position, target,
                                Time.deltaTime * Config.GameValues.cardsToReactorspeed);
                            yield return null;
                        }

                        topFoundationCard.GetComponent<SpriteRenderer>().sortingLayerID = GameplayLayer;
                        topCardScript.values.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingLayerID = GameplayLayer;

                        SoundEffectsController.Instance.CardToReactorSound();
                        topCardScript.MoveCard(reactor, isCycle: true);

                        if (Config.Instance.gameOver)
                        {
                            Config.Instance.moveCounter += 1;
                            yield break;
                        }

                        break;
                    }
                }
            }
        }

        SetInputStopped(false, nextCycle: true);
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
        foreach (GameObject foundation in UtilsScript.Instance.foundations)
        {
            if (foundation.GetComponent<FoundationScript>().cardList.Count != 0)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsInputStopped()
    {
        return inputStopped;
    }

    public void SetInputStopped(bool setTo, bool nextCycle = false)
    {
        if (nextCycle)
        {
            isNextCycle = setTo;
        }

        if (isNextCycle && nextCycle)
        {
            inputStopped = setTo;
        }
        else if (!isNextCycle)
        {
            inputStopped = setTo;
        }
    }

    public bool IsDragging()
    {
        return dragOn;
    }

    public void ManualGameOver()
    {
        EndGame.Instance.GameOver(true);
    }

    public void ManualGameWin()
    {
        if (Config.Instance.gamePaused) return;

        EndGame.Instance.GameOver(true);
        Config.Instance.matchCounter = 26;
    }
}
