using System.IO;
using UnityEngine;

public static class SaveState
{
    public static string GetFilePath()
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
        return File.Exists(GetFilePath());
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
