using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScript : MonoBehaviour
{
    private List<ArgumentListWrapper> commandList;
    private Queue<List<string>> commandQueue;
    private bool executeFlag = true;
    void Start()
    {
        commandList = CreateFromJSON();
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
    }
    public void ShowMask(string fileName)
    {

    }

    public void ShowText(string text, string region)
    {

    }

    private static List<ArgumentListWrapper> CreateFromJSON()
    {
        var jsonTextFile = Resources.Load<TextAsset>("Tutorial/TutorialCommandList");
        TutorialCommands commandFile = JsonUtility.FromJson<TutorialCommands>(jsonTextFile.ToString());
        return commandFile.commands;
    }
}
