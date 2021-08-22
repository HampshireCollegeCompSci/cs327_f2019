using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialScript : MonoBehaviour
{
    private Queue<List<string>> commandQueue;
    private bool waiting;

    public GameObject tutorial, tutorialText;
    public GameObject tutorialReactors, tutorialScore, tutorialMoveCounter;
    public GameObject tutorialUndo, tutorialPause;
    public GameObject tutorialFoundations, tutorialDeck, tutorialWastePile;

    private void Awake()
    {
        // this is the gateway to turn the tutorial on
        if (!Config.config.tutorialOn)
            return;

        Debug.Log("starting tutorialScript");
        
        // prevent the user from interacting with anything but the tutorial
        UtilsScript.Instance.SetInputStopped(true);
        // start the tutorial
        tutorial.SetActive(true);
        // update colors via script instead of having to do it in editor each time
        UpdateHighlightObjectsColor(new Color(1, 1, 0, 0.35f));

        // get the tutorial commands ready
        commandQueue = CommandEnqueuer(CreateFromJSON());

        // start the tutorial
        waiting = false;
        NextStep();
    }

    /// <summary>
    /// Updates all the highlightable objects (not including tokens/cards) color to the new color given.
    /// </summary>
    private void UpdateHighlightObjectsColor(Color newColor)
    {
        tutorialReactors.GetComponent<Image>().color = newColor;
        tutorialScore.GetComponent<Image>().color = newColor;
        tutorialMoveCounter.GetComponent<Image>().color = newColor;
        tutorialUndo.GetComponent<Image>().color = newColor;
        tutorialPause.GetComponent<Image>().color = newColor;
        tutorialFoundations.GetComponent<Image>().color = newColor;
        tutorialDeck.GetComponent<Image>().color = newColor;
        tutorialWastePile.GetComponent<Image>().color = newColor;
    }

    /// <summary>
    /// Creates and returns a list of commands to follow from a JSON file.
    /// </summary>
    private static List<ArgumentListWrapper> CreateFromJSON()
    {
        Debug.Log("creating list from JSON");

        var jsonTextFile = Resources.Load<TextAsset>("Tutorial/TutorialCommandList");
        TutorialCommands commandFile = JsonUtility.FromJson<TutorialCommands>(jsonTextFile.ToString());
        return commandFile.commands;
    }

    /// <summary>
    /// Enqueues the command list into a queue.
    /// </summary>
    private Queue<List<string>> CommandEnqueuer(List<ArgumentListWrapper> commandList)
    {
        Debug.Log("creating command queue");

        Queue<List<string>> newQueue = new Queue<List<string>>();

        foreach (ArgumentListWrapper command in commandList)
        {
            newQueue.Enqueue(command.argumentList);
        }

        return newQueue;
    }

    /// <summary>
    /// Determines if the next command should be interpreted.
    /// </summary>
    private void NextStep()
    {
        if (!waiting)
        {
            CommandInterpreter();
        }
    }

    /// <summary>
    /// Continues the tutorial if it's ready. The tutorial next button calls this.
    /// </summary>
    public void NextButton()
    {
        Debug.Log("tutorial touch");

        if (waiting)
        {
            waiting = false;
            SoundEffectsController.Instance.ButtonPressSound();
            NextStep();
        }
    }

    /// <summary>
    /// Looks at the next command in the queue before sending them off to be carried out.
    /// </summary>
    private void CommandInterpreter()
    {
        Debug.Log("interpreting command");

        if (commandQueue.Count == 0)
        {
            Debug.Log("the end of the tutorial command list has been reached, ending the tutorial now");
            EndTutorial();
            return;
        }

        // getting the next command
        List<string> command = commandQueue.Dequeue();

        // try to interpret the command
        // catches all failed attempts and logs them readably
        try
        {
            switch (command[0].ToLower())
            {
                case "endtutorial":
                    EndTutorial();
                    break;
                case "waitfortouch":
                    WaitForTouch();
                    break;
                case "loadsave":
                    LoadSave(command);
                    break;
                case "showtext":
                    ShowText(command);
                    break;
                case "highlightobject":
                    HighlightObject(command);
                    break;
                case "highlightcontainer":
                    HighlightContainer(command);
                    break;
                case "highlighttoken":
                    HighlightToken(command);
                    break;
                case "highlightallinteractabletokens":
                    HighlightAllInteractableTokens();
                    break;
                case "unhighlightalltokens":
                    UnHighlightAllTokens();
                    break;
                case "setmovecountertext":
                    SetMoveCounterText(command);
                    break;
                default:
                    throw new FormatException("does not contain a valid command request for command #0");
            }

            // see if we need to wait for user input or if we can continue interpreting
            NextStep();
        }
        catch (FormatException e)
        {
            // log error in readable form
            Debug.LogError($"tutorial command: \"{string.Join(", ", command)}\", {e.Message}");
        }
    }

    /// <summary>
    /// Ends the tutorial by turning off the tutorial game object and clearing tutorial config parameters.
    /// Loads the main menu afterwards.
    /// </summary>
    private void EndTutorial()
    {
        // not sure if most of this is needed,
        // but it all works and is to be expected on a game end

        Debug.Log("ending tutorial");

        tutorial.SetActive(false);

        // just in case
        if (Config.config != null)
        {
            // setting things to normal
            Config.config.gamePaused = false;
            Config.config.gameOver = false;
            Config.config.gameWin = false;
        }

        SceneManager.LoadScene("MainMenuScene");
        MusicController.Instance.MainMenuMusic();
    }

    /// <summary>
    /// Ends the tutorial by request from the tutorial exit button.
    /// </summary>
    public void ExitButton()
    {
        Debug.Log("exit tutorial requested");
        EndTutorial();
    }

    /// <summary>
    /// Stops the interpreting of commands.
    /// </summary>
    private void WaitForTouch()
    {
        Debug.Log("waiting for touch");

        waiting = true;
    }

    /// <summary>
    /// Loads a savestate into the game.
    /// </summary>
    private void LoadSave(List<string> command)
    {
        CheckCommandCount(command, 2);

        string fileName = command[1];

        Debug.Log("loading save: " + fileName);

        // move all tokens back to load pile
        foreach (GameObject foundation in Config.config.foundations)
            MoveCardsToLoadPile(foundation.GetComponent<FoundationScript>().cardList);
        foreach (GameObject reactor in Config.config.reactors)
            MoveCardsToLoadPile(reactor.GetComponent<ReactorScript>().cardList);
        MoveCardsToLoadPile(DeckScript.Instance.cardList);
        MoveCardsToLoadPile(WastepileScript.Instance.cardList);
        MoveCardsToLoadPile(MatchedPileScript.Instance.cardList);

        // load the saved state and then set it up
        StateLoader.Instance.LoadTutorialState(fileName);
        StateLoader.Instance.UnpackState(state: StateLoader.Instance.gameState, isTutorial: true);
    }

    /// <summary>
    /// Moves all cards/tokens in the list to the load pile.
    /// </summary>
    private void MoveCardsToLoadPile(List<GameObject> cards)
    {
        int cardCount = cards.Count;
        while (cardCount != 0)
        {
            cards[0].GetComponent<CardScript>().MoveCard(LoadPileScript.Instance.gameObject, doLog: false, isAction: false);
            cardCount--;
        }
    }

    /// <summary>
    /// Displays the text in the command on screen in the tutorial box.
    /// </summary>
    private void ShowText(List<string> command)
    {
        CheckCommandCount(command, 2);

        Debug.Log($"showing text: {command[1]}");

        tutorialText.GetComponent<Text>().text = command[1];
    }

    /// <summary>
    /// Changes the highlight state of a game object according to the given commands instructions.
    /// </summary>
    private void HighlightObject(List<string> command)
    {
        // command format: 
        // 0:HighlightObject, 1:Object to Highlight, 2:Highlight On/Off
        // 0:HighlightObject, 1:MANY,                2:On/Off

        Debug.Log("highlighting object");

        CheckCommandCount(command, 3);

        // get if the highlight needs to turned on or off
        bool highlightOn = ParseOnOrOff(command, 2);

        // interpreting  1st command
        switch (command[1].ToLower())
        {
            case "reactors":
                tutorialReactors.SetActive(highlightOn);
                break;
            case "scoreboard":
                tutorialScore.SetActive(highlightOn);
                break;
            case "movecounter":
                tutorialMoveCounter.SetActive(highlightOn);
                break;
            case "undo":
                tutorialUndo.SetActive(highlightOn);
                break;
            case "pause":
                tutorialPause.SetActive(highlightOn);
                break;
            case "foundations":
                tutorialFoundations.SetActive(highlightOn);
                break;
            case "deck":
                tutorialDeck.SetActive(highlightOn);
                break;
            case "wastepile":
                tutorialWastePile.SetActive(highlightOn);
                break;
            default:
                throw new FormatException("does not contain a valid object to highlight for command #1");
        }
    }

    /// <summary>
    /// Changes the highlight state of a container game object (where tokens/cards reside) according to the given commands instructions.
    /// </summary>
    private void HighlightContainer(List<string> command)
    {
        // command format: 
        // 0:HighlightContainer, 1:Container(s) to Highlight,       2:Highlight On/Off, 3:Index,   4:Alert Level
        // 0:HighlightContainer, 1:Reactor(s)-Foundation(s)-Deck,   2:On/Off,           3:0-Count, 4:1/2

        Debug.Log("highlighting container");

        CheckCommandCount(command, 3, maxRequirement: 5);

        // get if the highlight needs to turned on or off 
        bool highlightOn = ParseOnOrOff(command, 2);

        // find the container
        switch (command[1].ToLower())
        {
            case "reactors":
                if (highlightOn)
                {
                    // get the alert level (color) all the reactors need to be set at
                    if (command.Count != 5)
                        throw new FormatException("does not contain an alert level");

                    byte alertLevel = ParseAlertLevel(command, 4);
                    foreach (GameObject reactor in Config.config.reactors)
                        reactor.GetComponent<ReactorScript>().GlowOn(alertLevel);
                }
                else
                    foreach (GameObject reactor in Config.config.reactors)
                        reactor.GetComponent<ReactorScript>().GlowOff();
                break;

            case "reactor":
                if (highlightOn)
                {
                    // set the alert level (color) the reactor need to be set at
                    if (command.Count != 5)
                        throw new FormatException("does not contain an alert level");

                    Config.config.reactors[ParseContainerIndex(command, 3)].GetComponent<ReactorScript>().GlowOn(ParseAlertLevel(command, 4));
                }
                else
                    Config.config.reactors[ParseContainerIndex(command, 3)].GetComponent<ReactorScript>().GlowOff();
                break;

            case "foundations":
                if (highlightOn)
                    foreach (GameObject foundation in Config.config.foundations)
                        foundation.GetComponent<FoundationScript>().GlowOn();
                else
                    foreach (GameObject foundation in Config.config.foundations)
                        foundation.GetComponent<FoundationScript>().GlowOff();
                break;

            case "foundation":
                if (highlightOn)
                    Config.config.foundations[ParseContainerIndex(command, 3)].GetComponent<FoundationScript>().GlowOn();
                else
                    Config.config.foundations[ParseContainerIndex(command, 3)].GetComponent<FoundationScript>().GlowOff();
                break;

            case "deck":
                if (highlightOn)
                    DeckScript.Instance.gameObject.GetComponent<Image>().color = Color.yellow;
                else
                    DeckScript.Instance.gameObject.GetComponent<Image>().color = Color.white;
                break;

            default:
                throw new FormatException("does not contain a valid container to highlight for command #1");
        }
    }

    /// <summary>
    /// Changes the highlight state of a token/card game object according to the given commands instructions.
    /// </summary>
    private void HighlightToken(List<string> command)
    {
        // command format: 
        // 0:Highlight Token, 1:Object(s) Containing Token,   2:Object Index, 3:Token Index, 4:Highlight On/Off, 5:Is Match
        // 0:HighlightToken,  1:Reactor-Foundation-WastePile, 2:0-1-2-3,      3:0-Count,     4:On-Off,           5:True-False

        Debug.Log("highlighting token");

        CheckCommandCount(command, 6);

        // get if the highlight needs to turned on or off 
        bool highlightOn = ParseOnOrOff(command, 4);

        // parse the 2nd command
        int containerIndex = ParseContainerIndex(command, 2);

        // detect if the 3rd command is valid and parse it
        int tokenIndex;
        try
        {
            tokenIndex = Int32.Parse(command[3]);
        }
        catch (FormatException)
        {
            throw new FormatException("does not contain an int for command #3");
        }
        if (tokenIndex < 0)
        {
            throw new FormatException("contains a negative int for command #3");
        }

        // detect if the 5th command is needed, then validate and parse it
        bool match = false;
        if (highlightOn)
        {
            try
            {
                match = bool.Parse(command[5]);
            }
            catch (FormatException)
            {
                throw new FormatException("does not contain a bool for command #5");
            }
        }

        // find the desired token's location
        switch (command[1].ToLower())
        {
            case "reactor":
                List<GameObject> reactorCardList = Config.config.reactors[containerIndex].GetComponent<ReactorScript>().cardList;
                if (reactorCardList.Count < tokenIndex)
                    throw new FormatException($"contains an out of bounds token index for command #3. " +
                        $"there are only {reactorCardList.Count} token(s) to choose from in reactor {containerIndex}");

                if (highlightOn)
                    reactorCardList[tokenIndex].GetComponent<CardScript>().GlowOn(match);
                else
                    reactorCardList[tokenIndex].GetComponent<CardScript>().GlowOff();
                break;

            case "foundation":
                List<GameObject> foundationCardList = Config.config.foundations[containerIndex].GetComponent<FoundationScript>().cardList;
                if (foundationCardList.Count < tokenIndex)
                    throw new FormatException($"contains an out of bounds token index for command #3. " +
                        $"there are only {foundationCardList.Count} token(s) to choose from in foundation {containerIndex}");

                if (highlightOn)
                    foundationCardList[tokenIndex].GetComponent<CardScript>().GlowOn(match);
                else
                    foundationCardList[tokenIndex].GetComponent<CardScript>().GlowOff();
                break;

            case "wastepile":
                if (WastepileScript.Instance.cardList.Count < tokenIndex)
                    throw new FormatException($"contains an out of bounds token index for command #3. " +
                        $"there are only {WastepileScript.Instance.cardList.Count} token(s) to choose from in the waste pile");

                if (highlightOn)
                    WastepileScript.Instance.cardList[tokenIndex].GetComponent<CardScript>().GlowOn(match);
                else
                    WastepileScript.Instance.cardList[tokenIndex].GetComponent<CardScript>().GlowOff();
                break;

            default:
                throw new FormatException("contains an invalid object that contains the token for command #1");
        }
    }

    /// <summary>
    /// Highlights all tokens/cards that can be interacted with by the user.
    /// </summary>
    private void HighlightAllInteractableTokens()
    {
        Debug.Log("highlighting all interactable tokens");

        // to reference repeatedly
        List<GameObject> cardListRef;

        // each token/card on the top of every reactor can be interacted with
        foreach (GameObject reactor in Config.config.reactors)
        {
            cardListRef = reactor.GetComponent<ReactorScript>().cardList;
            if (cardListRef.Count != 0)
            {
                cardListRef[0].GetComponent<CardScript>().GlowOn(false);
            }
        }

        // all tokens/cards that are not hidden in the foundations can be interacted with
        CardScript cardScriptRef;
        foreach (GameObject foundation in Config.config.foundations)
        {
            cardListRef = foundation.GetComponent<FoundationScript>().cardList;
            if (cardListRef.Count != 0)
            {   
                foreach(GameObject card in cardListRef)
                {
                    cardScriptRef = card.GetComponent<CardScript>();
                    
                    // if this token/card is hidden all subsequents will be too
                    if (cardScriptRef.IsHidden)
                    {
                        break;
                    }

                    cardScriptRef.GlowOn(false);
                }
            }
        }

        // the first wastepile token/card can always be interacted with
        if (WastepileScript.Instance.cardList.Count != 0)
        {
            WastepileScript.Instance.cardList[0].GetComponent<CardScript>().GlowOn(false);
        }
    }

    /// <summary>
    /// UnHighlights all tokens/cards.
    /// </summary>
    private void UnHighlightAllTokens()
    {
        Debug.Log("unhighlighting all tokens");

        foreach (GameObject reactor in Config.config.reactors)
            CardListGlowOff(reactor.GetComponent<ReactorScript>().cardList);
        foreach (GameObject foundation in Config.config.foundations)
            CardListGlowOff(foundation.GetComponent<FoundationScript>().cardList);
        CardListGlowOff(WastepileScript.Instance.cardList);
    }

    /// <summary>
    /// UnHighlights all tokens/cards in the given list.
    /// </summary>
    private void CardListGlowOff(List<GameObject> cardList)
    {
        foreach (GameObject card in cardList)
            card.GetComponent<CardScript>().GlowOff();
    }

    /// <summary>
    /// Sets the move counter text according to the given commands instructions.
    /// </summary>
    private void SetMoveCounterText(List<string> command)
    {
        Debug.Log("setting move counter text");

        CheckCommandCount(command, 2);

        UtilsScript.Instance.moveCounter.GetComponent<ActionCountScript>().UpdateActionText(command[1]);
    }

    /// <summary>
    /// Checks to see if the given command's count equals the given requirement.
    /// Throws a FormatException if the command count does not meet the requirement.
    /// </summary>
    private void CheckCommandCount(List<string> command, int minRequirement, int maxRequirement = 0)
    {
        if (maxRequirement == 0)
        {
            if (command.Count != minRequirement)
                throw new FormatException($"does not contain exactly {minRequirement} entries");
            else
                return;
        }

        if (command.Count < minRequirement)
            throw new FormatException($"contains less than {minRequirement} entries");

        if (command.Count > maxRequirement)
            throw new FormatException($"contains more than {maxRequirement} entries");
    }

    /// <summary>
    /// Parses the given command index for "on" or "off" and returns the bool result.
    /// Ignores case and throws a FormatException if the command is not valid.
    /// </summary>
    private bool ParseOnOrOff(List<string> command, byte index)
    {
        if (command[index].Equals("on", StringComparison.OrdinalIgnoreCase))
            return true;

        if (command[index].Equals("off", StringComparison.OrdinalIgnoreCase))
            return false;

        throw new FormatException($"does not properly specify neither \"on\" nor \"off\" for command #{index}");
    }

    /// <summary>
    /// Parses the given command's index for a reactor alert level and returns it.
    /// </summary>
    private byte ParseAlertLevel(List<string> command, byte index)
    {
        byte alertLevel;
        try
        {
            alertLevel = byte.Parse(command[index]);
        }
        catch (FormatException)
        {
            throw new FormatException($"does not contain a byte for command #{index}");
        }
        if (alertLevel != 1 && alertLevel != 2) // there are only two valid alert levels for reactors
        {
            throw new FormatException($"does not properly specify either \"1\" nor \"2\" for command #{index}");
        }

        return alertLevel;
    }

    /// <summary>
    /// Parses the given command's index for a container game object (where tokens/cards reside) index and returns it.
    /// </summary>
    private byte ParseContainerIndex(List<string> command, byte index)
    {
        byte parsedIndex;
        try
        {
            parsedIndex = byte.Parse(command[index]);
        }
        catch (FormatException)
        {
            throw new FormatException($"does not contain a byte for command #{index}");
        }
        if (index < 0 || index > 3) // there are only 4 reactors and 4 foundations
        {
            throw new FormatException($"does not properly specify a number between 0 and 3 for command #{index}");
        }

        return parsedIndex;
    }
}
