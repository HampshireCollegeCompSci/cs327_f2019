using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuScript : MonoBehaviour
{
    public void NewGame(int difficulty)
    {
        Application.LoadLevel("FoundationTestScene");
        return;
    }
}
