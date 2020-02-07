using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScript : MonoBehaviour
{
    private List<string> commandList;
    private Queue<string[]> splitCommandList;
    void Start()
    {
        commandList = CreateFromJSON();
        CommandReader(commandList);
    }

    void CommandReader(List<string> commandList)
    {
        foreach (string command in commandList)
        {
            string[] splitCommand = command.Split(_);
            splitCommandList.Enqueue(splitCommand);
        }
    }

    void CommandInterpreter()
    {
        string[] command = splitCommandList.Dequeue();
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

    }

    public void LoadSave(string fileName)
    {

    }
    public void ShowMask(string fileName)
    {

    }

    public void ShowText(string text, string region)
    {

    }

    private static List<string> CreateFromJSON()
    {
        var jsonTextFile = Resources.Load<TextAsset>("Tutorial/TutorialCommandList");
        TutorialCommands commandFile = JsonUtility.FromJson<TutorialCommands>(jsonTextFile.ToString());
        return commandFile.commands;
    }
}
