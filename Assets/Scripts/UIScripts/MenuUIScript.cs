using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUIScript : MonoBehaviour
{

    public void NewGame(int difficulty)
    {
        //restarts the level there will be more added to this method later but for now we don't have dificulty
        SceneManager.LoadScene("FoundationTestScene");
        gameObject.SetActive(false);

    }

    public void Restart()
    {
        SceneManager.LoadScene("FoundationTestScene");//resets the level
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");//resets the level
    }

    public void CloseGameMenu(GameObject gameMenu)
    {
        gameMenu.SetActive(false);
    }
}
