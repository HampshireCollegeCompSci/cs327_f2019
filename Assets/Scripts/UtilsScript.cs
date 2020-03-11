using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class UtilsScript : MonoBehaviour
{
    public static UtilsScript global; //Creates a new instance if one does not yet exist
    public List<GameObject> selectedCards;
    private List<GameObject> selectedCardsCopy;
    public GameObject matchedPile;
    public GameObject matchPrefab;
    public GameObject matchPointsPrefab;

    public GameObject errorImage;
    public GameObject gameUI;
    public GameObject scoreBox;
    public GameObject moveCounter;
    public SoundController soundController;
    public int indexCounter;
    private bool dragOn;
    private bool draggingWastepile = false;
    private GameObject wastePile;

    private bool inputStopped = false;
    public bool isMatching = false;

    private int selectedLayer;
    private int gameplayLayer;

    public GameObject baby;
    public int matchPoints;
    public int emptyReactorPoints;
    public int PerfectGamePoints;

    private GameObject hoveringOver;
    private bool changedHologramColor;
    private bool changedSuitGlowColor;
    private bool hidFoodHologram;
    private bool matchTokensAreGlowing;
    private bool moveTokensAreGlowing;
    private bool reactorIsGlowing;
    private bool foundationIsGlowing;

    public void SetCards()
    {
        matchedPile = GameObject.Find("MatchedPile");
        gameUI = GameObject.Find("GameUI");
        soundController = GameObject.Find("Sound").GetComponent<SoundController>();
        wastePile = GameObject.Find("Scroll View");
        selectedLayer = SortingLayer.NameToID("SelectedCards");
        gameplayLayer = SortingLayer.NameToID("Gameplay");
        baby = GameObject.Find("SpaceBaby");
    }

    void Awake()
    {
        if (global == null)
        {
            //DontDestroyOnLoad(gameObject); //makes instance persist across scenes
            global = this;
        }
        else if (global != this)
        {
            Destroy(gameObject); //deletes copies of global which do not need to exist, so right version is used to get info from
        }
    }

    void Start()
    {
        selectedCardsCopy = new List<GameObject>();
        baby = GameObject.FindWithTag("Baby");
        matchPoints = Config.config.matchPoints;
        emptyReactorPoints = Config.config.emptyReactorPoints;
        PerfectGamePoints = Config.config.perfectGamePoints;
        UndoScript.undoScript.utils = gameObject.GetComponent<UtilsScript>();
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex != 2)
            return;

        if (!Config.config.gameOver && !Config.config.gamePaused && !Config.config.tutorialOn)
        {
            if (dragOn)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    TryToPlaceCards(GetClick());
                    ShowPossibleMoves.showPossibleMoves.HideMoves();
                    UnselectCards();
                }
                else
                    DragSelectedTokens(GetClick());
            }
            else if (Input.GetMouseButtonDown(0))
            {
                if (IsInputStopped())
                    return;

                TryToSelectCards(GetClick());

                if (dragOn)
                    DragSelectedTokens(GetClick());
            }
        }
    }

    public RaycastHit2D GetClick()
    {
        return Physics2D.Raycast(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                                                                           Input.mousePosition.y,
                                                                           10)),
                                                               Vector2.zero);
    }

    private void TryToSelectCards(RaycastHit2D hit)
    {
        if (hit.collider == null)
            return;

        GameObject hitGameObject = hit.collider.gameObject;
        if (!hitGameObject.CompareTag("Card"))
        {
            if (hitGameObject.CompareTag("Baby"))
                baby.GetComponent<SpaceBabyController>().BabyHappyAnim();
            return;
        }

        CardScript hitCardScript = hitGameObject.GetComponent<CardScript>();

        //if we click a card in the wastepile select it
        if (hitCardScript.container.CompareTag("Wastepile"))
            // all non-top wastepile tokens have their hitboxes disabled
            //if (hitCardScript.container.GetComponent<WastepileScript>().cardList[0] == hitGameObject)
            SelectCard(hitGameObject);

        //if we click a card in a reactor
        else if (hitCardScript.container.CompareTag("Reactor") &&
                 hitCardScript.container.GetComponent<ReactorScript>().cardList[0] == hitGameObject)
            SelectCard(hitGameObject);

        //if we click a card in a foundation
        else if (hitCardScript.container.CompareTag("Foundation"))
            //if (!hitCardScript.isHidden()) // hidden cards have their hitboxes disabled
            SelectMultipleCards(hitGameObject);
    }

    private void SelectCard(GameObject inputCard)
    {
        CardScript inputCardScript = inputCard.GetComponent<CardScript>();
        if (inputCardScript == null)
            throw new System.ArgumentException("inputCard must be a gameObject that contains a CardScript");

        if (inputCardScript.container.CompareTag("Wastepile"))
        {
            inputCardScript.container.GetComponent<WastepileScript>().DraggingCard(inputCard, true);
            draggingWastepile = true;
        }

        selectedCards.Add(inputCard);
        inputCardScript.SetSelected(true);

        StartDragging();
    }

    private void SelectMultipleCards(GameObject inputCard)
    {
        CardScript inputCardScript = inputCard.GetComponent<CardScript>();
        if (inputCardScript == null || !inputCardScript.container.CompareTag("Foundation"))
            throw new System.ArgumentException("inputCard must be a gameObject that contains a CardScript that is from a foundation");

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
        if (hit.collider == null)
            return;

        GameObject selectedContainer = selectedCards[0].GetComponent<CardScript>().container;

        if (selectedContainer.CompareTag("Wastepile"))
            selectedContainer.GetComponent<WastepileScript>().ProcessAction(hit.collider.gameObject);
        else if (selectedContainer.CompareTag("Foundation"))
            selectedContainer.GetComponent<FoundationScript>().ProcessAction(hit.collider.gameObject);
        else if (selectedContainer.CompareTag("Reactor"))
            selectedContainer.GetComponent<ReactorScript>().ProcessAction(hit.collider.gameObject);
        else
            Debug.Log("TryToPlaceCards detected a selected card that had a weird container");
    }

    private void UnselectCards()
    {
        if (draggingWastepile)
        {
            wastePile.GetComponent<WastepileScript>().DraggingCard(selectedCards[0], false);
            draggingWastepile = false;
        }

        for (int i = 0; i < selectedCards.Count; i++)
            selectedCards[i].GetComponent<CardScript>().SetSelected(false);
        selectedCards.Clear();

        for (int i = 0; i < selectedCardsCopy.Count; i++)
            Destroy(selectedCardsCopy[i]);
        selectedCardsCopy.Clear();

        dragOn = false;
        SetInputStopped(false);
    }

    private void StartDragging()
    {
        dragOn = true;
        SetInputStopped(true);

        // make a copy of the selected cards to move around
        GameObject newGameObject;
        int cardsCount = selectedCards.Count;
        for (int i = 0; i < cardsCount; i++)
        {
            newGameObject = Instantiate(selectedCards[i], selectedCards[i].transform.position, Quaternion.identity);
            newGameObject.GetComponent<CardScript>().MakeVisualOnly();
            selectedCardsCopy.Add(newGameObject);
        }

        // enable dragged reactor tokens holograms
        if (cardsCount == 1 && selectedCards[0].GetComponent<CardScript>().container.CompareTag("Reactor"))
            selectedCardsCopy[0].GetComponent<CardScript>().ShowHologram();

        // show any tokens (and reactors) that we can interact with
        ShowPossibleMoves.showPossibleMoves.ShowMoves(selectedCards[0]);

        changedHologramColor = false;
        changedSuitGlowColor = false;
        hidFoodHologram = false;

        if (ShowPossibleMoves.showPossibleMoves.cardMatches.Count != 0)
            matchTokensAreGlowing = true;
        else
            matchTokensAreGlowing = false;

        if (ShowPossibleMoves.showPossibleMoves.cardMoves.Count != 0)
            moveTokensAreGlowing = true;
        else
            moveTokensAreGlowing = false;

        if (ShowPossibleMoves.showPossibleMoves.reactorMove != null)
            reactorIsGlowing = true;
        else
            reactorIsGlowing = false;

        if (ShowPossibleMoves.showPossibleMoves.foundationMoves.Count != 0)
            foundationIsGlowing = true;
        else
            foundationIsGlowing = false;

        soundController.CardPressSound();
    }

    private void DragSelectedTokens(RaycastHit2D hit)
    {
        // move the bottom token to our input position
        selectedCardsCopy[0].transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                                                                                 Input.mousePosition.y,
                                                                                 1));
        // have the rest of the tokens appear above it
        for (int i = 1; i < selectedCardsCopy.Count; i++)
        {
            selectedCardsCopy[i].transform.position = new Vector3(selectedCardsCopy[i - 1].transform.position.x,
                                                                  selectedCardsCopy[i - 1].transform.position.y + Config.config.draggedTokenOffset,
                                                                  selectedCardsCopy[i - 1].transform.position.z - 0.05f);
        }

        DragGlow(hit);
    }

    private void DragGlow(RaycastHit2D hit)
    {
        // if there is no stuff glowing, stop
        if (!(matchTokensAreGlowing || moveTokensAreGlowing || reactorIsGlowing || foundationIsGlowing))
            return;

        if (hit.collider != null)
        {
            if (hit.collider.gameObject == hoveringOver) // are we still hovering over the same object
                return;

            DragGlowRevert();
            hoveringOver = hit.collider.gameObject;

            // if we are hovering over a glowing token
            if ((matchTokensAreGlowing || moveTokensAreGlowing) && hoveringOver.CompareTag("Card") &&
                hoveringOver.GetComponent<CardScript>().IsGlowing())
            {
                // change the dragged token hologram color to what it's hovering over and check if it was a match
                if (selectedCardsCopy[selectedCardsCopy.Count - 1].GetComponent<CardScript>().ChangeHologram(hoveringOver.GetComponent<CardScript>().GetGlowColor()))
                {
                    // if the hovering over token is not in the reactor
                    if (!hoveringOver.transform.parent.CompareTag("Reactor"))
                    {
                        // hide the hover over tokens food hologram
                        hoveringOver.GetComponent<CardScript>().HideHologram();
                        hidFoodHologram = true;
                    }
                }

                changedHologramColor = true;
            }
            // else if we are hovering over a glowing reactor
            else if (reactorIsGlowing && hoveringOver.CompareTag("Reactor") &&
                hoveringOver.GetComponent<ReactorScript>().IsGlowing())
            {
                selectedCardsCopy[0].GetComponent<CardScript>().ChangeHologram(hoveringOver.GetComponent<ReactorScript>().GetGlowColor());
                changedHologramColor = true;

                hoveringOver.GetComponent<ReactorScript>().ChangeSuitGlow(new Color(0, 1, 0, 0.5f));
                changedSuitGlowColor = true;
            }
            else if (foundationIsGlowing && hoveringOver.CompareTag("Foundation") &&
                hoveringOver.GetComponent<FoundationScript>().IsGlowing())
            {
                Debug.Log("hover");
                selectedCardsCopy[selectedCardsCopy.Count - 1].GetComponent<CardScript>().ChangeHologram(hoveringOver.GetComponent<FoundationScript>().GetGlowColor());
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
            selectedCardsCopy[selectedCardsCopy.Count - 1].GetComponent<CardScript>().ChangeHologram(Color.white);
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

        card2Script.MoveCard(matchedPile);
        card1Script.MoveCard(matchedPile);

        int points = matchPoints + (Config.config.consecutiveMatches * Config.config.scoreMultiplier);
        UpdateScore(points);
        UpdateActions(0, isMatch: true);

        soundController.FoodMatch(card1Script.cardSuit);
        baby.GetComponent<SpaceBabyController>().BabyEatAnim();

        StartCoroutine(FoodComboMove(comboHologram, matchExplosion));
        StartCoroutine(PointFade(points, p));
    }

    IEnumerator FoodComboMove(GameObject comboHologram, GameObject matchExplosion)
    {
        yield return new WaitForSeconds(0.3f);
        //Vector3 target = baby.transform.position;
        SpriteRenderer comboSR = comboHologram.GetComponent<SpriteRenderer>();
        Color fadeColor = new Color(1, 1, 1, 1);
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
        }

        //soundController.CardMatchSound();
        Destroy(matchExplosion);
        Destroy(comboHologram);
    }

    IEnumerator PointFade(int points, Vector3 position)
    {
        yield return new WaitForSeconds(0.2f);
        GameObject matchPointsEffect = Instantiate(matchPointsPrefab, position, Quaternion.identity, gameUI.transform);

        Text pointText = matchPointsEffect.GetComponent<Text>();
        pointText.text = "+" + points.ToString();

        Text comboText = matchPointsEffect.transform.GetChild(0).GetComponent<Text>();
        if (Config.config.consecutiveMatches > 1)
        {
            comboText.text = "X" + Config.config.consecutiveMatches.ToString() + " COMBO";

            Color comboColor;
            if (Config.config.consecutiveMatches == 2)
                comboColor = Color.cyan;
            else if (Config.config.consecutiveMatches == 3)
                comboColor = Color.blue;
            else if (Config.config.consecutiveMatches == 4)
                comboColor = Color.magenta;
            else if (Config.config.consecutiveMatches == 5)
                comboColor = Color.red;
            else
                comboColor = Color.yellow;

            comboText.color = comboColor;
            pointText.color = comboColor;
        }

        float scale = 1;
        while (scale < 1.5)
        {
            yield return new WaitForSeconds(0.02f);
            scale += 0.01f;
            matchPointsEffect.transform.localScale = Vector3.one * scale;
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
        }

        Destroy(matchPointsEffect);
    }

    public bool CanMatch(CardScript card1, CardScript card2, bool checkIsTop = true)
    {
        //checks if the two cards can match together
        
        // checks if the cards are at the top of their containers
        if (checkIsTop && (!IsAtContainerTop(card1) || !IsAtContainerTop(card2)))
            return false;

        if (card1.cardNum != card2.cardNum)
            return false;

        if ((card1.cardSuit.Equals("hearts") && card2.cardSuit.Equals("diamonds")) ||
            (card1.cardSuit.Equals("diamonds") && card2.cardSuit.Equals("hearts")) ||
            (card1.cardSuit.Equals("spades") && card2.cardSuit.Equals("clubs")) ||
            (card1.cardSuit.Equals("clubs") && card2.cardSuit.Equals("spades")))
            return true;

        //otherwise not a match 
        return false;
    }

    private bool IsAtContainerTop(CardScript card)
    {
        // checks if the card is at the top of its container's cardList
        // hitboxes are disabled for all cards not on the top for the reactor, wastepile, and deck

        if (card.container.CompareTag("Foundation") &&
            card.container.GetComponent<FoundationScript>().cardList[0].GetComponent<CardScript>() != card)
            return false;
        /*if (card.container.CompareTag("Reactor") &&
            card.container.GetComponent<ReactorScript>().cardList[0].GetComponent<CardScript>() != card)
            return false;
        if (card.container.CompareTag("Wastepile") &&
            card.container.GetComponent<WastepileScript>().cardList[0].GetComponent<CardScript>() != card)
            return false;
        if (card.container.CompareTag("Deck") &&
            card.container.GetComponent<DeckScript>().cardList[0].GetComponent<CardScript>() != card)
            return false;*/
        return true;
    }

    public bool IsSameSuit(GameObject object1, GameObject object2)
    {
        return (GetSuit(object1) == GetSuit(object2));
    }

    public string GetSuit(GameObject suitObject)
    {
        if (suitObject.CompareTag("Card"))
            return suitObject.GetComponent<CardScript>().cardSuit;
        if (suitObject.CompareTag("Reactor"))
            return suitObject.GetComponent<ReactorScript>().suit;

        throw new System.ArgumentException("suitObject must have a suit variable");
    }

    public void UpdateScore(int addScore)
    {
        Config.config.score += addScore;
        scoreBox.GetComponent<ScoreScript>().UpdateScore();
    }

    public void UpdateActions(int actionUpdate, bool setAsValue = false, bool checkGameOver = false, bool startingGame = false, bool isMatch = false)
    {
        if (isMatch)
            Config.config.consecutiveMatches++;
        else
            Config.config.consecutiveMatches = 0;

        // so that a nextcycle trigger doesn't save the state before and after, we only need the after
        bool doSaveState = true;

        // detecting if a nextcycle will be triggered
        if (startingGame || (!setAsValue && ((Config.config.actions + actionUpdate) >= Config.config.actionMax)))
            doSaveState = false;
        else
            Config.config.MoveCounter++;

        bool wasInAlertThreshold = Config.config.actionMax - Config.config.actions <= Config.config.turnAlertThreshold;

        // loading a saved game triggers this
        if (startingGame)
        {
            Config.config.actions = actionUpdate;
        }
        // a nextcycle after it's done triggers this
        else if (setAsValue)
        {
            if (CheckGameOver()) // if nextcycle caused a Game Over
                return;

            Config.config.actions = actionUpdate;
        }
        else if (actionUpdate == 0) // a match was made
        {
            // check if reactor alerts should be turned off
            if (wasInAlertThreshold)
                Alert(false, true);

            if (!CheckGameOver()) // if a match didn't win the game
                StateLoader.saveSystem.writeState();
            return;
        }
        else if (actionUpdate == -1) // a match undo
        {
            if (wasInAlertThreshold)
                Alert(false, true);
            StateLoader.saveSystem.writeState();
            return;
        }
        else
            Config.config.actions += actionUpdate;

        moveCounter.GetComponent<ActionCountScript>().UpdateActionText();

        // foundation moves trigger this as they are the only ones that can cause a gameover via winning
        // reactors trigger their own gameovers
        if (checkGameOver && CheckGameOver())
            return;

        if (doSaveState)
            StateLoader.saveSystem.writeState();

        // time to determine if the alert should be turned on
        bool isInAlertThreshold = Config.config.actionMax - Config.config.actions <= Config.config.turnAlertThreshold;
        if (!wasInAlertThreshold && !isInAlertThreshold)
        {
            // do nothing
        }
        else if (wasInAlertThreshold && isInAlertThreshold)
            Alert(false, true);
        else if (wasInAlertThreshold && !isInAlertThreshold)
            Alert(false);
        else if (!wasInAlertThreshold && isInAlertThreshold)
            Alert(true);

        CheckNextCycle();
    }

    private void Alert(bool turnOn, bool checkAgain = false)
    {
        bool alertTurnedOn = false;

        // if we are checking again to see if anything has changed since the last move
        if (turnOn || checkAgain)
        {
            foreach (GameObject reactor in Config.config.reactors)
            {
                // if a nextcyle will overload the reactor
                if (reactor.GetComponent<ReactorScript>().CountReactorCard() +
                    reactor.GetComponent<ReactorScript>().GetIncreaseOnNextCycle() >= Config.config.maxReactorVal)
                {
                    reactor.GetComponent<ReactorScript>().AlertOn();
                    alertTurnedOn = true;
                }
                // try turning the glow off just in case if it already on
                else if (checkAgain)
                    reactor.GetComponent<ReactorScript>().AlertOff();
            }
        }
        // we are done with the alert
        else
        {
            Config.config.GetComponent<MusicController>().GameMusic();

            foreach (GameObject reactor in Config.config.reactors)
                reactor.GetComponent<ReactorScript>().AlertOff();
        }

        // if the alert was turned on during this check
        if (alertTurnedOn)
        {
            // if the alert was not already turned on, turn it on and play the sound
            if (moveCounter.GetComponent<ActionCountScript>().TurnSirenOn(2))
                soundController.AlertSound();
        }
        // if the action counter is low
        else if (turnOn || checkAgain)
        {
            moveCounter.GetComponent<ActionCountScript>().TurnSirenOn(1);
        }
        // the action counter is not low so turn stuff off
        else
        {
            moveCounter.GetComponent<ActionCountScript>().TurnSirenOff();
        }

        if (turnOn)
        {
            Config.config.GetComponent<MusicController>().AlertMusic();
            baby.GetComponent<SpaceBabyController>().BabyAngryAnim();
        }
    }

    private bool CheckGameOver()
    {
        if (!Config.config.gameOver && Config.config.CountFoundationCards() == 0)
        {
            SetEndGameScore();
            Config.config.GameOver(true);
            return true;
        }
        return false;
    }

    private void CheckNextCycle()
    {
        if (Config.config.actions >= Config.config.actionMax)
            Config.config.deck.GetComponent<DeckScript>().StartNextCycle();
    }

    private void SetEndGameScore()
    {
        int extraScore = 0;
        if (matchedPile.GetComponent<MatchedPileScript>().cardList.Count == 52)
            extraScore += PerfectGamePoints;

        if (Config.config.reactor1.GetComponent<ReactorScript>().cardList.Count == 0)
            extraScore += emptyReactorPoints;

        if (Config.config.reactor2.GetComponent<ReactorScript>().cardList.Count == 0)
            extraScore += emptyReactorPoints;

        if (Config.config.reactor3.GetComponent<ReactorScript>().cardList.Count == 0)
            extraScore += emptyReactorPoints;

        if (Config.config.reactor4.GetComponent<ReactorScript>().cardList.Count == 0)
            extraScore += emptyReactorPoints;

        UpdateScore(extraScore);
    }

    public bool IsInputStopped()
    {
        return inputStopped;
    }

    public void SetInputStopped(bool setTo)
    {
        if (!isMatching)
            inputStopped = setTo;
    }

    public bool IsDragging()
    {
        return dragOn;
    }
}