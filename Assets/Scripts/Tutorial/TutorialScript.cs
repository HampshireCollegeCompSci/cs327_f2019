using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialScript : MonoBehaviour
{
    private Queue<List<string>> commandQueue;
    private bool waiting;

    public Button[] gameplayButtons;
    public Button tutorialNextButton;

    public GameObject tutorialUIPanel, tutorialText;
    public GameObject tutorialReactors, tutorialScore, tutorialMoveCounter;
    public GameObject tutorialUndo, tutorialPause;
    public GameObject tutorialFoundations, tutorialDeck, tutorialWastePile;

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

        // prevent the user from interacting with anything but the tutorial
        UtilsScript.Instance.SetInputStopped(true);
        foreach (Button button in gameplayButtons)
        {
            button.interactable = false;
        }

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
    private static List<ArgumentListWrapper> CreateFromJSON()
    {
        Debug.Log("creating list from JSON");

        TextAsset jsonTextFile = Resources.Load<TextAsset>(Constants.tutorialCommandListFilePath);
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
        Debug.Log("ending tutorial");

        tutorialUIPanel.SetActive(false);
        foreach (Button button in gameplayButtons)
        {
            button.interactable = true;
        }

        Config.Instance.tutorialOn = false;
        Config.Instance.SetDifficulty(0);
        MusicController.Instance.GameMusic();
        GameLoader.Instance.RestartGame();
    }

    /// <summary>
    /// Ends the tutorial by request from the tutorial exit button.
    /// </summary>
    public void ExitButton()
    {
        Debug.Log("exit tutorial requested");
        SoundEffectsController.Instance.ButtonPressSound();
        SceneManager.LoadScene(Constants.mainMenuScene);
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
        // 0:HighlightContainer, 1:Container(s) to Highlight,       2:Highlight On/Off, 3:Index,   4:Highlight Color Level
        // 0:HighlightContainer, 1:Reactor(s) or Foundation(s),     2:On/Off,           3:0-Count, 4:0-3 (inclusive)

        Debug.Log("highlighting container(s)");

        CheckCommandCount(command, 5);

        // get if the highlight needs to turned on or off 
        bool highlightOn = ParseOnOrOff(command, 2);

        // get the containers index
        byte index = ParseContainerIndex(command, 3);

        // get the highlight color level
        byte highlightColorLevel = ParseHighlightColorLevel(command, 4);

        // find the container
        switch (command[1].ToLower())
        {
            case "reactors":
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
            case "reactor":
                if (highlightOn)
                {
                    UtilsScript.Instance.reactorScripts[index].GlowLevel = highlightColorLevel;
                }
                else
                {
                    UtilsScript.Instance.reactorScripts[index].Glowing = false;
                }
                break;
            case "foundations":
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
            case "foundation":
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
    private void HighlightToken(List<string> command)
    {
        // command format: 
        // 0:Highlight Token, 1:Object(s) Containing Token,   2:Object Index, 3:Token Index, 4:Highlight On/Off, 5:Highlight Color Level
        // 0:HighlightToken,  1:Reactor-Foundation-WastePile, 2:0-1-2-3,      3:0-Count,     4:On-Off,           5:0-3 (inclusive)

        Debug.Log("highlighting token");

        CheckCommandCount(command, 6);

        // 2nd command
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

        // get if the highlight needs to turned on or off 
        bool highlightOn = ParseOnOrOff(command, 4);

        // get the highlight color level
        byte highlightColorLevel = ParseHighlightColorLevel(command, 5);

        // find the desired token's location
        switch (command[1].ToLower())
        {
            case "reactor":
                List<GameObject> reactorCardList = UtilsScript.Instance.reactorScripts[containerIndex].cardList;
                if (reactorCardList.Count < tokenIndex)
                {
                    throw new FormatException($"contains an out of bounds token index for command #3. " +
                        $"there are only {reactorCardList.Count} token(s) to choose from in reactor {containerIndex}");
                }

                if (highlightOn)
                {
                    reactorCardList[tokenIndex].GetComponent<CardScript>().GlowLevel = highlightColorLevel;
                }
                else
                {
                    reactorCardList[tokenIndex].GetComponent<CardScript>().Glowing = false;
                }
                break;
            case "foundation":
                List<GameObject> foundationCardList = UtilsScript.Instance.foundationScripts[containerIndex].cardList;
                if (foundationCardList.Count < tokenIndex)
                {
                    throw new FormatException($"contains an out of bounds token index for command #3. " +
                        $"there are only {foundationCardList.Count} token(s) to choose from in foundation {containerIndex}");
                }
                if (highlightOn)
                {
                    foundationCardList[tokenIndex].GetComponent<CardScript>().GlowLevel = highlightColorLevel;
                }
                else
                {
                    foundationCardList[tokenIndex].GetComponent<CardScript>().Glowing = false;
                }
                break;
            case "wastepile":
                if (WastepileScript.Instance.cardList.Count < tokenIndex)
                {
                    throw new FormatException($"contains an out of bounds token index for command #3. " +
                        $"there are only {WastepileScript.Instance.cardList.Count} token(s) to choose from in the waste pile");
                }

                if (highlightOn)
                {
                    WastepileScript.Instance.cardList[tokenIndex].GetComponent<CardScript>().GlowLevel = highlightColorLevel;
                }
                else
                {
                    WastepileScript.Instance.cardList[tokenIndex].GetComponent<CardScript>().Glowing = false;
                }
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

        // each token/card on the top of every reactor can be interacted with
        foreach (ReactorScript reactorScript in UtilsScript.Instance.reactorScripts)
        {
            if (reactorScript.cardList.Count != 0)
            {
                reactorScript.cardList[0].GetComponent<CardScript>().GlowLevel = Constants.moveHighlightColorLevel;
            }
        }

        // all tokens/cards that are not hidden in the foundations can be interacted with
        foreach (FoundationScript foundationScript in UtilsScript.Instance.foundationScripts)
        {
            if (foundationScript.cardList.Count != 0)
            {   
                foreach(GameObject card in foundationScript.cardList)
                {
                    CardScript cardScriptRef = card.GetComponent<CardScript>();
                    
                    // if this token/card is hidden all subsequents will be too
                    if (cardScriptRef.IsHidden)
                    {
                        break;
                    }

                    cardScriptRef.GlowLevel = Constants.moveHighlightColorLevel;
                }
            }
        }

        // the first wastepile token/card can always be interacted with
        if (WastepileScript.Instance.cardList.Count != 0)
        {
            WastepileScript.Instance.cardList[0].GetComponent<CardScript>().GlowLevel = Constants.moveHighlightColorLevel;
        }
    }

    /// <summary>
    /// UnHighlights all tokens/cards.
    /// </summary>
    private void UnHighlightAllTokens()
    {
        Debug.Log("unhighlighting all tokens");

        foreach (ReactorScript reactorScript in UtilsScript.Instance.reactorScripts)
        {
            CardListGlowOff(reactorScript.cardList);
        }
        foreach (FoundationScript foundationScript in UtilsScript.Instance.foundationScripts)
        {
            CardListGlowOff(foundationScript.cardList);
        }
        CardListGlowOff(WastepileScript.Instance.cardList);
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
    private void SetMoveCounterText(List<string> command)
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
    private bool ParseOnOrOff(List<string> command, byte index)
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
    private byte ParseHighlightColorLevel(List<string> command, byte index)
    {
        byte colorLevel;
        try
        {
            colorLevel = byte.Parse(command[index]);
        }
        catch (FormatException)
        {
            throw new FormatException($"does not contain a byte for command #{index}");
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
