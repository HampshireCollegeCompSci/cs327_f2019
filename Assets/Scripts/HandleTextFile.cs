using UnityEngine;
using UnityEditor;
using System.IO;

public class HandleTextFile
{
    [SerializeField]
    string json;
    public string WriteString(string path)
    {
        using (StreamReader stream = new StreamReader(path))
        {
            json = stream.ReadToEnd();
        }
        return json;
    }
}
