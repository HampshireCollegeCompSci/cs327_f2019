using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
            UpdateButtonText(playButton, Config.Instance.menuSceneButtonsTxtEnglish[0]);
            UpdateButtonText(tutorialButton, Config.Instance.menuSceneButtonsTxtEnglish[1]);
            UpdateButtonText(creditsButton, Config.Instance.menuSceneButtonsTxtEnglish[2]);
        }
        else if (activeSceneName == "LevelSelectScene")
        {
            Debug.Log("updating level select buttons");
            UpdateButtonText(continueButton, Config.Instance.levelSceneButtonsTxtEnglish[0]);
            UpdateButtonText(easyButton, Config.Instance.levelSceneButtonsTxtEnglish[1]);
            UpdateButtonText(normalButton, Config.Instance.levelSceneButtonsTxtEnglish[2]);
            UpdateButtonText(hardButton, Config.Instance.levelSceneButtonsTxtEnglish[3]);
            UpdateButtonText(returnButton, Config.Instance.levelSceneButtonsTxtEnglish[4]);
        }
        else if (activeSceneName == "LoadingScene")
        {
            if (loadingText != null)
            {
                Debug.Log("updating loading text");
                loadingText.GetComponent<Text>().text = Config.Instance.loadingSceneTxtEnglish;
            }
        }
        else if (SceneManager.GetSceneByName("PauseScene").isLoaded)
        {
            Debug.Log("updating pause scene buttons");
            UpdateButtonText(resumeButton, Config.Instance.pauseSceneButtonsTxtEnglish[0]);
            UpdateButtonText(restartButton, Config.Instance.pauseSceneButtonsTxtEnglish[1]);
            //UpdateButtonText(settingsButton, Config.config.pauseSceneButtonsTxtEnglish[2]);
            UpdateButtonText(mainMenuButton, Config.Instance.pauseSceneButtonsTxtEnglish[3]);
        }
        else if (activeSceneName == "SummaryScene")
        {
            Debug.Log("updating summary scene buttons");
            UpdateButtonText(mainMenuButton, Config.Instance.summarySceneButtonsTxtEnglish[0]);
            UpdateButtonText(playAgainButton, Config.Instance.summarySceneButtonsTxtEnglish[1]);
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

        SoundEffectsController.Instance.ButtonPressSound();
        SceneManager.LoadScene("LevelSelectScene");
    }

    public void NewGame(bool isContinue = false)
    {
        Debug.Log("MenuUI new game");

        UndoScript.Instance.moveLog.Clear();
        if (!isContinue)
        {
            Config.Instance.score = 0;
            Config.Instance.actions = 0;
            Config.Instance.moveCounter = 0;
        }
        Config.Instance.gameOver = false;
        Config.Instance.gameWin = false;
        Config.Instance.gamePaused = false;
        SoundEffectsController.Instance.ButtonPressSound();

        SceneManager.LoadScene("LoadingScene");
    }

    public void UndoButton()
    {
        Debug.Log("MenuUI undo button");

        if (UtilsScript.Instance.IsInputStopped())
            return;

        UndoScript.Instance.Undo();
        SoundEffectsController.Instance.UndoPressSound();
    }

    public void PlayAgain()
    {
        Debug.Log("MenuUI play again");

        SaveState.Delete();
        NewGame();
    }

    public void Restart()
    {
        Debug.Log("MenuUI restart");

        SaveState.Delete();
        NewGame();
    }

    public void MainMenu()
    {
        Debug.Log("MenuUI main menu");

        Config.Instance.gamePaused = false;
        if (Config.Instance != null)
        {
            Config.Instance.gameOver = false;
            Config.Instance.gameWin = false;
        }

        SoundEffectsController.Instance.ButtonPressSound();
        SceneManager.LoadScene("MainMenuScene");

        MusicController.Instance.MainMenuMusic();
    }

    //possibly be renamed to settings
    public void Settings()
    {
        Debug.Log("MenuUI settings");

        SoundEffectsController.Instance.ButtonPressSound();
        SceneManager.LoadScene("SettingsScene");
    }

    public void Credits()
    {
        Debug.Log("MenuUI credits");

        SoundEffectsController.Instance.ButtonPressSound();
        SceneManager.LoadScene("CreditScene");

        MusicController.Instance.CreditMusic();
    }

    public void PauseGame()
    {
        Debug.Log("MenuUI pause game");

        if (UtilsScript.Instance.IsInputStopped())
            return;

        SoundEffectsController.Instance.PauseMenuButtonSound();
        //TODO save the game scene
        Config.Instance.gamePaused = true;
        SceneManager.LoadScene("PauseScene", LoadSceneMode.Additive);

    }

    public void ResumeGame()
    {
        Debug.Log("MenuUI resume game");

        Config.Instance.gamePaused = false;
        //TODO load the saved game scene then uncomment the above code
        SoundEffectsController.Instance.ButtonPressSound();
        SceneManager.UnloadSceneAsync("PauseScene");
    }

    public void Return()
    {
        Debug.Log("MenuUI return");

        SoundEffectsController.Instance.ButtonPressSound();
        if (Config.Instance.gamePaused)
            SceneManager.UnloadSceneAsync("SoundScene");
        else
            SceneManager.LoadScene("MainMenuScene");

        MusicController.Instance.MainMenuMusic();
    }

    public void Tutorial()
    {
        Debug.Log("MenuUI starting tutorial");

        StateLoader.Instance.LoadTutorialState(Constants.tutorialStateStartFileName);
        Config.Instance.SetDifficulty(StateLoader.Instance.gameState.difficulty);
        Config.Instance.tutorialOn = true;
        NewGame();
    }

    public void HardDifficulty()
    {
        Config.Instance.SetDifficulty(Config.Instance.difficulties[2]);
        Config.Instance.tutorialOn = false;
        SaveState.Delete();
        NewGame();
    }

    public void MediumDifficulty()
    {
        Config.Instance.SetDifficulty(Config.Instance.difficulties[1]);
        Config.Instance.tutorialOn = false;
        SaveState.Delete();
        NewGame();
    }

    public void EasyDifficulty()
    {
        Config.Instance.SetDifficulty(Config.Instance.difficulties[0]);
        Config.Instance.tutorialOn = false;
        SaveState.Delete();
        NewGame();
    }

    public void Continue()
    {
        Debug.Log("MenuUI continue");

        if (SaveState.Exists())
        {
            if (Constants.inEditor)
            {
                //Preprocessor Directive to make builds work
                #if (UNITY_EDITOR)
                   UnityEditor.AssetDatabase.Refresh();
                #endif
            }

            StateLoader.Instance.LoadSaveState();
            Config.Instance.SetDifficulty(StateLoader.Instance.gameState.difficulty);
            Config.Instance.tutorialOn = false;
            NewGame(true);
        }
    }

    public void MakeActionsMax()
    {
        Debug.Log("MenuUI make actions max");

        if (UtilsScript.Instance.IsInputStopped())
            return;

        DeckScript.Instance.StartNextCycle(manuallyTriggered: true);
    }
}
