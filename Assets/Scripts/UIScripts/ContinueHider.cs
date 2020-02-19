using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ContinueHider : MonoBehaviour
{
    public GameObject continueButton;
    private void Awake()
    {
        if (!Application.isEditor && !File.Exists(Application.persistentDataPath + "/testState.json"))
        {
            continueButton.SetActive(false);
        }
        if (Application.isEditor && !File.Exists("Assets/Resources/GameStates/testState.json"))
        {
            continueButton.SetActive(false);
        }
    }
}
