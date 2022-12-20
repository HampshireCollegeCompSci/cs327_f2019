using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialScript : MonoBehaviour
{
    // command names for switch usage
    private const string sReactors = "REACTORS";
    private const string sReactor = "REACTOR";
    private const string sFoundations = "FOUNDATIONS";
    private const string sFoundation = "FOUNDATION";
    private const string sWastepile = "WASTEPILE";

    private Queue<List<string>> commandQueue;
    private bool waiting;

    [SerializeField]
    private Button deckButton, undoButton, timerButton, pauseButton, tutorialNextButton;

    [SerializeField]
    private GameObject tutorialUIPanel, tutorialText,
        tutorialReactors, tutorialScore, tutorialMoveCounter,
        tutorialUndo, tutorialPause,
        tutorialFoundations, tutorialDeck, tutorialWastePile;

    private void Awake()
    {
        // this is the gateway to turn the tutorial on
        if (Config.Instance.tutorialOn)
        {
            StartTutorial();
        }
    }

    private void StartTutorial()
    {
        Debug.Log("starting the tutorial");

        // prevent the user from interacting with buttons during the tutorial
        deckButton.interactable = false;
        undoButton.interactable = false;
        timerButton.interactable = false;
        pauseButton.interactable = false;

        // start the tutorial
        tutorialUIPanel.SetActive(true);
        // update colors via script instead of having to do it in editor each time
        UpdateHighlightObjectsColor(Config.GameValues.tutorialObjectHighlightColor);

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
        tutorialReactors.GetComponent<SpriteRenderer>().color = newColor;
        tutorialScore.GetComponent<Image>().color = newColor;
        tutorialMoveCounter.GetComponent<Image>().color = newColor;
        tutorialUndo.GetComponent<Image>().color = newColor;
        tutorialPause.GetComponent<Image>().color = newColor;
        tutorialFoundations.GetComponent<SpriteRenderer>().color = newColor;
        tutorialDeck.GetComponent<Image>().color = newColor;
        tutorialWastePile.GetComponent<Image>().color = newColor;
    }

    /// <summary>
    /// Creates and returns a list of commands to follow from a JSON file.
    /// </summary>
    private static List<Command> CreateFromJSON()
    {
        Debug.Log("creating list from JSON");

        TextAsset jsonTextFile = Resources.Load<TextAsset>(Constants.Tutorial.tutorialCommandListFilePath);
        TutorialCommands commandFile = JsonUtility.FromJson<TutorialCommands>(jsonTextFile.ToString());
        return commandFile.commands;
    }

    /// <summary>
    /// Enqueues the command list into a queue.
    /// </summary>
    private Queue<List<string>> CommandEnqueuer(List<Command> commandList)
    {
        Debug.Log("creating command queue");

        Queue<List<string>> newQueue = new();

        foreach (Command command in commandList)
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

    [SerializeField]
    /// <summary>
    /// Continues the tutorial if it's ready. The tutorial next button calls this.
    /// </summary>
    private void NextButton()
    {
        Debug.Log("tutorial touch");

        if (waiting && !UtilsScript.Instance.InputStopped)
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
            switch (command[0].ToUpper())
            {
                case "ENDTUTORIAL":
                    EndTutorial();
                    break;
                case "WAITFORTOUCH":
                    WaitForTouch();
                    break;
                case "LOADSAVE":
                    LoadSave(command);
                    break;
                case "CHANGESHOWNTEXT":
                    ChangeShownText(command);
                    break;
                case "CHANGEOBJECTHIGHLIGHT":
                    ChangeObjectHighlight(command);
                    break;
                case "CHANGECONTAINERHIGHLIGHT":
                    ChangeContainerHighlight(command);
                    break;
                case "CHANGETOKENHIGHLIGHT":
                    ChangeTokenHighlight(command);
                    break;
                case "REMOVEALLTOKENHIGHLIGHT":
                    RemoveAllTokenHighlight();
                    break;
                case "CHANGETOKENOBSTRUCTION":
                    ChangeTokenObstruction(command);
                    break;
                case "CHANGEALLTOKENOBSTRUCTION":
                    ChangeAllTokenObstruction();
                    break;
                case "CHANGEREACTOROBSTRUCTION":
                    ChangeReactorObstruction(command);
                    break;
                case "CHANGETOKENMOVEABILITY":
                    ChangeTokenMoveability(command);
                    break;
                case "CHANGEBUTTONINTERACTABLE":
                    ChangeButtonInteractable(command);
                    break;
                case "ENABLEONENEXTCYCLE":
                    EnableOneNextCycle();
                    break;
                case "CHANGEMOVECOUNTERTEXT":
                    ChangeMoveCounterText(command);
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
        Debug.Log("ending tutorial");

        tutorialUIPanel.SetActive(false);
        UtilsScript.Instance.ShowPossibleMoves.TokenMoveable = true;

        Config.Instance.tutorialOn = false;
        Config.Instance.SetDifficulty(0);
        MusicController.Instance.GameMusic();
        GameLoader.Instance.RestartGame();

        deckButton.interactable = true;
        undoButton.interactable = true;
        timerButton.interactable = true;
        pauseButton.interactable = true;
    }

    [SerializeField]
    /// <summary>
    /// Ends the tutorial by request from the tutorial exit button.
    /// </summary>
    private void ExitButton()
    {
        Debug.Log("exit tutorial requested");
        SoundEffectsController.Instance.ButtonPressSound();
        SceneManager.LoadScene(Constants.ScenesNames.mainMenu);
        MusicController.Instance.MainMenuMusic();
    }

    /// <summary>
    /// Stops the interpreting of commands.
    /// </summary>
    private void WaitForTouch()
    {
        Debug.Log("waiting for touch");

        waiting = true;
        StartCoroutine(DelayNextButtonInteraction());
    }

    private System.Collections.IEnumerator DelayNextButtonInteraction()
    {
        tutorialNextButton.interactable = false;
        yield return new WaitForSeconds(1);
        tutorialNextButton.interactable = true;
    }

    /// <summary>
    /// Loads a savestate into the game.
    /// </summary>
    private void LoadSave(List<string> command)
    {
        CheckCommandCount(command, 2);

        string fileName = command[1];

        Debug.Log("loading save: " + fileName);
        GameLoader.Instance.LoadTutorial(fileName);
    }

    /// <summary>
    /// Displays the text in the command on screen in the tutorial box.
    /// </summary>
    private void ChangeShownText(List<string> command)
    {
        CheckCommandCount(command, 2);

        Debug.Log($"showing text: {command[1]}");

        tutorialText.GetComponent<Text>().text = command[1];
    }

    /// <summary>
    /// Changes the highlight state of a game object according to the given commands instructions.
    /// </summary>
    private void ChangeObjectHighlight(List<string> command)
    {
        // command format: 
        // 0:ChangeObjectHighlight, 1:Object to Highlight, 2:Highlight On/Off
        // 0:ChangeObjectHighlight, 1:MANY,                2:On/Off

        Debug.Log("highlighting object");

        CheckCommandCount(command, 3);

        // get if the highlight needs to turned on or off
        bool highlightOn = ParseOnOrOff(command, 2);

        // interpreting  1st command
        switch (command[1].ToUpper())
        {
            case sReactors:
                tutorialReactors.SetActive(highlightOn);
                break;
            case "SCOREBOARD":
                tutorialScore.SetActive(highlightOn);
                break;
            case "MOVECOUNTER":
                tutorialMoveCounter.SetActive(highlightOn);
                break;
            case "UNDO":
                tutorialUndo.SetActive(highlightOn);
                break;
            case "PAUSE":
                tutorialPause.SetActive(highlightOn);
                break;
            case sFoundations:
                tutorialFoundations.SetActive(highlightOn);
                break;
            case "DECK":
                tutorialDeck.SetActive(highlightOn);
                break;
            case sWastepile:
                tutorialWastePile.SetActive(highlightOn);
                break;
            default:
                throw new FormatException("does not contain a valid object to highlight for command #1");
        }
    }

    /// <summary>
    /// Changes the highlight state of a container game object (where tokens/cards reside) according to the given commands instructions.
    /// </summary>
    private void ChangeContainerHighlight(List<string> command)
    {
        // command format: 
        // 0:ChangeContainerHighlight, 1:Container(s) to Highlight,       2:Highlight On/Off, 3:Index,   4:Highlight Color Level
        // 0:ChangeContainerHighlight, 1:Reactor(s) or Foundation(s),     2:On/Off,           3:0-Count, 4:0-3 (inclusive)

        Debug.Log("highlighting container(s)");

        CheckCommandCount(command, 5);

        // get if the highlight needs to turned on or off 
        bool highlightOn = ParseOnOrOff(command, 2);

        // get the containers index
        int index = ParseContainerIndex(command, 3);

        // get the highlight color level
        int highlightColorLevel = ParseHighlightColorLevel(command, 4);

        // find the container
        switch (command[1].ToUpper())
        {
            case sReactors:
                if (highlightOn)
                {
                    foreach (ReactorScript reactorScript in UtilsScript.Instance.reactorScripts)
                    {
                        reactorScript.GlowLevel = highlightColorLevel;
                    }
                }
                else
                {
                    foreach (ReactorScript reactorScript in UtilsScript.Instance.reactorScripts)
                    {
                        reactorScript.Glowing = false;
                    }
                }
                break;
            case sReactor:
                if (highlightOn)
                {
                    UtilsScript.Instance.reactorScripts[index].GlowLevel = highlightColorLevel;
                    if (highlightColorLevel == Constants.HighlightColorLevel.over)
                    {
                        UtilsScript.Instance.reactorScripts[index].Alert = true;
                    }
                }
                else
                {
                    UtilsScript.Instance.reactorScripts[index].Glowing = false;
                    UtilsScript.Instance.reactorScripts[index].Alert = false;
                }
                break;
            case sFoundations:
                if (highlightOn)
                {
                    foreach (FoundationScript foundationScript in UtilsScript.Instance.foundationScripts)
                    {
                        foundationScript.GlowLevel = highlightColorLevel;
                    }
                }
                else
                {
                    foreach (FoundationScript foundationScript in UtilsScript.Instance.foundationScripts)
                    {
                        foundationScript.Glowing = false;
                    }
                }
                break;
            case sFoundation:
                if (highlightOn)
                {
                    UtilsScript.Instance.foundationScripts[index].GlowLevel = highlightColorLevel;
                }
                else
                {
                    UtilsScript.Instance.foundationScripts[index].Glowing = false;
                }
                break;
            default:
                throw new FormatException("does not contain a valid container to highlight for command #1");
        }
    }

    /// <summary>
    /// Changes the highlight state of a token/card game object according to the given commands instructions.
    /// </summary>
    private void ChangeTokenHighlight(List<string> command)
    {
        // command format: 
        // 0:ChangeTokenHighlight, 1:Object(s) Containing Token,   2:Object Index, 3:Token Index, 4:Highlight On/Off, 5:Highlight Color Level
        // 0:ChangeTokenHighlight,  1:Reactor-Foundation-WastePile, 2:0-1-2-3,      3:0-Count,     4:On-Off,           5:0-3 (inclusive)

        Debug.Log("highlighting token");

        CheckCommandCount(command, 6);

        // 2nd command
        int containerIndex = ParseContainerIndex(command, 2);

        // 3rd command
        int tokenIndex = ParseTokenIndex(command, 3);

        // 4th command 
        bool highlightOn = ParseOnOrOff(command, 4);

        // get the highlight color level
        int highlightColorLevel = ParseHighlightColorLevel(command, 5);

        // find the desired token's location
        switch (command[1].ToUpper())
        {
            case sReactor:
                List<GameObject> reactorCardList = UtilsScript.Instance.reactorScripts[containerIndex].CardList;
                if (reactorCardList.Count < tokenIndex - 1)
                {
                    throw new FormatException($"contains an out of bounds token index for command #3. " +
                        $"there are only {reactorCardList.Count} token(s) to choose from in reactor {containerIndex}");
                }

                if (highlightOn)
                {
                    reactorCardList[^tokenIndex].GetComponent<CardScript>().GlowLevel = highlightColorLevel;
                }
                else
                {
                    reactorCardList[^tokenIndex].GetComponent<CardScript>().Glowing = false;
                }
                break;
            case sFoundation:
                List<GameObject> foundationCardList = UtilsScript.Instance.foundationScripts[containerIndex].CardList;
                if (foundationCardList.Count < tokenIndex - 1)
                {
                    throw new FormatException($"contains an out of bounds token index for command #3. " +
                        $"there are only {foundationCardList.Count} token(s) to choose from in foundation {containerIndex}");
                }
                if (highlightOn)
                {
                    foundationCardList[^tokenIndex].GetComponent<CardScript>().GlowLevel = highlightColorLevel;
                }
                else
                {
                    foundationCardList[^tokenIndex].GetComponent<CardScript>().Glowing = false;
                }
                break;
            case sWastepile:
                if (WastepileScript.Instance.CardList.Count < tokenIndex - 1)
                {
                    throw new FormatException($"contains an out of bounds token index for command #3. " +
                        $"there are only {WastepileScript.Instance.CardList.Count} token(s) to choose from in the waste pile");
                }

                if (highlightOn)
                {
                    WastepileScript.Instance.CardList[^tokenIndex].GetComponent<CardScript>().GlowLevel = highlightColorLevel;
                }
                else
                {
                    WastepileScript.Instance.CardList[^tokenIndex].GetComponent<CardScript>().Glowing = false;
                }
                break;
            default:
                throw new FormatException("contains an invalid object that contains the token for command #1");
        }
    }

    /// <summary>
    /// Changes the obstruction state of a token/card game object according to the given commands instructions.
    /// </summary>
    private void ChangeTokenObstruction(List<string> command)
    {
        // command format: 
        // 0:ChangeTokenObstruction,  1:Object(s) Containing Token,   2:Object Index, 3:Token Index, 4:Obstruction On/Off
        // 0:ChangeTokenObstruction,  1:Reactor-Foundation-WastePile, 2:0-1-2-3,      3:0-Count,     4:On-Off

        Debug.Log("changing token obstruction");

        CheckCommandCount(command, 5);

        // 2nd command
        int containerIndex = ParseContainerIndex(command, 2);

        // 3rd command
        int tokenIndex = ParseTokenIndex(command, 3);

        // 4th command 
        bool obstructed = ParseOnOrOff(command, 4);

        // find the desired token's location
        switch (command[1].ToUpper())
        {
            case sReactor:
                List<GameObject> reactorCardList = UtilsScript.Instance.reactorScripts[containerIndex].CardList;
                if (reactorCardList.Count < tokenIndex - 1)
                {
                    throw new FormatException($"contains an out of bounds token index for command #3. " +
                        $"there are only {reactorCardList.Count} token(s) to choose from in reactor {containerIndex}");
                }
                reactorCardList[^tokenIndex].GetComponent<CardScript>().Obstructed = obstructed;
                break;
            case sFoundation:
                List<GameObject> foundationCardList = UtilsScript.Instance.foundationScripts[containerIndex].CardList;
                if (foundationCardList.Count < tokenIndex - 1)
                {
                    throw new FormatException($"contains an out of bounds token index for command #3. " +
                        $"there are only {foundationCardList.Count} token(s) to choose from in foundation {containerIndex}");
                }
                foundationCardList[^tokenIndex].GetComponent<CardScript>().Obstructed = obstructed;
                break;
            case sWastepile:
                if (WastepileScript.Instance.CardList.Count < tokenIndex - 1)
                {
                    throw new FormatException($"contains an out of bounds token index for command #3. " +
                        $"there are only {WastepileScript.Instance.CardList.Count} token(s) to choose from in the waste pile");
                }
                WastepileScript.Instance.CardList[^tokenIndex].GetComponent<CardScript>().Obstructed = obstructed;
                break;
            default:
                throw new FormatException("contains an invalid object that contains the token for command #1");
        }
    }

    private void ChangeAllTokenObstruction()
    {
        Debug.Log("Obstructing all tokens");

        foreach (ReactorScript reactorScript in UtilsScript.Instance.reactorScripts)
        {
            // only the top reactor token/card is ever not obstructed
            if (reactorScript.CardList.Count != 0)
            {
                reactorScript.CardList[^1].GetComponent<CardScript>().Obstructed = true;
            }
        }

        foreach (FoundationScript foundationScript in UtilsScript.Instance.foundationScripts)
        {
            // from the top of the list downwards, obstruct until a hidden card is reached
            for (int i = foundationScript.CardList.Count - 1; i >= 0; i--)
            {
                if (foundationScript.CardList[i].GetComponent<CardScript>().Hidden) break;
                foundationScript.CardList[i].GetComponent<CardScript>().Obstructed = true;
            }
        }

        // only the top wastepile token/card is ever not obstructed
        if (WastepileScript.Instance.CardList.Count != 0)
        {
            WastepileScript.Instance.CardList[^1].GetComponent<CardScript>().Obstructed = true;
        }
    }

    private void ChangeReactorObstruction(List<string> command)
    {
        CheckCommandCount(command, 2);

        Debug.Log($"changing reactor obstruction: {command[1]}");

        bool obstructed = ParseOnOrOff(command, 1);
        UtilsScript.Instance.ShowPossibleMoves.ReactorObstructed = obstructed;
    }

    private void ChangeTokenMoveability(List<string> command)
    {
        CheckCommandCount(command, 2);

        Debug.Log($"changing token moveability: {command[1]}");

        bool moveability = ParseOnOrOff(command, 1);
        UtilsScript.Instance.ShowPossibleMoves.TokenMoveable = moveability;
    }

    private void ChangeButtonInteractable(List<string> command)
    {
        CheckCommandCount(command, 3);

        Debug.Log($"changing button interactable: {command[1]}");

        bool interactable = ParseOnOrOff(command, 2);

        switch (command[1].ToUpper())
        {
            case "DECK":
                deckButton.interactable = interactable;
                break;
            case "UNDO":
                undoButton.interactable = interactable;
                break;
            case "TIMER":
                timerButton.interactable = interactable;
                break;
            default:
                throw new FormatException("contains an invalid button for command #2");
        }
    }

    private void EnableOneNextCycle()
    {
        Debug.Log($"enabling one next cycle");

        Config.Instance.nextCycleEnabled = true;
    }

    /// <summary>
    /// UnHighlights all tokens/cards.
    /// </summary>
    private void RemoveAllTokenHighlight()
    {
        Debug.Log("unhighlighting all tokens");

        foreach (ReactorScript reactorScript in UtilsScript.Instance.reactorScripts)
        {
            CardListGlowOff(reactorScript.CardList);
        }
        foreach (FoundationScript foundationScript in UtilsScript.Instance.foundationScripts)
        {
            CardListGlowOff(foundationScript.CardList);
        }
        CardListGlowOff(WastepileScript.Instance.CardList);
        CardListGlowOff(DeckScript.Instance.CardList);
        CardListGlowOff(MatchedPileScript.Instance.CardList);
    }

    /// <summary>
    /// UnHighlights all tokens/cards in the given list.
    /// </summary>
    private void CardListGlowOff(List<GameObject> cardList)
    {
        foreach (GameObject card in cardList)
        {
            card.GetComponent<CardScript>().Glowing = false;
        }
    }

    /// <summary>
    /// Sets the move counter text according to the given commands instructions.
    /// </summary>
    private void ChangeMoveCounterText(List<string> command)
    {
        Debug.Log("setting move counter text");

        CheckCommandCount(command, 2);

        ActionCountScript.Instance.UpdateActionText(command[1]);
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
            {
                throw new FormatException($"does not contain exactly {minRequirement} entries");
            }
            else
            {
                return;
            }
        }

        if (command.Count < minRequirement)
        {
            throw new FormatException($"contains less than {minRequirement} entries");
        }

        if (command.Count > maxRequirement)
        {
            throw new FormatException($"contains more than {maxRequirement} entries");
        }
    }

    /// <summary>
    /// Parses the given command index for "on" or "off" and returns the bool result.
    /// Ignores case and throws a FormatException if the command is not valid.
    /// </summary>
    private bool ParseOnOrOff(List<string> command, int index)
    {
        if (command[index].Equals("on", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (command[index].Equals("off", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        throw new FormatException($"does not properly specify neither \"on\" nor \"off\" for command #{index}");
    }

    /// <summary>
    /// Parses the given command's index for a reactor alert level and returns it.
    /// </summary>
    private int ParseHighlightColorLevel(List<string> command, int index)
    {
        int colorLevel;
        try
        {
            colorLevel = int.Parse(command[index]);
        }
        catch (FormatException)
        {
            throw new FormatException($"does not contain a int for command #{index}");
        }
        if (colorLevel > 3) // there are only three color levels available
        {
            throw new FormatException($"does not properly specify between \"0\" to \"3\" (inclusive) for command #{index}");
        }

        return colorLevel;
    }

    /// <summary>
    /// Parses the given command's index for a container game object (where tokens/cards reside) index and returns it.
    /// </summary>
    private int ParseContainerIndex(List<string> command, int index)
    {
        int parsedIndex;
        try
        {
            parsedIndex = int.Parse(command[index]);
        }
        catch (FormatException)
        {
            throw new FormatException($"does not contain a int for command #{index}");
        }
        if (index < 0 || index > 3) // there are only 4 reactors and 4 foundations
        {
            throw new FormatException($"does not properly specify a number between 0 and 3 for command #{index}");
        }

        return parsedIndex;
    }

    /// <summary>
    /// Parses the given command's index for a token/card index and returns it.
    /// </summary>
    private int ParseTokenIndex(List<string> command, int index)
    {
        int tokenIndex;
        try
        {
            tokenIndex = int.Parse(command[index]);
        }
        catch (FormatException)
        {
            throw new FormatException($"does not contain an int for command #{index}");
        }
        if (tokenIndex < 0)
        {
            throw new FormatException($"contains a negative int for command #{index}");
        }

        // tutorial input index is based on the top index = 0, in-game it's the opposite
        // this will used in list[list.Count - tokenIndex]
        return tokenIndex + 1;
    }
}
