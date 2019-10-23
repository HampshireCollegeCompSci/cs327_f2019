using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUIScript : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("LevelSelectScene");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void NewGame(int difficulty)
    {
        //restarts the level there will be more added to this method later but for now we don't have dificulty
        SceneManager.LoadScene("FoundationTestScene");
        Config.config.gameOver = false;
        Config.config.gameWin = false;
        gameObject.SetActive(false);
    }

    public void Restart()
    {
        SceneManager.LoadScene("FoundationTestScene");//resets the level
        Config.config.gamePaused = false;
        Config.config.gameOver = false;
        Config.config.gameWin = false;
    }

    public void MainMenu()
    {
        if (Config.config != null)
        {
            SceneManager.LoadScene("MainMenuScene");
            Config.config.gamePaused = false;
            Config.config.gameOver = false;
            Config.config.gameWin = false;
        }
        else
        {
            SceneManager.LoadScene("MainMenuScene");
        }
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
        Config.config.gamePaused = true;
        SceneManager.LoadScene("PauseScene");
    }

    public void ResumeGame()
    {
        //Config.config.gamePaused = false;
        //TODO load the saved game scene then uncomment the above code
    }

}
