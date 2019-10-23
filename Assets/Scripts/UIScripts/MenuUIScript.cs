using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUIScript : MonoBehaviour
{
    public void Play()
    {
        Config.config.GetComponent<SoundController>().ButtonPressSound();
        SceneManager.LoadScene("LevelSelectScene");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void NewGame(int difficulty)
    {
        Config.config.GetComponent<SoundController>().ButtonPressSound();
        //restarts the level there will be more added to this method later but for now we don't have dificulty
        Config.config.gameOver = false;
        Config.config.gameWin = false;
        gameObject.SetActive(false);
        SceneManager.LoadScene("FoundationTestScene");
    }

    public void Restart()
    {
        Config.config.GetComponent<SoundController>().ButtonPressSound();
        SceneManager.LoadScene("FoundationTestScene");//resets the level
        Config.config.gamePaused = false;
        Config.config.gameOver = false;
        Config.config.gameWin = false;
    }

    public void MainMenu()
    {
        Config.config.GetComponent<SoundController>().ButtonPressSound();
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

    public void About()
    {
        Config.config.GetComponent<SoundController>().ButtonPressSound();
        SceneManager.LoadScene("AboutScene");
    }

    //possibly be renamed to settings
    public void Sound()
    {
        Config.config.GetComponent<SoundController>().ButtonPressSound();
        SceneManager.LoadScene("SoundScene");
    }

    public void Credits()
    {
        Config.config.GetComponent<SoundController>().ButtonPressSound();
        SceneManager.LoadScene("CreditScene");
    }

    public void PauseGame()
    {
        Config.config.GetComponent<SoundController>().PauseMenuButtonSound();
        //TODO save the game scene
        Config.config.gamePaused = true;
        SceneManager.LoadScene("PauseScene");
    }

    public void ResumeGame()
    {
        Config.config.GetComponent<SoundController>().ButtonPressSound();
        //Config.config.gamePaused = false;
        //TODO load the saved game scene then uncomment the above code
    }

}
