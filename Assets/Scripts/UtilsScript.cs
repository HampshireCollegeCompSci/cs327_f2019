using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class UtilsScript : MonoBehaviour
{
    public static UtilsScript global; //Creates a new instance if one does not yet exist
    public List<GameObject> selectedCards;
    private List<GameObject> selectedCardsCopy = new List<GameObject>();
    public GameObject matchedPile;
    public GameObject matchPrefab;

    public GameObject gameUI;
    public GameObject scoreBox;
    public GameObject moveCounter;
    public SoundController soundController;
    public int indexCounter;
    public RaycastHit2D hit;
    private bool dragOn;
    private GameObject newGameObject;
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
    private bool matchTokensAreGlowing;
    private bool moveTokensAreGlowing;
    private bool reactorIsGlowing;

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
        baby = GameObject.FindWithTag("Baby");
        matchPoints = Config.config.matchPoints;
        emptyReactorPoints = Config.config.emptyReactorPoints;
        PerfectGamePoints = Config.config.perfectGamePoints;
        UndoScript.undoScript.utils = gameObject.GetComponent<UtilsScript>();
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex != 2)
        {
            return;
        }

        if (!Config.config.gameOver && !Config.config.gamePaused)
        {
            if (Input.GetMouseButtonDown(0) && dragOn == false)
            {
                Click();

                if (selectedCards.Count > 0)
                {
                    StartDragging();
                }

                //checks if the game has been won

                /*this code is if we want to check cards in the deck and the wastepile as well as the foundations to see if you can win the game
                 * if (Config.config.CountFoundationCards() + Config.config.wastePile.GetComponent<WastepileScript>().cardList.Count +
                   Config.config.deck.GetComponent<DeckScript>().cardList.Count == 0)*/
            }
            else if (Input.GetMouseButtonUp(0) && selectedCardsCopy.Count > 0)
            {
                Click();
                ShowPossibleMoves.showPossibleMoves.HideMoves();

                for (int i = 0; i < selectedCardsCopy.Count; i++)
                {
                    Destroy(selectedCardsCopy[i]);
                }

                selectedCardsCopy.Clear();
                dragOn = false;
                UnselectCards();
            }
        }

        if (dragOn == true)
        {
            ClickAndDrag();
        }
    }

    public void StartDragging()
    {
        // make a copy of the selected cards to move around
        int cardsCount = selectedCards.Count;
        for (int i = 0; i < cardsCount; i++)
        {
            newGameObject = (GameObject)Instantiate(selectedCards[i], Vector3.zero, Quaternion.identity);
            newGameObject.GetComponent<CardScript>().MakeVisualOnly();
            newGameObject.GetComponent<CardScript>().SetSelected(false);
            selectedCardsCopy.Add(newGameObject);
        }

        // enable dragged reactor tokens holograms
        if (cardsCount == 1 && selectedCards[0].GetComponent<CardScript>().container.CompareTag("Reactor"))
        {
            selectedCardsCopy[0].GetComponent<CardScript>().ShowHologram();
        }

        // show any tokens (and reactors) that we can interact with
        ShowPossibleMoves.showPossibleMoves.ShowMoves(selectedCards[0]);

        changedHologramColor = false;
        changedSuitGlowColor = false;

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

        soundController.CardPressSound();
        dragOn = true;

        Vibration.Vibrate(1);
    }

    public void ClickAndDrag()
    {
        // move the bottom token to our input position
        selectedCardsCopy[0].transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                                                                                 Input.mousePosition.y,
                                                                                 1));
        // have the rest of the tokens appear above it
        int cardCount = selectedCardsCopy.Count;
        for (int i = 1; i < cardCount; i++)
        {
            selectedCardsCopy[i].transform.position = new Vector3(selectedCardsCopy[i - 1].transform.position.x,
                                                                  selectedCardsCopy[i - 1].transform.position.y + Config.config.draggedTokenOffset,
                                                                  selectedCardsCopy[i - 1].transform.position.z - 0.05f);
        }

        // if there is stuff glowing, check to see if we are hovering over them
        if (matchTokensAreGlowing || moveTokensAreGlowing || reactorIsGlowing)
        {
            hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                                                                               Input.mousePosition.y,
                                                                               10)),
                                    Vector2.zero);

            if (hit.collider != null) // if we hit something
            {
                if (hit.collider.gameObject == hoveringOver) // are we still hovering over the same object
                {
                    return;
                }

                if (changedSuitGlowColor)
                {
                    hoveringOver.GetComponent<ReactorScript>().RevertSuitGlow();
                    changedSuitGlowColor = false;
                }

                hoveringOver = hit.collider.gameObject;

                // and tokens are glowing and we hit a glowing card (can move to it or match with it)
                if ((matchTokensAreGlowing || moveTokensAreGlowing) && hoveringOver.CompareTag("Card") &&
                    hoveringOver.GetComponent<CardScript>().IsGlowing())
                {
                    CardScript hoveringOverToken = hoveringOver.GetComponent<CardScript>();
                    Color hoveringGlow = hoveringOverToken.GetGlowColor();
                    selectedCardsCopy[cardCount - 1].GetComponent<CardScript>().ChangeHologramColor(hoveringGlow);
                    changedHologramColor = true;

                    if (matchTokensAreGlowing) // if there are matching tokens available
                    {
                        if (hoveringGlow == Config.config.cardMatchHighlightColor) // if we can match with this token
                        {
                            selectedCardsCopy[0].GetComponent<CardScript>().ChangeFoodHologram(false, updateScale: true);
                        }
                        else
                        {
                            selectedCardsCopy[0].GetComponent<CardScript>().ChangeFoodHologram(true, updateScale: true);
                        }
                    }
                }
                // if we are hovering over a glowing reactor
                else if (reactorIsGlowing && hoveringOver.CompareTag("Reactor") &&
                    hoveringOver.GetComponent<ReactorScript>().IsGlowing())
                {
                    selectedCardsCopy[0].GetComponent<CardScript>().ChangeHologramColor(hoveringOver.GetComponent<ReactorScript>().GetGlowColor());
                    changedHologramColor = true;

                    hoveringOver.GetComponent<ReactorScript>().ChangeSuitGlow(new Color(0, 1, 0, 0.5f));
                    changedSuitGlowColor = true;

                    if (matchTokensAreGlowing)
                    {
                        selectedCardsCopy[0].GetComponent<CardScript>().ChangeFoodHologram(true, updateScale: true);
                    }
                }
                // we are hovering over an object that is of no concern
                else if (changedHologramColor) // if we where hovering over a glowing token
                {
                    selectedCardsCopy[cardCount - 1].GetComponent<CardScript>().ChangeHologramColor(Color.white);
                    changedHologramColor = false;

                    if (matchTokensAreGlowing) // if there is the possibility that we where hovering over a matching token
                    {
                        selectedCardsCopy[0].GetComponent<CardScript>().ChangeFoodHologram(true, updateScale: true);
                    }
                }
            }
            else if (changedHologramColor) // if we where hovering over a glowing token or reactor
            {
                if (changedSuitGlowColor)
                {
                    hoveringOver.GetComponent<ReactorScript>().RevertSuitGlow();
                    changedSuitGlowColor = false;
                }

                hoveringOver = null;

                selectedCardsCopy[cardCount - 1].GetComponent<CardScript>().ChangeHologramColor(Color.white);
                changedHologramColor = false;
                
                if (matchTokensAreGlowing) // if there is the possibility that we where hovering over a matching token
                {
                    selectedCardsCopy[0].GetComponent<CardScript>().ChangeFoodHologram(true, updateScale: true);
                }
            }
        }
    }

    public void SelectCard(GameObject inputCard)
    {
        if (inputCard.GetComponent<CardScript>().container.CompareTag("Wastepile"))
        {
            inputCard.GetComponent<CardScript>().container.GetComponent<WastepileScript>().DraggingCard(inputCard, true);
            draggingWastepile = true;
        }

        selectedCards.Add(inputCard);
        inputCard.GetComponent<CardScript>().SetSelected(true);
    }

    public void UnselectCards()
    {
        if (draggingWastepile && selectedCards.Count == 1)
        {
            wastePile.GetComponent<WastepileScript>().DraggingCard(selectedCards[0], false);
            draggingWastepile = false;
        }

        for (int i = 0; i < selectedCards.Count; i++)
        {
            selectedCards[i].GetComponent<CardScript>().SetSelected(false);
        }

        selectedCards.Clear();
    }

    public void SelectMultipleCards(int cardsToCount)
    {
        for (indexCounter = cardsToCount; indexCounter + 1 > 0; indexCounter--)
        {
            SelectCard(hit.collider.gameObject.GetComponent<CardScript>().container.GetComponent<FoundationScript>().cardList[indexCounter]);
        }
    }

    public int countCardsToSelect(GameObject selectedCard) //takes the input of a selected card and counts how many cards are above it in a foundation
    {
        for (indexCounter = hit.collider.gameObject.GetComponent<CardScript>().container.GetComponent<FoundationScript>().cardList.Count - 1; indexCounter > 0; indexCounter--)
        {
            if (selectedCard == hit.collider.gameObject.GetComponent<CardScript>().container.GetComponent<FoundationScript>().cardList[indexCounter])
            {
                //Debug.Log("indexCounter " + (indexCounter + 1));
                return indexCounter;
            }
        }

        return 0;
    }

    //sends out a raycast to see you selected something
    public void Click()
    {
        if (IsInputStopped())
        {
            return;
        }

        //raycast to see what we clicked
        hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)), Vector2.zero);

        //if we click a deck activates deck and deselected our cards
        if (hit.collider != null && !hit.collider.gameObject.CompareTag("Card"))
        {
            if (hit.collider.gameObject.CompareTag("Deck"))
            {
                return;
                //hit.collider.gameObject.GetComponent<DeckScript>().ProcessAction(hit.collider.gameObject);
            }

            if (selectedCards.Count != 0)
            {
                selectedCards[0].GetComponent<CardScript>().container.SendMessage("ProcessAction", hit.collider.gameObject);
                UnselectCards();
            }

            else if (hit.collider != null && hit.collider.gameObject.CompareTag("Baby"))
            {
                baby.GetComponent<SpaceBabyController>().BabyHappyAnim();
            }

            return;
        }

        // if we click a card in the deck call deck clicked and deselect all cards
        else if (hit.collider != null && hit.collider.gameObject.GetComponent<CardScript>().container.CompareTag("Deck"))
        {
            hit.collider.gameObject.GetComponent<CardScript>().container.SendMessage("ProcessAction", hit.collider.gameObject);
            UnselectCards();
            return;
        }

        //if we click a card in the wastepile and we don't have any card selected select the card in the wastepile
        else if (hit.collider != null && hit.collider.gameObject.GetComponent<CardScript>().container.CompareTag("Wastepile") && selectedCards.Count == 0)
        {
            if (hit.collider.gameObject.GetComponent<CardScript>().container.GetComponent<WastepileScript>().cardList[0] == hit.collider.gameObject)
            {
                SelectCard(hit.collider.gameObject);
            }
        }

        //if we click a card in a reactor and we don't have any card selected select the card in the reactor
        else if (hit.collider != null && hit.collider.gameObject.GetComponent<CardScript>().container.CompareTag("Reactor") && selectedCards.Count == 0)
        {
            if (hit.collider.gameObject.GetComponent<CardScript>().container.GetComponent<ReactorScript>().cardList[0] == hit.collider.gameObject)
            {
                SelectCard(hit.collider.gameObject);
            }
        }

        //if we click a card in a foundation and we don't have any card selected and the card we're trying to select is not hidden select the card in the foundation
        else if (hit.collider != null && selectedCards.Count == 0 && !hit.collider.gameObject.GetComponent<CardScript>().isHidden() &&
            hit.collider.gameObject.GetComponent<CardScript>().container.CompareTag("Foundation"))
        {
            SelectMultipleCards(countCardsToSelect(hit.collider.gameObject));
        }


        //if we click on our first selected card deselect all cards
        else if (hit.collider != null && selectedCards.Count != 0 && selectedCards[0] == hit.collider.gameObject)
        {
            UnselectCards();
        }

        //if we click on something else tries to move the selected cards 
        else if (hit.collider != null && selectedCards.Count != 0 && !hit.collider.GetComponent<CardScript>().isHidden())
        {
            selectedCards[0].GetComponent<CardScript>().container.SendMessage("ProcessAction", hit.collider.gameObject);
            //we are no longer changing a list that we are also iterating over
            UnselectCards();
        }
    }

    public void Match(GameObject card1, GameObject card2)
    {
        // these must be in this order
        SetInputStopped(true);
        wastePile.GetComponent<WastepileScript>().scrollRect.horizontal = false;
        isMatching = true; // a wastepile animation will overwrite the above without this

        Vector3 p = card1.transform.position;
        Quaternion t = card1.transform.rotation;
        p.z += 2;

        GameObject matchExplosion = Instantiate(matchPrefab, p, t);

        StartCoroutine(animatorwait(card1, card2, matchExplosion));
    }
    IEnumerator animatorwait(GameObject card1, GameObject card2, GameObject matchExplosion)
    {
        CardScript card1Script = card1.GetComponent<CardScript>();
        CardScript card2Script = card2.GetComponent<CardScript>();

        if (card1Script.container.CompareTag("Reactor"))
        {
            card1Script.ShowHologram();
        }

        if (card2Script.container.CompareTag("Reactor"))
        {
            card2Script.hologramFood.SetActive(true);
        }
        else if (card2Script.container.CompareTag("Foundation"))
        {
            List<GameObject> containerCards = card2Script.container.GetComponent<FoundationScript>().cardList;
            if (containerCards.Count > 1)
            {
                CardScript containerCardScript = containerCards[1].GetComponent<CardScript>();
                if (containerCardScript.isHidden())
                {
                    containerCardScript.SetVisibility(true);
                    containerCardScript.ShowHologram();
                    containerCardScript.hidden = true;
                }
            }
        }

        Vector3 origin = card1.transform.position;
        card2.transform.position = origin;
        card2Script.hologram.SetActive(false);
        card2Script.UpdateMaskInteraction(card1.GetComponent<SpriteRenderer>().maskInteraction);
        card1Script.hologram.GetComponent<SpriteRenderer>().sortingOrder = 1;

        soundController.CardStackSound();
        yield return new WaitForSeconds(0.4f);
        soundController.FoodMatch(card1Script.cardSuit);
        yield return new WaitForSeconds(0.3f);

        card1Script.hologram.GetComponent<SpriteRenderer>().sortingOrder = 0;
        card2Script.UpdateMaskInteraction(SpriteMaskInteraction.None);
        card2Script.MoveCard(matchedPile);
        card1Script.MoveCard(matchedPile);
        UpdateActionCounter(0);

        UpdateScore(matchPoints);
        baby.GetComponent<SpaceBabyController>().BabyEatAnim();

        wastePile.GetComponent<WastepileScript>().scrollRect.horizontal = true;

        // these must be in this order
        isMatching = false;
        SetInputStopped(false);
        CheckGameOver();

        GameObject comboToLoad = new GameObject("combo");
        SpriteRenderer Srenderer = comboToLoad.AddComponent<SpriteRenderer>();
        Srenderer.sortingLayerID = selectedLayer;

        Srenderer.sprite = card1Script.hologramComboSprite;
        comboToLoad.transform.localScale = Vector3.one * 0.15f;
        comboToLoad.transform.position = origin;

        StartCoroutine(FadeImage(comboToLoad, matchExplosion));

        yield return new WaitForSeconds(0.2f);
        soundController.CardMatchSound();
        //StateLoader.saveSystem.writeState();
    }

    IEnumerator FadeImage(GameObject comboToLoad, GameObject matchExplosion)
    {
        Vector3 target = baby.transform.position;
        SpriteRenderer comboSR = comboToLoad.GetComponent<SpriteRenderer>();
        float initialDistance = Vector3.Distance(comboToLoad.transform.position, target);
        byte speed = (byte) (Config.config.cardsToReactorspeed / 3);
        while (comboToLoad.transform.position != target)
        {
            comboSR.color = new Color(1, 1, 1, Vector3.Distance(comboToLoad.transform.position, target) / initialDistance);
            comboToLoad.transform.position = Vector3.MoveTowards(comboToLoad.transform.position, target,
                Time.deltaTime * speed);
            yield return null;
        }

        Destroy(matchExplosion);
        Destroy(comboToLoad);
    }
    //checks if suit match AND value match
    public bool IsMatch(GameObject card1, GameObject card2)
    {

        //Debug.Log(card1.GetComponent<CardScript>().cardSuit + card1.GetComponent<CardScript>().cardNum);
        //Debug.Log(card2.GetComponent<CardScript>().cardSuit + card2.GetComponent<CardScript>().cardNum);
        //just to make it cleaner because this utils.blah blah blah is yucky
        //basically a string of if/else cases for matching
        string card1Suit = card1.GetComponent<CardScript>().cardSuit;
        string card2Suit = card2.GetComponent<CardScript>().cardSuit;
        int card1Num = card1.GetComponent<CardScript>().cardNum;
        int card2Num = card2.GetComponent<CardScript>().cardNum;
        if (card1Num != card2Num)
        {
            //Debug.Log("Numbers don't match");
            return false;
        }
        else
        {
            //hearts diamond combo #1
            if (card1Suit.Equals("hearts") && card2Suit.Equals("diamonds"))
            {
                return true;
            }
            //hearts diamond combo #2
            else if (card1Suit.Equals("diamonds") && card2Suit.Equals("hearts"))
            {
                return true;
            }
            //spades clubs combo #1
            else if (card1Suit.Equals("spades") && card2Suit.Equals("clubs"))
            {
                return true;
            }
            //spades clubs combo #2
            else if (card1Suit.Equals("clubs") && card2Suit.Equals("spades"))
            {
                return true;
            }
            //otherwise not a match 
            else
            {
                //Debug.Log("Suits don't match");
                return false;
            }
        }
    }

    public bool IsSameSuit(GameObject card1, GameObject card2)
    {
        string card1Suit = card1.GetComponent<CardScript>().cardSuit;
        string card2Suit = card2.GetComponent<CardScript>().cardSuit;

        if (card1Suit.Equals("hearts") && card2Suit.Equals("hearts"))
        {
            return true;
        }
        else if (card1Suit.Equals("diamonds") && card2Suit.Equals("diamonds"))
        {
            return true;
        }
        else if (card1Suit.Equals("spades") && card2Suit.Equals("spades"))
        {
            return true;
        }
        else if (card1Suit.Equals("clubs") && card2Suit.Equals("clubs"))
        {
            return true;
        }
        //otherwise not a match 
        else
        {
            //Debug.Log("Suits don't match");
            return false;
        }
    }

    public void UpdateScore(int addScore)
    {
        Config.config.score += addScore;
        scoreBox.GetComponent<ScoreScript>().UpdateScore();
    }

    public void UpdateActionCounter(int actionUpdate, bool setAsValue = false)
    {
        bool wasInAlertThreshold = Config.config.actionMax - Config.config.actions <= Config.config.turnAlertThreshold;

        if (setAsValue)
        {
            Config.config.actions = actionUpdate;
        }
        else if (actionUpdate == 0)
        {
            if (wasInAlertThreshold)
            {
                Alert(false, true);
            }
            return;
        }
        else
        {
            Config.config.actions += actionUpdate;
        }

        moveCounter.GetComponent<ActionCountScript>().UpdateActionText();

        bool isInAlertThreshold = Config.config.actionMax - Config.config.actions <= Config.config.turnAlertThreshold;

        if (wasInAlertThreshold && isInAlertThreshold)
        {
            Alert(false, true);
            return;
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

    private void Alert(bool turnOn, bool checkAgain = false)
    {
        bool alertTurnedOn = false;

        // if we are checking again to see if anything has changed since the last move
        if (checkAgain)
        {
            foreach (GameObject reactor in Config.config.reactors)
            {
                // if a nextcyle will overload the reactor
                if (reactor.GetComponent<ReactorScript>().CountReactorCard() +
                    reactor.GetComponent<ReactorScript>().GetIncreaseOnNextCycle() >= Config.config.maxReactorVal)
                {
                    reactor.GetComponent<ReactorScript>().GlowOn(1);
                    alertTurnedOn = true;
                }
                // try turning the glow off just in case if it already on
                else
                {
                    reactor.GetComponent<ReactorScript>().GlowOff(true);
                }
            }
        }
        // if we are checking for the first time
        else if (turnOn)
        {
            Config.config.GetComponent<MusicController>().AlertMusic();
            baby.GetComponent<SpaceBabyController>().BabyAngryAnim();

            foreach (GameObject reactor in Config.config.reactors)
            {
                // if a nextcyle will overload the reactor
                if (reactor.GetComponent<ReactorScript>().CountReactorCard() +
                    reactor.GetComponent<ReactorScript>().GetIncreaseOnNextCycle() >= Config.config.maxReactorVal)
                {
                    reactor.GetComponent<ReactorScript>().GlowOn(1);
                    alertTurnedOn = true;
                }
            }
        }
        // we are done with the alert
        else
        {
            Config.config.GetComponent<MusicController>().GameMusic();

            foreach (GameObject reactor in Config.config.reactors)
            {
                reactor.GetComponent<ReactorScript>().GlowOff(true);
            }
        }

        // if the alert was turned on during this check
        if (alertTurnedOn)
        {
            // if the alert was not already turned on, turn it on and play the sound
            if (moveCounter.GetComponent<ActionCountScript>().TurnAlertOn())
            {
                soundController.AlertSound();
            }
        }
        // if the action counter is low
        else if (turnOn || checkAgain)
        {
            moveCounter.GetComponent<ActionCountScript>().TurnSirenOn();
        }
        // the action counter is not low so turn stuff off
        else
        {
            moveCounter.GetComponent<ActionCountScript>().TurnSirenOff();
        }
    }

    public void CheckNextCycle()
    {
        if (Config.config.actions == Config.config.actionMax)
        {
            Config.config.deck.GetComponent<DeckScript>().StartNextCycle();
        }
    }

    public void CheckGameOver()
    {
        if (!Config.config.gameOver && Config.config.CountFoundationCards() == 0)
        {
            SetEndGameScore();
            Config.config.GameOver(true);
        }
    }

    public void SetEndGameScore()
    {
        int extraScore = 0;
        if (matchedPile.GetComponent<MatchedPileScript>().cardList.Count == 52)
        {
            extraScore += PerfectGamePoints;
        }

        if (Config.config.reactor1.GetComponent<ReactorScript>().cardList.Count == 0)
        {
            extraScore += emptyReactorPoints;
        }

        if (Config.config.reactor2.GetComponent<ReactorScript>().cardList.Count == 0)
        {
            extraScore += emptyReactorPoints;
        }

        if (Config.config.reactor3.GetComponent<ReactorScript>().cardList.Count == 0)
        {
            extraScore += emptyReactorPoints;
        }

        if (Config.config.reactor4.GetComponent<ReactorScript>().cardList.Count == 0)
        {
            extraScore += emptyReactorPoints;
        }
        UpdateScore(extraScore);
    }

    public void PACards()
    {
        if (selectedCards.Count != 0)
        {
            selectedCards[0].GetComponent<CardScript>().container.SendMessage("ProcessAction", hit.collider.gameObject);
            UnselectCards();
        }
    }

    public bool IsInputStopped()
    {
        return inputStopped;
    }

    public void SetInputStopped(bool setTo)
    {
        if (!isMatching)
        {
            inputStopped = setTo;
        }
    }
}