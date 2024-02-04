using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
    private static readonly Regex sWhitespace = new Regex(@"\s+");
    private static readonly WaitForSeconds nextButtonDelay = new(1);

    private Queue<List<string>> commandQueue;
    private bool waiting;

    [SerializeField]
    private Button undoButton, pauseButton, tutorialNextButton;

    [SerializeField]
    private GameObject tutorialUIPanel, tutorialText,
        tutorialReactors, tutorialScore, tutorialMoveCounter,
        tutorialUndo, tutorialPause,
        tutorialFoundations, tutorialDeck, tutorialWastePile;

    private void Awake()
    {
        // this is the gateway to turn the tutorial on
        if (!Config.Instance.TutorialOn) return;
        Debug.Log("starting the tutorial");

        // start the tutorial
        tutorialUIPanel.SetActive(true);
        // update colors via script instead of having to do it in editor each time
        UpdateHighlightObjectsColor(Config.Instance.CurrentColorMode.Notify.GlowColor);

        // get the tutorial commands ready
        commandQueue = CommandEnqueuer(CreateFromJSON(Config.Instance.TutorialFileName));

        // start the tutorial
        waiting = false;
        NextStep();
    }

    private void Start()
    {
        if (!Config.Instance.TutorialOn) return;

        // prevent the user from interacting with buttons during the tutorial
        DeckScript.Instance.ButtonReady = false;
        NextCycle.Instance.ButtonReady = false;
        undoButton.interactable = false;
        pauseButton.interactable = false;
    }

    /// <summary>
    /// Continues the tutorial if it's ready. The tutorial next button calls this.
    /// </summary>
    public void NextButton()
    {
        Debug.Log("tutorial touch");

        if (waiting && !GameInput.Instance.InputStopped)
        {
            waiting = false;
            SoundEffectsController.Instance.ButtonPressSound();
            NextStep();
        }
    }

    /// <summary>
    /// Ends the tutorial by request from the tutorial exit button.
    /// </summary>
    public void ExitButton()
    {
        Debug.Log("exit tutorial requested");
        SoundEffectsController.Instance.ButtonPressSound();
        SceneManager.LoadScene(Constants.ScenesNames.mainMenu);
        MusicController.Instance.MainMenuMusic();
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
    private static List<Command> CreateFromJSON(string fileNameToLoad)
    {
        Debug.Log("creating list from JSON");
        string filePath = Constants.Tutorial.tutorialResourcePath + fileNameToLoad;
        TextAsset jsonTextFile = Resources.Load<TextAsset>(filePath);
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
            switch (NormalizeString(command[0]))
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
                case "CHANGETOKENPLACEMENT":
                    ChangeTokenPlacement(command);
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
        GameInput.Instance.ShowPossibleMoves.TokenMoveable = true;

        Config.Instance.SetTutorialOff();
        Config.Instance.SetDifficulty(Difficulties.easy);
        MusicController.Instance.GameMusic();
        GameInput.Instance.InputStopped = true;
        GameLoader.Instance.RestartGame();
        GameInput.Instance.InputStopped = false;

        DeckScript.Instance.ButtonReady = true;
        NextCycle.Instance.ButtonReady = true;
        undoButton.interactable = true;
        pauseButton.interactable = true;
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
        yield return nextButtonDelay;
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
        switch (NormalizeString(command[1]))
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
        // 0:ChangeContainerHighlight, 1:Reactor(s) or Foundation(s),     2:On/Off,           3:0-Count, 4:0-4 (inclusive)

        Debug.Log("highlighting container(s)");

        CheckCommandCount(command, 5);

        // get if the highlight needs to turned on or off 
        bool highlightOn = ParseOnOrOff(command, 2);

        // get the containers index
        int index = ParseContainerIndex(command, 3);

        // get the highlight color level
        HighLightColor highlightColor = ParseHighlightColor(command, 4);

        // find the container
        switch (NormalizeString(command[1]))
        {
            case sReactors:
                if (highlightOn)
                {
                    foreach (ReactorScript reactorScript in GameInput.Instance.reactorScripts)
                    {
                        reactorScript.GlowColor = highlightColor;
                    }
                }
                else
                {
                    foreach (ReactorScript reactorScript in GameInput.Instance.reactorScripts)
                    {
                        reactorScript.Glowing = false;
                    }
                }
                break;
            case sReactor:
                if (highlightOn)
                {
                    GameInput.Instance.reactorScripts[index].GlowColor = highlightColor;
                    if (highlightColor.ColorLevel == Constants.ColorLevel.Over)
                    {
                        GameInput.Instance.reactorScripts[index].Alert = true;
                    }
                }
                else
                {
                    GameInput.Instance.reactorScripts[index].Glowing = false;
                    GameInput.Instance.reactorScripts[index].Alert = false;
                }
                break;
            case sFoundations:
                if (highlightOn)
                {
                    foreach (FoundationScript foundationScript in GameInput.Instance.foundationScripts)
                    {
                        foundationScript.GlowColor = highlightColor;
                    }
                }
                else
                {
                    foreach (FoundationScript foundationScript in GameInput.Instance.foundationScripts)
                    {
                        foundationScript.Glowing = false;
                    }
                }
                break;
            case sFoundation:
                if (highlightOn)
                {
                    GameInput.Instance.foundationScripts[index].GlowColor = highlightColor;
                }
                else
                {
                    GameInput.Instance.foundationScripts[index].Glowing = false;
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
        // 0:ChangeTokenHighlight, 1:Card Container,               2:Container Index, 3:Card Index, 4:Highlight On/Off, 5:Highlight Color Level
        // 0:ChangeTokenHighlight, 1:Reactor-Foundation-WastePile, 2:0-1-2-3,         3:0-Count,    4:On-Off,           5:0-4 (inclusive)

        Debug.Log("highlighting token");

        CheckCommandCount(command, 6);

        // 2nd command
        int containerIndex = ParseContainerIndex(command, 2);

        // 3rd command
        int tokenIndex = ParseTokenIndex(command, 3);

        // 4th command 
        bool highlightOn = ParseOnOrOff(command, 4);

        // get the highlight color level
        HighLightColor highlightColor = ParseHighlightColor(command, 5);

        // find the desired token's location
        switch (NormalizeString(command[1]))
        {
            case sReactor:
                List<GameObject> reactorCardList = GameInput.Instance.reactorScripts[containerIndex].CardList;
                if (reactorCardList.Count < tokenIndex - 1)
                {
                    throw new FormatException($"contains an out of bounds token index for command #3. " +
                        $"there are only {reactorCardList.Count} token(s) to choose from in reactor {containerIndex}");
                }

                if (highlightOn)
                {
                    reactorCardList[^tokenIndex].GetComponent<CardScript>().GlowColor = highlightColor;
                }
                else
                {
                    reactorCardList[^tokenIndex].GetComponent<CardScript>().Glowing = false;
                }
                break;
            case sFoundation:
                List<GameObject> foundationCardList = GameInput.Instance.foundationScripts[containerIndex].CardList;
                if (foundationCardList.Count < tokenIndex - 1)
                {
                    throw new FormatException($"contains an out of bounds token index for command #3. " +
                        $"there are only {foundationCardList.Count} token(s) to choose from in foundation {containerIndex}");
                }
                if (highlightOn)
                {
                    foundationCardList[^tokenIndex].GetComponent<CardScript>().GlowColor = highlightColor;
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
                    WastepileScript.Instance.CardList[^tokenIndex].GetComponent<CardScript>().GlowColor = highlightColor;
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
        switch (NormalizeString(command[1]))
        {
            case sReactor:
                List<GameObject> reactorCardList = GameInput.Instance.reactorScripts[containerIndex].CardList;
                if (reactorCardList.Count < tokenIndex - 1)
                {
                    throw new FormatException($"contains an out of bounds token index for command #3. " +
                        $"there are only {reactorCardList.Count} token(s) to choose from in reactor {containerIndex}");
                }
                reactorCardList[^tokenIndex].GetComponent<CardScript>().Obstructed = obstructed;
                break;
            case sFoundation:
                List<GameObject> foundationCardList = GameInput.Instance.foundationScripts[containerIndex].CardList;
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

        foreach (ReactorScript reactorScript in GameInput.Instance.reactorScripts)
        {
            // only the top reactor token/card is ever not obstructed
            if (reactorScript.CardList.Count != 0)
            {
                reactorScript.CardList[^1].GetComponent<CardScript>().Obstructed = true;
            }
        }

        foreach (FoundationScript foundationScript in GameInput.Instance.foundationScripts)
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
        GameInput.Instance.ShowPossibleMoves.ReactorObstructed = obstructed;
    }

    private void ChangeTokenMoveability(List<string> command)
    {
        CheckCommandCount(command, 2);

        Debug.Log($"changing token moveability: {command[1]}");

        bool moveability = ParseOnOrOff(command, 1);
        GameInput.Instance.ShowPossibleMoves.TokenMoveable = moveability;
    }

    private void ChangeTokenPlacement(List<string> command)
    {
        CheckCommandCount(command, 2);

        Debug.Log($"changing token placement: {command[1]}");

        bool placemenmt = ParseOnOrOff(command, 1);
        GameInput.Instance.CardPlacement = placemenmt;
    }

    private void ChangeButtonInteractable(List<string> command)
    {
        CheckCommandCount(command, 3);

        Debug.Log($"changing button interactable: {command[1]}");

        bool interactable = ParseOnOrOff(command, 2);

        switch (NormalizeString(command[1]))
        {
            case "DECK":
                DeckScript.Instance.ButtonReady = interactable;
                break;
            case "UNDO":
                undoButton.interactable = interactable;
                break;
            case "TIMER":
                NextCycle.Instance.ButtonReady = interactable;
                break;
            default:
                throw new FormatException("contains an invalid button for command #2");
        }
    }

    private void EnableOneNextCycle()
    {
        Debug.Log($"enabling one next cycle");
        NextCycle.Instance.ButtonReady = true;
    }

    /// <summary>
    /// UnHighlights all tokens/cards.
    /// </summary>
    private void RemoveAllTokenHighlight()
    {
        Debug.Log("unhighlighting all tokens");

        foreach (ReactorScript reactorScript in GameInput.Instance.reactorScripts)
        {
            CardListGlowOff(reactorScript.CardList);
        }
        foreach (FoundationScript foundationScript in GameInput.Instance.foundationScripts)
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
    /// Take a string and returns that string without any whitespace and in uppercase 
    /// </summary>
    private string NormalizeString(string toNormalize)
    {
        return sWhitespace.Replace(toNormalize, "").ToUpper();
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
    private HighLightColor ParseHighlightColor(List<string> command, int index)
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
        if (!Enum.IsDefined(typeof(Constants.ColorLevel), colorLevel))
        {
            throw new FormatException($"does not contain a valid color level for command #{index}");
        }

        return colorLevel switch
        {
            (int)Constants.ColorLevel.None => GameValues.Colors.normal,
            (int)Constants.ColorLevel.Match => Config.Instance.CurrentColorMode.Match,
            (int)Constants.ColorLevel.Move => Config.Instance.CurrentColorMode.Move,
            (int)Constants.ColorLevel.Over => Config.Instance.CurrentColorMode.Over,
            (int)Constants.ColorLevel.Notify => Config.Instance.CurrentColorMode.Notify,
            _ => throw new FormatException($"the color level of \"{colorLevel}\" was not found for command #{index}")
        };
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
