using System.IO;
using UnityEngine;

public static class SaveFile
{
    public static string GetPath()
    {
        if (Constants.inEditor)
        {
            return Constants.saveStatePathInEditor + Constants.saveStateFileNameJson;
        }
        else
        {
            return Application.persistentDataPath + Constants.saveStateFileNameJson;
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
        if (Constants.inEditor)
        {
            File.Delete(Constants.saveStateFilePathJsonInEditor);
            File.Delete(Constants.saveStateFilePathMetaInEditor);
        }
        else
        {
            File.Delete(Application.persistentDataPath + Constants.saveStateFileNameJson);
            File.Delete(Application.persistentDataPath + Constants.saveStateFileNameMeta);
        }
    }
}
