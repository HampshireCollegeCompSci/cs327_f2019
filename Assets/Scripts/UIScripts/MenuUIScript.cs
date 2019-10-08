using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUIScript : MonoBehaviour
{
    public void NewGame(int difficulty)
    {
        Application.LoadLevel("FoundationTestScene");//restarts the level there will be more added to this method later but for now we don't have dificulty
        return;
    }

    public void Restart()
    {
        Application.LoadLevel(0);//resets the level
        return;
    }

    public void MainMenu()
    {
        Application.LoadLevel("MainMenuScene");//resets the level
        return;
    }

    public void CloseGameMenu(GameObject gameMenu)
    {
        gameMenu.SetActive(false);
    }
}
