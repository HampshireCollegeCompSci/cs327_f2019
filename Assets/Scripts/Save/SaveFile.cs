using System.IO;
#if !UNITY_WEBGL
using System.Threading;
using System.Threading.Tasks;
#endif
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
            Delete();
        }
    }

    public static void Delete()
    {
        if (!Exists()) return;
        Debug.Log("deleting save state");
        #if (UNITY_EDITOR)
            File.Delete(Constants.GameStates.saveStateFilePathJsonInEditor);
            File.Delete(Constants.GameStates.saveStateFilePathMetaInEditor);
        #else
            File.Delete(SaveFilePath);
        #endif
    }

#if UNITY_WEBGL
    public static void SaveGame(string content)
    {
        Debug.Log("writing the save file");
        File.WriteAllText(SaveFilePath, content);
    }
#else
    public static Task SaveGame(string content, CancellationToken token)
    {
        Debug.Log("starting the task to write the save file");
        return File.WriteAllTextAsync(SaveFilePath, content, token);
    }
#endif

    public static string GetGameSave()
    {
        return File.ReadAllText(SaveFilePath);
    }
}
