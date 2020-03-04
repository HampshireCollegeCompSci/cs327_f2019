﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialScript : MonoBehaviour
{
    private List<ArgumentListWrapper> commandList;
    private Queue<List<string>> commandQueue;
    private bool executeFlag = true;

    public GameObject tutorialText;
    public GameObject tutorialMask;

    private void Awake()
    {
        if (!Config.config.tutorialOn)
        {
            gameObject.SetActive(false);
        }
        else
        {
            tutorialText.SetActive(true);
        }
    }

    void Start()
    {
        commandList = CreateFromJSON();
        foreach (ArgumentListWrapper command in commandList)
        {
            print(command.argumentList);
        }
        CommandReader(commandList);
    }

    void CommandReader(List<ArgumentListWrapper> commandList)
    {
        foreach (ArgumentListWrapper command in commandList)
        {
            commandQueue.Enqueue(command.argumentList);
        }
    }

    private void Update()
    {

        if (!Config.config.tutorialOn)
        {
            tutorialText.SetActive(false);
        }
        if (Input.touchCount > 0 && !executeFlag)
        {
            executeFlag = true;
        }
        else
        {
            CommandInterpreter();
        }
    }

    void CommandInterpreter()
    {
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
        }
    }

    //Tutorial Commands
    public void WaitForTouch()
    {
        if (executeFlag)
        {
            executeFlag = false;
        }
    }

    public void LoadSave(string fileName)
    {
        //move all tokens back to load pile, then call LoadState and UnpackState
        foreach (GameObject foundation in Config.config.foundations)
        {
            foreach (GameObject card in foundation.GetComponent<FoundationScript>().cardList)
            {
                card.GetComponent<CardScript>().MoveCard(Config.config.loadPile, false, false);
            }
        }

        foreach (GameObject reactor in Config.config.reactors)
        {
            foreach (GameObject card in reactor.GetComponent<FoundationScript>().cardList)
            {
                card.GetComponent<CardScript>().MoveCard(Config.config.loadPile, false, false);
            }
        }

        foreach (GameObject card in Config.config.deck.GetComponent<DeckScript>().cardList)
        {
            card.GetComponent<CardScript>().MoveCard(Config.config.loadPile, false, false);
        }

        foreach (GameObject card in Config.config.wastePile.GetComponent<WastepileScript>().cardList)
        {
            card.GetComponent<CardScript>().MoveCard(Config.config.loadPile, false, false);
        }

        StateLoader.saveSystem.loadTutorialState("GameStates/" + fileName);
        StateLoader.saveSystem.unpackState(state: StateLoader.saveSystem.gameState, isTutorial: true);

    }
    public void ShowMask(string fileName)
    {
        tutorialMask.GetComponent<SpriteRenderer>().sprite = Resources.Load(fileName) as Sprite;
        tutorialMask.transform.localScale.Set(1, 1, 1);
    }

    public void ShowText(string text, string region)
    {
        tutorialText.GetComponent<Text>().text = text;
        if (region == "middle")
        {
            tutorialText.transform.position.Set(0, 0, 0);
        }
        else if (region == "top")
        {
            tutorialText.transform.position.Set(0, 593, 0);
        }
        else if (region == "bottom")
        {
            tutorialText.transform.position.Set(0, -570, 0);
        }
    }

    private static List<ArgumentListWrapper> CreateFromJSON()
    {
        var jsonTextFile = Resources.Load<TextAsset>("Tutorial/TutorialCommandList");
        TutorialCommands commandFile = JsonUtility.FromJson<TutorialCommands>(jsonTextFile.ToString());
        return commandFile.commands;
    }
}
