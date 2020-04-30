using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class MenuUIScript : MonoBehaviour
{
    private void Start()
    {

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
        //Preprocessor Directive to make builds work
        #if (UNITY_EDITOR)
            UnityEditor.AssetDatabase.Refresh();
        #endif
        Config.config.GetComponent<SoundController>().ButtonPressSound();
        SceneManager.LoadScene("LevelSelectScene");
    }

    public void NewGame(bool isContinue = false)
    {
        UndoScript.undoScript.moveLog.Clear();
        if (!isContinue)
        {
            Config.config.score = 0;
            Config.config.actions = 0;
            Config.config.moveCounter = 0;
        }
        Config.config.gameOver = false;
        Config.config.gameWin = false;
        Config.config.gamePaused = false;
        Config.config.GetComponent<SoundController>().ButtonPressSound();

        SceneManager.LoadScene("LoadingScene");
    }

    public void UndoButton()
    {
        if (UtilsScript.global.IsInputStopped())
            return;

        UndoScript.undoScript.undo();
        Config.config.GetComponent<SoundController>().UndoPressSound();

        /*Animator undoAnim = GameObject.Find("Undo").GetComponentInChildren<Animator>();
        if (!undoAnim.enabled)
            undoAnim.enabled = true;
        else
            undoAnim.Play("");
        */
    }

    public void PlayAgain()
    {
        Config.config.DeleteSave();
        NewGame();
    }

    public void Restart()
    {
        Config.config.DeleteSave();
        NewGame();
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
        if (UtilsScript.global.IsInputStopped())
            return;

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
            SceneManager.UnloadSceneAsync("SoundScene");
        else
            SceneManager.LoadScene("MainMenuScene");
    }
    public void Tutorial()
    {
        Debug.Log("tutorial <3");
        StateLoader.saveSystem.loadTutorialState("GameStates/tutorialState");
        Config.config.setDifficulty(StateLoader.saveSystem.gameState.difficulty);
        Config.config.tutorialOn = true;
        Config.config.DeleteSave();
        NewGame();
    }
    public void HardDifficulty()
    {
        Config.config.setDifficulty("HARD");
        Config.config.tutorialOn = false;
        Config.config.DeleteSave();
        NewGame();
    }

    public void MediumDifficulty()
    {
        Config.config.setDifficulty("MEDIUM");
        Config.config.tutorialOn = false;
        Config.config.DeleteSave();
        NewGame();
    }

    public void EasyDifficulty()
    {
        Config.config.setDifficulty("EASY");
        Config.config.tutorialOn = false;
        Config.config.DeleteSave();
        NewGame();
    }

    public void Continue()
    {
        if (Application.isEditor)
        {
            if (File.Exists("Assets/Resources/GameStates/testState.json"))
            {
                //Preprocessor Directive to make builds work
                #if (UNITY_EDITOR)
                    UnityEditor.AssetDatabase.Refresh();
                #endif


                StateLoader.saveSystem.loadState();
                Config.config.setDifficulty(StateLoader.saveSystem.gameState.difficulty);
                Config.config.tutorialOn = false;
                NewGame(true);
            }
        } 
        else
        {
            if (File.Exists(Application.persistentDataPath + "/testState.json"))
            {
                StateLoader.saveSystem.loadState();
                Config.config.setDifficulty(StateLoader.saveSystem.gameState.difficulty);
                Config.config.tutorialOn = false;
                NewGame(true);
            }
        }
        
    }

    public void MakeActionsMax()
    {
        if (UtilsScript.global.IsInputStopped())
            return;

        Config.config.deck.GetComponent<DeckScript>().StartNextCycle(manuallyTriggered: true);
    }

    public void NextTutorialStep()
    {
        if (Config.config.tutorialOn)
        {
            GameObject.Find("TutorialController").GetComponent<TutorialScript>().executeFlag = true;
        }
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
