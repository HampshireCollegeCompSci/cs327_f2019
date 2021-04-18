using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialScript : MonoBehaviour
{
    private List<ArgumentListWrapper> commandList;
    private Queue<List<string>> commandQueue;
    private bool waiting;

    public GameObject tutorial, tutorialText;
    public GameObject tutorialReactors, tutorialScore, tutorialMoveCounter;
    public GameObject tutorialUndo, tutorialPause;
    public GameObject tutorialFoundations, tutorialDeck, tutorialWastePile;

    private void Awake()
    {
        if (!Config.config.tutorialOn)
            return;

        Debug.Log("starting tutorialScript");

        UtilsScript.global.SetInputStopped(true);
        tutorial.SetActive(true);

        commandQueue = new Queue<List<string>>();
        commandList = CreateFromJSON();
        CommandReader(commandList);
        waiting = false;
        NextStep();
    }

    private void CommandReader(List<ArgumentListWrapper> commandList)
    {
        Debug.Log("populating command queue");

        foreach (ArgumentListWrapper command in commandList)
        {
            commandQueue.Enqueue(command.argumentList);
        }
    }

    private void NextStep()
    {
        if (!waiting)
        {
            CommandInterpreter();
        }
    }

    public void Touch()
    {
        // the tutorial next button calls this

        Debug.Log("tutorial touch");

        if (waiting)
        {
            waiting = false;
            NextStep();
        }
    }

    private void CommandInterpreter()
    {
        Debug.Log("interpreting command");

        if (commandQueue.Count == 0)
        {
            Debug.LogError("tutorial command list does not end with \"EndTutorial\". ending tutorial now");
            EndTutorial();
            return;
        }

        List<string> command = commandQueue.Dequeue();
        try
        {
            if (command[0] == "LoadSave")
            {
                LoadSave(command);
            }
            else if (command[0] == "ShowText")
            {
                ShowText(command);
            }
            else if (command[0] == "WaitForTouch")
            {
                WaitForTouch();
            }
            else if (command[0] == "EndTutorial")
            {
                EndTutorial();
            }
            else if (command[0] == "HighlightObject")
            {
                HighlightObject(command);
            }
            else if (command[0] == "HighlightContainer")
            {
                HighlightContainer(command);
            }
            else if (command[0] == "HighlightToken")
            {
                HighlightToken(command);
            }
            else if (command[0] == "HighlightAllInteractableTokens")
            {
                HighlightAllInteractableTokens();
            }
            else if (command[0] == "UnHighlightAllTokens")
            {
                UnHighlightAllTokens();
            }
            else
            {
                throw new FormatException("does not contain a valid command request for the 0th command");
            }

            NextStep();
        }
        catch (FormatException e)
        {
            Debug.LogError($"tutorial command: \"{string.Join(", ", command)}\", {e.Message}");
        }
    }

    private void WaitForTouch()
    {
        Debug.Log("waiting for touch");

        waiting = true;
    }

    private void LoadSave(List<string> command)
    {
        if (command.Count != 2)
        {
            throw new FormatException("does not contain exactly 2 entries");
        }

        string fileName = command[1];

        Debug.Log("loading save: " + fileName);

        //move all tokens back to load pile, then call LoadState and UnpackState

        foreach (GameObject foundation in Config.config.foundations)
        {
            MoveCardsToLoadPile(foundation.GetComponent<FoundationScript>().cardList);
        }

        foreach (GameObject reactor in Config.config.reactors)
        {
            MoveCardsToLoadPile(reactor.GetComponent<ReactorScript>().cardList);
        }

        MoveCardsToLoadPile(Config.config.deck.GetComponent<DeckScript>().cardList);
        MoveCardsToLoadPile(Config.config.wastePile.GetComponent<WastepileScript>().cardList);
        MoveCardsToLoadPile(Config.config.matches.GetComponent<MatchedPileScript>().cardList);

        StateLoader.saveSystem.LoadTutorialState("GameStates/" + fileName);
        StateLoader.saveSystem.UnpackState(state: StateLoader.saveSystem.gameState, isTutorial: true);
    }

    private static void MoveCardsToLoadPile(List<GameObject> cards)
    {
        int cardCount = cards.Count;
        while (cardCount != 0)
        {
            cards[0].GetComponent<CardScript>().MoveCard(Config.config.loadPile, doLog: false, isAction: false);
            cardCount--;
        }
    }

    private void ShowText(List<string> command)
    {
        if (command.Count != 2)
        {
            throw new FormatException("does not contain exactly 2 entries");
        }

        Debug.Log($"showing text: {command[1]}");

        tutorialText.GetComponent<Text>().text = command[1];

        /*
        Debug.Log("showing text: \"" + text + "\" at " + region);

        tutorialText.GetComponent<Text>().text = text;
        if (region == "middle")
        {
            tutorialText.transform.GetComponent<RectTransform>().anchoredPosition.Set(0, 800);
        }
        else if (region == "top")
        {
            tutorialText.transform.GetComponent<RectTransform>().anchoredPosition.Set(0, 1300);
        }
        else if (region == "bottom")
        {
            tutorialText.transform.GetComponent<RectTransform>().anchoredPosition.Set(0, 45);
        }
        */
    }

    private void EndTutorial()
    {
        Debug.Log("ending tutorial");

        tutorial.SetActive(false);

        if (Config.config != null)
        {
            Config.config.gamePaused = false;
            Config.config.gameOver = false;
            Config.config.gameWin = false;
        }

        SceneManager.LoadScene("MainMenuScene");
        Config.config.GetComponent<MusicController>().MainMenuMusic();
    }

    private void HighlightObject(List<string> command)
    {
        // command format: 
        // 0:HighlightObject, 1:Object to Highlight, 2:Highlight On/Off
        // 0:HighlightObject, 1:MANY,                2:On/Off

        Debug.Log("highlighting object");

        // detect if the command is the right length
        if (command.Count != 3)
        {
            throw new FormatException("does not contain only 3 entries");
        }

        // detect if the 2nd command is valid
        bool highlightOn = command[2].Equals("On");
        if (!highlightOn && command[2] != "Off")
        {
            throw new FormatException("does not properly specify either \"On\" nor \"Off\" for the 2nd command");
        }

        if (command[1] == "Reactors")
        {
            tutorialReactors.SetActive(highlightOn);
        }
        else if (command[1] == "Score")
        {
            tutorialScore.SetActive(highlightOn);
        }
        else if (command[1] == "MoveCounter")
        {
            tutorialMoveCounter.SetActive(highlightOn);
        }
        else if (command[1] == "Undo")
        {
            tutorialUndo.SetActive(highlightOn);
        }
        else if (command[1] == "Pause")
        {
            tutorialPause.SetActive(highlightOn);
        }
        else if (command[1] == "Foundations")
        {
            tutorialFoundations.SetActive(highlightOn);
        }
        else if (command[1] == "Deck")
        {
            tutorialDeck.SetActive(highlightOn);
        }
        else if (command[1] == "WastePile")
        {
            tutorialWastePile.SetActive(highlightOn);
        }
        else
        {
            throw new FormatException("does not contain a valid object to highlight in the 1st command");
        }
    }

    private void HighlightContainer(List<string> command)
    {
        // command format: 
        // 0:HighlightContainer, 1:Container(s) to Highlight,           2:Highlight On/Off, 3:Alert Level
        // 0:HighlightContainer, 1:Reactors-Foundations-Deck-WastePile, 2:On/Off,           3:1-2

        Debug.Log("highlighting container");

        // detect if the command is the right length
        if (command.Count != 4)
        {
            throw new FormatException("does not contain only 4 entries");
        }

        // detect if the 2nd command is valid
        bool highlightOn = command[2].Equals("On");
        if (!highlightOn && command[2] != "Off")
        {
            throw new FormatException("does not properly specify either \"On\" nor \"Off\" for the 2nd command");
        }

        // detect if the containers to highlight are the reactors
        // the reactors have 2 ways they can glow
        if (command[1] == "Reactors")
        {
            if (highlightOn)
            {
                byte alertLevel;
                try
                {
                    alertLevel = byte.Parse(command[3]);
                }
                catch (FormatException)
                {
                    throw new FormatException("does not contain a byte in the 3rd command");
                }
                if (alertLevel != 1 || alertLevel != 2)
                {
                    throw new FormatException("does not properly specify either \"1\" nor \"2\" for the 3rd command");
                }

                foreach (GameObject reactor in Config.config.reactors)
                {
                    reactor.GetComponent<ReactorScript>().GlowOn(alertLevel);
                }
            }
            else
            {
                foreach (GameObject reactor in Config.config.reactors)
                {
                    reactor.GetComponent<ReactorScript>().GlowOff();
                }
            }
        }

        // detect if the containers to highlight are the foundations
        else if (command[1] == "Foundations")
        {
            if (highlightOn)
            {
                foreach (GameObject foundation in Config.config.foundations)
                {
                    foundation.GetComponent<FoundationScript>().GlowOn();
                }
            }
            else
            {
                foreach (GameObject foundation in Config.config.foundations)
                {
                    foundation.GetComponent<FoundationScript>().GlowOff();
                }
            }
        }

        // detect if the container to highlight is the deck
        else if (command[1] == "Deck")
        {
            if (highlightOn)
            {
                Config.config.deck.GetComponent<Image>().color = Color.yellow;
            }
            else
            {
                Config.config.deck.GetComponent<Image>().color = Color.white;
            }
        }

        // detect if the container to highlight is the waste pile
        else if (command[1] == "WastePile")
        {

        }

        // the container to highlight is invalid
        else
        {
            throw new FormatException("does not contain a valid container to highlight in the 1st command");
        }
    }

    private void HighlightToken(List<string> command)
    {
        // command format: 
        // 0:Highlight Token, 1:Object(s) Containing Token,   2:Object Index, 3:Token Index, 4:Highlight On/Off, 5:Is Match
        // 0:HighlightToken,  1:Reactor-Foundation-Wastepile, 2:0-1-2-3,      3:0-Count,     4:On-Off,           5:True-False

        Debug.Log("highlighting token");

        // detect if the command is the right length
        if (command.Count != 6)
        {
            throw new FormatException("does not contain only 6 entries");
        }

        // detect if the 4th command is valid
        bool highlightOn = command[4].Equals("On");
        if (!highlightOn && command[4] != "Off")
        {
            throw new FormatException("does not properly specify either \"On\" nor \"Off\" for the 4th command");
        }

        // detect if the 2nd command is valid and parse it
        int objectIndex;
        try
        {
            objectIndex = Int32.Parse(command[2]);
        }
        catch (FormatException)
        {
            throw new FormatException("does not contain a int in the 2nd command");
        }
        if (objectIndex < 0)
        {
            throw new FormatException("contains a negative int in the 2nd command");
        }

        // detect if the 3rd command is valid and parse it
        int tokenIndex;
        try
        {
            tokenIndex = Int32.Parse(command[3]);
        }
        catch (FormatException)
        {
            throw new FormatException("does not contain a int in the 3rd command");
        }
        if (tokenIndex < 0)
        {
            throw new FormatException("does a negative int in the 3rd command");
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
                throw new FormatException("does not contain a bool in the 5th command");
            }
        }

        // detect if the desired token is in a reactor
        // there are only 4 reactors and only so many tokens in them
        if (command[1] == "Reactor")
        {
            if (objectIndex < 0 || objectIndex > 3)
            {
                throw new FormatException("contains an invalid reactor index for the 2nd command");
            }

            List<GameObject> reactorCardList = Config.config.reactors[objectIndex].GetComponent<ReactorScript>().cardList;
            if (reactorCardList.Count < tokenIndex)
            {
                throw new FormatException($"contains an out of bounds token index for the 3nd command. there are only {reactorCardList.Count} token(s) to choose from in reactor {objectIndex}");
            }

            if (highlightOn)
            {
                reactorCardList[tokenIndex].GetComponent<CardScript>().GlowOn(match);
            }
            else
            {
                reactorCardList[tokenIndex].GetComponent<CardScript>().GlowOff();
            }
        }

        // detect if the desired token is in a foundation
        // there are only 4 foundations and only so many tokens in them
        else if (command[1] == "Foundation")
        {
            if (objectIndex < 0 || objectIndex > 3)
            {
                throw new FormatException("contains an invalid foundation index for the 2nd command");
            }

            List<GameObject> foundationCardList = Config.config.foundations[objectIndex].GetComponent<FoundationScript>().cardList;
            if (foundationCardList.Count < tokenIndex)
            {
                throw new FormatException($"contains an out of bounds token index for the 3nd command. there are only {foundationCardList.Count} token(s) to choose from in foundation {objectIndex}");
            }

            if (highlightOn)
            {
                foundationCardList[tokenIndex].GetComponent<CardScript>().GlowOn(match);
            }
            else
            {
                foundationCardList[tokenIndex].GetComponent<CardScript>().GlowOff();
            }
        }

        // detect if the desired token is in the waste pile
        // there is only 1 waste pile and only so many tokens in it
        else if (command[1] == "WastePile")
        {
            if (objectIndex != 0)
            {
                throw new FormatException("contains an invalid waste pile index for the 2nd command, it must be 0");
            }

            List<GameObject> wastePileCardList = Config.config.wastePile.GetComponent<WastepileScript>().cardList;
            if (wastePileCardList.Count < tokenIndex)
            {
                throw new FormatException($"contains an out of bounds token index for the 3nd command. there are only {wastePileCardList.Count} token(s) to choose from in the waste pile");
            }

            if (highlightOn)
            {
                wastePileCardList[tokenIndex].GetComponent<CardScript>().GlowOn(match);
            }
            else
            {
                wastePileCardList[tokenIndex].GetComponent<CardScript>().GlowOff();
            }
        }

        // the object that contains the desired token is invalid
        else
        {
            throw new FormatException("contains an invalid object that contains the token for the 1st command");
        }
    }

    private void HighlightAllInteractableTokens()
    {
        Debug.Log("highlighting all interactable tokens");

        List<GameObject> cardListRef;

        foreach (GameObject reactor in Config.config.reactors)
        {
            cardListRef = reactor.GetComponent<ReactorScript>().cardList;
            if (cardListRef.Count != 0)
            {
                cardListRef[0].GetComponent<CardScript>().GlowOn(false);
            }
        }

        foreach (GameObject foundation in Config.config.foundations)
        {
            cardListRef = foundation.GetComponent<FoundationScript>().cardList;
            if (cardListRef.Count != 0)
            {
                CardScript cardScriptRef;
                foreach(GameObject card in cardListRef)
                {
                    cardScriptRef = card.GetComponent<CardScript>();
                    if (cardScriptRef.IsHidden)
                    {
                        break;
                    }

                    cardScriptRef.GlowOn(false);
                }
            }
        }

        cardListRef = Config.config.wastePile.GetComponent<WastepileScript>().cardList;
        if (cardListRef.Count != 0)
        {
            cardListRef[0].GetComponent<CardScript>().GlowOn(false);
        }
    }

    private void UnHighlightAllTokens()
    {
        Debug.Log("unhighlighting all tokens");

        foreach (GameObject reactor in Config.config.reactors)
        {
            CardListGlowOff(reactor.GetComponent<ReactorScript>().cardList);
        }

        foreach (GameObject foundation in Config.config.foundations)
        {
            CardListGlowOff(foundation.GetComponent<FoundationScript>().cardList);
        }

        CardListGlowOff(Config.config.wastePile.GetComponent<WastepileScript>().cardList);
    }

    private void CardListGlowOff(List<GameObject> cardList)
    {
        foreach (GameObject card in cardList)
        {
            card.GetComponent<CardScript>().GlowOff();
        }
    }

    private static List<ArgumentListWrapper> CreateFromJSON()
    {
        Debug.Log("creating list from JSON");

        var jsonTextFile = Resources.Load<TextAsset>("Tutorial/TutorialCommandList");
        TutorialCommands commandFile = JsonUtility.FromJson<TutorialCommands>(jsonTextFile.ToString());
        return commandFile.commands;
    }
}
