using System.Collections.Generic;

[System.Serializable]
public class TutorialCommands
{
    public List<Command> commands;
}

[System.Serializable]
public class Command
{
    public List<string> argumentList;
}
