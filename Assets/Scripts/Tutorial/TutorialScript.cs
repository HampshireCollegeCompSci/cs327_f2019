using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialScript : MonoBehaviour
{
    private List<ArgumentListWrapper> commandList;
    private Queue<List<string>> commandQueue;
    public bool executeFlag;

    public GameObject tutorialText;
    public GameObject tutorialMask;
    public GameObject tutorialNext;

    private void Awake()
    {
        if (!Config.config.tutorialOn)
        {
            gameObject.SetActive(false);
        }
        else
        {
            tutorialText.SetActive(true);
            tutorialNext.SetActive(true);
            tutorialMask.SetActive(true);
        }
    }

    void Start()
    {
        Debug.Log("starting tutorialScript");

        commandQueue = new Queue<List<string>>();
        commandList = CreateFromJSON();
        CommandReader(commandList);
        executeFlag = true;
    }

    private void CommandReader(List<ArgumentListWrapper> commandList)
    {
        Debug.Log("populating command queue");

        foreach (ArgumentListWrapper command in commandList)
        {
            commandQueue.Enqueue(command.argumentList);
        }
    }

    private void Update()
    {
        if (executeFlag)
        {
            CommandInterpreter();
        }
    }

    private void CommandInterpreter()
    {
        Debug.Log("interpreting command");

        if (commandQueue.Count > 0)
        {
            List<string> command = commandQueue.Dequeue();
            if (command[0] == "LoadSave")
            {
                LoadSave(command[1]);
            }
            else if (command[0] == "ShowMask")
            {
                ShowMask(command[1]);
            }
            else if (command[0] == "ShowText")
            {
                ShowText(command[1], command[2]);
            }
            else if (command[0] == "WaitForTouch")
            {
                WaitForTouch();
            }
            else if (command[0] == "EndTutorial")
            {
                EndTutorial();
            }
        }
        else
        {
            Debug.LogWarning("commandQueue found to be empty, the queue should end with \"EndTutorial\"");
            EndTutorial();
        }
    }

    //Tutorial Commands
    private void WaitForTouch()
    {
        Debug.Log("waiting for touch");

        if (executeFlag)
        {
            executeFlag = false;
        }
    }

    private void LoadSave(string fileName)
    {
        Debug.Log("loading save: " + fileName);

        //move all tokens back to load pile, then call LoadState and UnpackState
        foreach (GameObject foundation in Config.config.foundations)
        {
            List<GameObject> cardList = foundation.GetComponent<FoundationScript>().cardList;
            int listLength = cardList.Count;
            for (int i = 0; i < listLength; i++)
            {
                cardList[0].GetComponent<CardScript>().MoveCard(Config.config.loadPile, false, false);
            }
        }

        foreach (GameObject reactor in Config.config.reactors)
        {
            List<GameObject> cardList = reactor.GetComponent<ReactorScript>().cardList;
            int listLength = cardList.Count;
            for (int i = 0; i < listLength; i++)
            {
                cardList[0].GetComponent<CardScript>().MoveCard(Config.config.loadPile, false, false);
            }
        }

        List<GameObject> deckCardList = Config.config.deck.GetComponent<DeckScript>().cardList;
        int deckListLength = deckCardList.Count;
        for (int i = 0; i < deckListLength; i++)
        {
            deckCardList[0].GetComponent<CardScript>().MoveCard(Config.config.loadPile, false, false);
        }

        List<GameObject> wasteCardList = Config.config.wastePile.GetComponent<WastepileScript>().cardList;
        int wasteListLength = wasteCardList.Count;
        for (int i = 0; i < wasteListLength; i++)
        {
            wasteCardList[0].GetComponent<CardScript>().MoveCard(Config.config.loadPile, false, false);
        }

        List<GameObject> matchCardList = Config.config.matches.GetComponent<MatchedPileScript>().cardList;
        int matchListLength = matchCardList.Count;
        for (int i = 0; i < matchListLength; i++)
        {
            matchCardList[0].GetComponent<CardScript>().MoveCard(Config.config.loadPile, false, false);
        }

        StateLoader.saveSystem.LoadTutorialState("GameStates/" + fileName);
        StateLoader.saveSystem.UnpackState(state: StateLoader.saveSystem.gameState, isTutorial: true);
        UtilsScript.global.UpdateScore(0);

    }
    private void ShowMask(string fileName)
    {
        Debug.Log("Show Mask: TutorialMasks/" + fileName);

        tutorialMask.GetComponent<Image>().sprite = Resources.Load<Sprite>("TutorialMasks/" + fileName);
    }

    private void ShowText(string text, string region)
    {
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
    }

    private void EndTutorial()
    {
        Debug.Log("ending tutorial");

        if (Config.config != null)
        {
            Config.config.gamePaused = false;
            Config.config.gameOver = false;
            Config.config.gameWin = false;
        }

        SceneManager.LoadScene("MainMenuScene");
        Config.config.GetComponent<MusicController>().MainMenuMusic();
    }

    private static List<ArgumentListWrapper> CreateFromJSON()
    {
        Debug.Log("creating list from JSON");

        var jsonTextFile = Resources.Load<TextAsset>("Tutorial/TutorialCommandList");
        TutorialCommands commandFile = JsonUtility.FromJson<TutorialCommands>(jsonTextFile.ToString());
        return commandFile.commands;
    }
}
