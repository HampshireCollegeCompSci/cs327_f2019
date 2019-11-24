using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIScript : MonoBehaviour
{

    private void Start()
    {
        //stop animation from start
        //Animator[] animators = gameObject.GetComponentsInChildren<Animator>();
        //foreach (Animator anim in animators)
        //    anim.enabled = false;

        //update main menu button txt
        if (GameObject.Find("Play") != null)
        {
            GameObject button = GameObject.Find("Play");
            button.GetComponentInChildren<Text>().text = Config.config.menuSceneButtonsTxtEnglish[0].ToUpper();
        }
        if (GameObject.Find("Tutorial") != null)
        {
            GameObject button = GameObject.Find("Tutorial");
            button.GetComponentInChildren<Text>().text = Config.config.menuSceneButtonsTxtEnglish[1].ToUpper();
        }
        if (GameObject.Find("Credits") != null)
        {
            GameObject button = GameObject.Find("Credits");
            button.GetComponentInChildren<Text>().text = Config.config.menuSceneButtonsTxtEnglish[2].ToUpper();
        }
        if (GameObject.Find("Loading") != null)
        {
            GameObject txt = GameObject.Find("Loading");
            txt.GetComponent<Text>().text = Config.config.loadingSceneTxtEnglish.ToUpper();
        }

        //update level txt
        if (GameObject.Find("Easy") != null)
        {
            GameObject button = GameObject.Find("Easy");
            button.GetComponentInChildren<Text>().text = Config.config.levelSceneButtonsTxtEnglish[0].ToUpper();
        }
        if (GameObject.Find("Normal") != null)
        {
            GameObject button = GameObject.Find("Normal");
            button.GetComponentInChildren<Text>().text = Config.config.levelSceneButtonsTxtEnglish[1].ToUpper();
        }
        if (GameObject.Find("Hard") != null)
        {
            GameObject button = GameObject.Find("Hard");
            button.GetComponentInChildren<Text>().text = Config.config.levelSceneButtonsTxtEnglish[2].ToUpper();
        }
        // update return txt
        if (GameObject.Find("Return") != null)
        {
            GameObject button = GameObject.Find("Return");
            button.GetComponentInChildren<Text>().text = Config.config.levelSceneButtonsTxtEnglish[3].ToUpper();
        }
        //update pause menu txt
        if (GameObject.Find("Resume") != null)
        {
            GameObject button = GameObject.Find("Resume");
            button.GetComponentInChildren<Text>().text = Config.config.pauseSceneButtonsTxtEnglish[0].ToUpper();
        }
        if (GameObject.Find("Restart") != null)
        {
            GameObject button = GameObject.Find("Restart");
            button.GetComponentInChildren<Text>().text = Config.config.pauseSceneButtonsTxtEnglish[1].ToUpper();
        }
        if (GameObject.Find("Settings") != null)
        {
            GameObject button = GameObject.Find("Settings");
            button.GetComponentInChildren<Text>().text = Config.config.pauseSceneButtonsTxtEnglish[2].ToUpper();
        }
        //update summary txt
        if (GameObject.Find("MainMenu") != null)
        {
            GameObject button = GameObject.Find("MainMenu");
            button.GetComponentInChildren<Text>().text = Config.config.summarySceneButtonsTxtEnglish[0].ToUpper();
        }
        if (GameObject.Find("PlayAgain") != null)
        {
            GameObject button = GameObject.Find("PlayAgain");
            button.GetComponentInChildren<Text>().text = Config.config.summarySceneButtonsTxtEnglish[1].ToUpper();
        }

    }

    public void Play()
    {
        Config.config.GetComponent<SoundController>().ButtonPressSound();
        SceneManager.LoadScene("LevelSelectScene");
    }

    public void NewGame()
    {
        Config.config.GetComponent<SoundController>().ButtonPressSound();
        Config.config.gameOver = false;
        Config.config.gameWin = false;
        Config.config.gamePaused = false;
        gameObject.SetActive(false);

        //SceneManager.LoadScene("GameplayScene");
        //Config.config.GetComponent<MusicController>().GameMusic();

        //loading scene
        SceneManager.LoadScene("LoadingScene", LoadSceneMode.Additive);

    }

    public void UndoButton()
    {
        UndoScript.undoScript.undo();
        //GameObject.Find("Undo").GetComponentInChildren<Animator>().enabled = true;
        //GameObject.Find("Undo").GetComponentInChildren<Animator>().Play("MenuButtonPressAnim");
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
    public void HardDifficulty()
    {
        Config.config.setDifficulty("hard");
        NewGame();
    }
    public void EasyDifficulty()
    {
        Config.config.setDifficulty("easy");
        NewGame();
    }
    public void MediumDifficulty()
    {
        Config.config.setDifficulty("medium");
        NewGame();
    }

    public void MakeActionsMax()
    {
        Config.config.actions = Config.config.actionMax;
    }

}
