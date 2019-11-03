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
        Config.config.GetComponent<MusicController>().GameMusic();
        //restarts the level there will be more added to this method later but for now we don't have dificulty
        Config.config.gameOver = false;
        Config.config.gameWin = false;
        Config.config.gamePaused = false;
        gameObject.SetActive(false);
        SceneManager.LoadScene("GameplayScene");
    }

    public void Restart()
    {
        Config.config.GetComponent<SoundController>().ButtonPressSound();
        Config.config.GetComponent<MusicController>().GameMusic();
        SceneManager.LoadScene("GameplayScene");//resets the level
        Config.config.gameOver = false;
        Config.config.gameWin = false;
        Config.config.gamePaused = false;
    }

    public void MainMenu()
    {
        Config.config.gamePaused = false;
        Config.config.GetComponent<SoundController>().ButtonPressSound();
        if (Config.config != null)
        {
            SceneManager.LoadScene("MainMenuScene");
            Config.config.gameOver = false;
            Config.config.gameWin = false;
        }
        else
        {
            SceneManager.LoadScene("MainMenuScene");
        }
        Config.config.GetComponent<MusicController>().MainMenuMusic();
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
        SceneManager.LoadScene("SoundScene", LoadSceneMode.Additive);
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
        SceneManager.LoadScene("PauseScene", LoadSceneMode.Additive);
    }

    public void ResumeGame()
    {
        Config.config.GetComponent<SoundController>().ButtonPressSound();
        Config.config.gamePaused = false;
        //TODO load the saved game scene then uncomment the above code
        SceneManager.UnloadSceneAsync("PauseScene");
    }

    public void Return()
    {
        Config.config.GetComponent<SoundController>().ButtonPressSound();
        if (Config.config.gamePaused)
        {
            SceneManager.UnloadSceneAsync("SoundScene");
        }

        else
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }

    public void Tutorial()
    {
        Config.config.GetComponent<SoundController>().ButtonPressSound();
        SceneManager.LoadScene("TutorialScene");
    }
    public void hardDifficulty()
    {
        Config.config.setDifficulty("hard");
    }
    public void easyDifficulty()
    {
        Config.config.setDifficulty("easy");
    }
    public void mediumDifficulty()
    {
        Config.config.setDifficulty("medium");
    }

    public void MakeActionsMax()
    {
        Config.config.actions = Config.config.actionMax;
    }
}
