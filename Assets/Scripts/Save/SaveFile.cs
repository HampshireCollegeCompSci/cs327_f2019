using System.IO;
using UnityEngine;

public static class SaveFile
{
    public static string GetPath()
    {
        if (Application.isEditor)
        {
            return Constants.GameStates.saveStatePathInEditor + Constants.GameStates.saveStateFileNameJson;
        }
        else
        {
            return Application.persistentDataPath + Constants.GameStates.saveStateFileNameJson;
        }
    }

    public static bool Exists()
    {
        return File.Exists(GetPath());
    }

    public static void CheckNewGameStateVersion()
    {
        if(PlayerPrefKeys.NewGameStateVersion())
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
        if (Application.isEditor)
        {
            File.Delete(Constants.GameStates.saveStateFilePathJsonInEditor);
            File.Delete(Constants.GameStates.saveStateFilePathMetaInEditor);
        }
        else
        {
            File.Delete(Application.persistentDataPath + Constants.GameStates.saveStateFileNameJson);
            File.Delete(Application.persistentDataPath + Constants.GameStates.saveStateFileNameMeta);
        }
    }
}
