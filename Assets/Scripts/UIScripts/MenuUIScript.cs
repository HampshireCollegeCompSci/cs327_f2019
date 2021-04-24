using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class MenuUIScript : MonoBehaviour
{
    // MainMenuScene buttons
    public GameObject playButton, tutorialButton, creditsButton;

    // LevelSelectScene buttons
    public GameObject continueButton, easyButton, normalButton, hardButton, returnButton;

    // LoadingScene text
    public GameObject loadingText;

    // PauseScene buttons
    public GameObject resumeButton, restartButton;

    // PauseScene and SummaryScene button
    public GameObject mainMenuButton;

    // SummaryScene buttons
    public GameObject playAgainButton;


    private void Start()
    {
        Debug.Log("starting MenuUIScript");

        string activeSceneName = SceneManager.GetActiveScene().name;

        if (activeSceneName == "MainMenuScene")
        {
            Debug.Log("updating main menu buttons");
            UpdateButtonText(playButton, Config.config.menuSceneButtonsTxtEnglish[0]);
            UpdateButtonText(tutorialButton, Config.config.menuSceneButtonsTxtEnglish[1]);
            UpdateButtonText(creditsButton, Config.config.menuSceneButtonsTxtEnglish[2]);
        }
        else if (activeSceneName == "LevelSelectScene")
        {
            Debug.Log("updating level select buttons");
            UpdateButtonText(continueButton, Config.config.levelSceneButtonsTxtEnglish[0]);
            UpdateButtonText(easyButton, Config.config.levelSceneButtonsTxtEnglish[1]);
            UpdateButtonText(normalButton, Config.config.levelSceneButtonsTxtEnglish[2]);
            UpdateButtonText(hardButton, Config.config.levelSceneButtonsTxtEnglish[3]);
            UpdateButtonText(returnButton, Config.config.levelSceneButtonsTxtEnglish[4]);
        }
        else if (activeSceneName == "LoadingScene")
        {
            if (loadingText != null)
            {
                Debug.Log("updating loading text");
                loadingText.GetComponent<Text>().text = Config.config.loadingSceneTxtEnglish;
            }
        }
        else if (SceneManager.GetSceneByName("PauseScene").isLoaded)
        {
            Debug.Log("updating pause scene buttons");
            UpdateButtonText(resumeButton, Config.config.pauseSceneButtonsTxtEnglish[0]);
            UpdateButtonText(restartButton, Config.config.pauseSceneButtonsTxtEnglish[1]);
            //UpdateButtonText(settingsButton, Config.config.pauseSceneButtonsTxtEnglish[2]);
            UpdateButtonText(mainMenuButton, Config.config.pauseSceneButtonsTxtEnglish[3]);
        }
        else if (activeSceneName == "SummaryScene")
        {
            Debug.Log("updating summary scene buttons");
            UpdateButtonText(mainMenuButton, Config.config.summarySceneButtonsTxtEnglish[0]);
            UpdateButtonText(playAgainButton, Config.config.summarySceneButtonsTxtEnglish[1]);
        }

    }

    private void UpdateButtonText(GameObject button, string text)
    {
        button.transform.GetChild(2).GetComponent<Text>().text = text;
    }

    public void Play()
    {
        Debug.Log("MenuUI play");

        //Preprocessor Directive to make builds work
        #if (UNITY_EDITOR)
            UnityEditor.AssetDatabase.Refresh();
        #endif

        Config.config.GetComponent<SoundController>().ButtonPressSound();
        SceneManager.LoadScene("LevelSelectScene");
    }

    public void NewGame(bool isContinue = false)
    {
        Debug.Log("MenuUI new game");

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
        Debug.Log("MenuUI undo button");

        if (UtilsScript.global.IsInputStopped())
            return;

        UndoScript.undoScript.undo();
        Config.config.GetComponent<SoundController>().UndoPressSound();
    }

    public void PlayAgain()
    {
        Debug.Log("MenuUI play again");

        Config.config.DeleteSave();
        NewGame();
    }

    public void Restart()
    {
        Debug.Log("MenuUI restart");

        Config.config.DeleteSave();
        NewGame();
    }

    public void MainMenu()
    {
        Debug.Log("MenuUI main menu");

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
        Debug.Log("MenuUI settings");

        Config.config.GetComponent<SoundController>().ButtonPressSound();
        SceneManager.LoadScene("SoundScene");
    }

    public void Credits()
    {
        Debug.Log("MenuUI credits");

        Config.config.GetComponent<SoundController>().ButtonPressSound();
        SceneManager.LoadScene("CreditScene");
    }

    public void PauseGame()
    {
        Debug.Log("MenuUI pause game");

        if (UtilsScript.global.IsInputStopped())
            return;

        Config.config.GetComponent<SoundController>().PauseMenuButtonSound();
        //TODO save the game scene
        Config.config.gamePaused = true;
        SceneManager.LoadScene("PauseScene", LoadSceneMode.Additive);

    }

    public void ResumeGame()
    {
        Debug.Log("MenuUI resume game");

        Config.config.gamePaused = false;
        //TODO load the saved game scene then uncomment the above code
        Config.config.GetComponent<SoundController>().ButtonPressSound();
        SceneManager.UnloadSceneAsync("PauseScene");
    }

    public void Return()
    {
        Debug.Log("MenuUI return");

        Config.config.GetComponent<SoundController>().ButtonPressSound();
        if (Config.config.gamePaused)
            SceneManager.UnloadSceneAsync("SoundScene");
        else
            SceneManager.LoadScene("MainMenuScene");
    }

    public void Tutorial()
    {
        Debug.Log("MenuUI starting tutorial");

        StateLoader.saveSystem.LoadTutorialState("tutorialStart");
        Config.config.SetDifficulty(StateLoader.saveSystem.gameState.difficulty);
        Config.config.tutorialOn = true;
        //Config.config.DeleteSave();
        NewGame();
    }

    public void HardDifficulty()
    {
        Config.config.SetDifficulty("HARD");
        Config.config.tutorialOn = false;
        Config.config.DeleteSave();
        NewGame();
    }

    public void MediumDifficulty()
    {
        Config.config.SetDifficulty("MEDIUM");
        Config.config.tutorialOn = false;
        Config.config.DeleteSave();
        NewGame();
    }

    public void EasyDifficulty()
    {
        Config.config.SetDifficulty("EASY");
        Config.config.tutorialOn = false;
        Config.config.DeleteSave();
        NewGame();
    }

    public void Continue()
    {
        Debug.Log("MenuUI continue");

        if (Application.isEditor)
        {
            if (File.Exists("Assets/Resources/GameStates/testState.json"))
            {
                //Preprocessor Directive to make builds work
                #if (UNITY_EDITOR)
                    UnityEditor.AssetDatabase.Refresh();
                #endif


                StateLoader.saveSystem.LoadState();
                Config.config.SetDifficulty(StateLoader.saveSystem.gameState.difficulty);
                Config.config.tutorialOn = false;
                NewGame(true);
            }
        } 
        else
        {
            if (File.Exists(Application.persistentDataPath + "/testState.json"))
            {
                StateLoader.saveSystem.LoadState();
                Config.config.SetDifficulty(StateLoader.saveSystem.gameState.difficulty);
                Config.config.tutorialOn = false;
                NewGame(true);
            }
        }
        
    }

    public void MakeActionsMax()
    {
        Debug.Log("MenuUI make actions max");

        if (UtilsScript.global.IsInputStopped())
            return;

        Config.config.deck.GetComponent<DeckScript>().StartNextCycle(manuallyTriggered: true);
    }
}
