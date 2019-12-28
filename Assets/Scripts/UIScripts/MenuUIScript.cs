using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class MenuUIScript : MonoBehaviour
{
    private IEnumerator coroutine;

    private void Start()
    {
        //stop animation from start
        Animator[] animators = gameObject.GetComponentsInChildren<Animator>();
        foreach (Animator anim in animators)
            anim.enabled = false;


        if (GameObject.Find("Spacebaby Loading") != null)
            GameObject.Find("Spacebaby Loading").GetComponent<Animator>().enabled = true;

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
        if (GameObject.Find("Continue") != null)
        {
            GameObject button = GameObject.Find("Continue");
            button.GetComponentInChildren<Text>().text = Config.config.levelSceneButtonsTxtEnglish[0].ToUpper();
        }
        if (GameObject.Find("Easy") != null)
        {
            GameObject button = GameObject.Find("Easy");
            button.GetComponentInChildren<Text>().text = Config.config.levelSceneButtonsTxtEnglish[1].ToUpper();
        }
        if (GameObject.Find("Normal") != null)
        {
            GameObject button = GameObject.Find("Normal");
            button.GetComponentInChildren<Text>().text = Config.config.levelSceneButtonsTxtEnglish[2].ToUpper();
        }
        if (GameObject.Find("Hard") != null)
        {
            GameObject button = GameObject.Find("Hard");
            button.GetComponentInChildren<Text>().text = Config.config.levelSceneButtonsTxtEnglish[3].ToUpper();
        }
        // update return txt
        if (GameObject.Find("Return") != null)
        {
            GameObject button = GameObject.Find("Return");
            button.GetComponentInChildren<Text>().text = Config.config.levelSceneButtonsTxtEnglish[4].ToUpper();
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
        Config.config.gameOver = false;
        Config.config.gameWin = false;
        Config.config.gamePaused = false;
        Config.config.GetComponent<SoundController>().ButtonPressSound();

        SceneManager.LoadScene("LoadingScene");

    }

    public void UndoButton()
    {
        UndoScript.undoScript.undo();
        Config.config.GetComponent<SoundController>().UndoPressSound();

        Animator undoAnim = GameObject.Find("Undo").GetComponentInChildren<Animator>();
        if (!undoAnim.enabled)
            undoAnim.enabled = true;
        else
            undoAnim.Play("");

    }

    public void PlayAgain()
    {
        Config.config.GetComponent<MusicController>().GameMusic(startNew: true);
        Config.config.gameOver = false;
        Config.config.gameWin = false;
        Config.config.gamePaused = false;

        Config.config.GetComponent<SoundController>().ButtonPressSound();
        SceneManager.LoadScene("GameplayScene");
    }

    public void Restart()
    {
        Config.config.GetComponent<MusicController>().GameMusic(startNew: true);
        Config.config.gameOver = false;
        Config.config.gameWin = false;
        Config.config.gamePaused = false;

        Config.config.GetComponent<SoundController>().ButtonPressSound();
        StopAllCoroutines();
        SceneManager.LoadScene("GameplayScene");
    }

    public void MainMenu()
    {
        Config.config.gamePaused = false;
        if (Config.config != null)
        {
            Config.config.gameOver = false;
            Config.config.gameWin = false;
        }

        Config.config.GetComponent<SoundController>().ButtonPressSound();
        SceneManager.LoadScene("MainMenuScene");

        Config.config.GetComponent<MusicController>().MainMenuMusic();
    }


    //possibly be renamed to settings
    public void Settings()
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
        GameObject utils = GameObject.Find("Utils");
        if (utils != null && utils.GetComponent<UtilsScript>().IsInputStopped())
        {
            return;
        }

        Config.config.GetComponent<SoundController>().PauseMenuButtonSound();
        //TODO save the game scene
        Config.config.gamePaused = true;
        SceneManager.LoadScene("PauseScene", LoadSceneMode.Additive);

    }

    public void ResumeGame()
    {
        Config.config.gamePaused = false;
        //TODO load the saved game scene then uncomment the above code
        Config.config.GetComponent<SoundController>().ButtonPressSound();
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
        if (File.Exists("Assets/Resources/GameStates/tutorialState.json"))
        {
            Debug.Log("tutorial <3");
            StateLoader.saveSystem.tutorialState();
            Config.config.setDifficulty(StateLoader.saveSystem.gameState.difficulty);
            NewGame();
        }
    }

    public void HardDifficulty()
    {
        Config.config.DeleteSave();
        Config.config.setDifficulty("hard");
        NewGame();
    }

    public void MediumDifficulty()
    {
        Config.config.DeleteSave();
        Config.config.setDifficulty("medium");
        NewGame();
    }

    public void EasyDifficulty()
    {
        Config.config.DeleteSave();
        Config.config.setDifficulty("easy");
        NewGame();
    }

    public void Continue()
    {
        if (File.Exists("Assets/Resources/GameStates/testState.json"))
        {
            StateLoader.saveSystem.loadState();
            Config.config.setDifficulty(StateLoader.saveSystem.gameState.difficulty);
            NewGame();
        }
    }

    public void MakeActionsMax()
    {
        Config.config.deck.GetComponent<DeckScript>().StartNextCycle(manuallyTriggered: true);
    }

    //IEnumerator ButtonPressedAnim(GameObject button, string scene, bool additive = false, bool unload = false)
    //{
    //    Config.config.GetComponent<SoundController>().ButtonPressSound();
    //    button.GetComponentInChildren<Animator>().enabled = true;

    //    for (float ft = 0.2f; ft >= 0; ft -= 0.1f)
    //    {
    //        yield return new WaitForSeconds(0.08f);
    //    }

    //    button.GetComponentInChildren<Animator>().enabled = false;

    //    if (!additive && !unload)
    //        SceneManager.LoadScene(scene);
    //    else if (additive)
    //        SceneManager.LoadScene(scene, LoadSceneMode.Additive);
    //    else if (unload)
    //        SceneManager.UnloadSceneAsync(scene);
    //}
}
