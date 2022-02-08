using UnityEngine;

public static class Constants
{
    // Save States
    public const string saveStateLocationEditor = "Assets/Resources/GameStates";
    public const string saveStateFileName = "/saveState";

    public const string saveStateFileNameJson = saveStateFileName + ".json";
    public const string saveStateFilePathJsonEditor = saveStateLocationEditor + saveStateFileNameJson;
    public const string saveStateFileNameMeta = saveStateFileNameJson + ".meta";
    public const string saveStateFilePathMetaEditor = saveStateLocationEditor + saveStateFileNameMeta;

    // Tutorial
    public const string tutorialStateLocation = "Assets/Resources/Tutorial";
    public const string tutorialStateStartFileName = "tutorialState_start";

    // Other
    public static bool inEditor = Application.isEditor;
}
