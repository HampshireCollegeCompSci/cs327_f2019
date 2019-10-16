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
        Config.config.gameOver = false;
        SceneManager.LoadScene("FoundationTestScene");//resets the level
        Config.config.gameWin = false;
    }

    public void MainMenu()
    {
        Config.config.gameOver = false;
        SceneManager.LoadScene("MainMenuScene");//resets the level
        Config.config.gameWin = false;
    }

    public void CloseGameMenu(GameObject gameMenu)
    {
        gameMenu.SetActive(false);
    }

    public void About()
    {
        SceneManager.LoadScene("AboutScene");//resets the level
    }

    public void PauseGame()
    {
        //TODO save the game scene

        SceneManager.LoadScene("PauseScene");
    }

    public void ResumeGame()
    {
        //TODO load the saved game scene
    }

}
