using System.IO;
using UnityEngine;

public static class SaveState
{
    public static string GetFilePath()
    {
        if (Constants.inEditor)
        {
            return Constants.saveStateFilePathJsonEditor;
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
        if (Constants.inEditor)
        {
            File.Delete(Constants.saveStateFilePathJsonEditor);
            File.Delete(Constants.saveStateFilePathMetaEditor);
        }
        else
        {
            File.Delete(Application.persistentDataPath + Constants.saveStateFileNameJson);
            File.Delete(Application.persistentDataPath + Constants.saveStateFileNameMeta);
        }
    }
}
