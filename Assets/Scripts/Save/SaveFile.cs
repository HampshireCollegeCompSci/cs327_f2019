using System.IO;
using UnityEngine;

public static class SaveFile
{
    private static string _saveFilePath;
    public static string SaveFilePath => _saveFilePath;

    public static void SetPath()
    {
#if (UNITY_EDITOR)
        _saveFilePath = Constants.GameStates.saveStateFilePathJsonInEditor;
#else
        _saveFilePath = $"{Application.persistentDataPath}/{Constants.GameStates.saveStateFileNameJson}";
#endif
    }

    public static bool Exists()
    {
        return File.Exists(SaveFilePath);
    }

    public static void CheckNewGameStateVersion()
    {
        if(PersistentSettings.NewGameStateVersion())
        {
            Debug.Log("a new game state version was detected");
            if (Exists())
            {
                Delete();
            }
        }
    }

    public static void Delete()
    {
        Debug.Log("deleting save state");
        #if (UNITY_EDITOR)
            File.Delete(Constants.GameStates.saveStateFilePathJsonInEditor);
            File.Delete(Constants.GameStates.saveStateFilePathMetaInEditor);
        #else
            File.Delete(SaveFilePath);
        #endif
    }
}
