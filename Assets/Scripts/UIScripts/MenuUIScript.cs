using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIScript : MonoBehaviour
{
    // MainMenuScene Main buttons
    public GameObject playButton, tutorialButton, aboutButton;

    // MainMenuScene Play buttons
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

        string activeScene = SceneManager.GetActiveScene().name;
        // Update Button text from gameValues
        switch (activeScene)
        {
            case Constants.mainMenuScene:
                Debug.Log("updating main menu buttons");
                // Main buttons
                UpdateButtonText(playButton, Config.Instance.menuSceneButtonsTxtEnglish[0]);
                UpdateButtonText(tutorialButton, Config.Instance.menuSceneButtonsTxtEnglish[1]);
                UpdateButtonText(aboutButton, Config.Instance.menuSceneButtonsTxtEnglish[2]);
                // Play buttons
                UpdateButtonText(continueButton, Config.Instance.levelSceneButtonsTxtEnglish[0]);
                UpdateButtonText(easyButton, Config.Instance.levelSceneButtonsTxtEnglish[1]);
                UpdateButtonText(normalButton, Config.Instance.levelSceneButtonsTxtEnglish[2]);
                UpdateButtonText(hardButton, Config.Instance.levelSceneButtonsTxtEnglish[3]);
                UpdateButtonText(returnButton, Config.Instance.levelSceneButtonsTxtEnglish[4]);
                break;
            case Constants.loadingScene:
                Debug.Log("updating loading text");
                loadingText.GetComponent<Text>().text = Config.Instance.loadingSceneTxtEnglish;
                break;
            case Constants.summaryScene:
                Debug.Log("updating summary scene buttons");
                UpdateButtonText(mainMenuButton, Config.Instance.summarySceneButtonsTxtEnglish[0]);
                UpdateButtonText(playAgainButton, Config.Instance.summarySceneButtonsTxtEnglish[1]);
                break;
            case Constants.gameplayScene:
                if (SceneManager.GetSceneByName("PauseScene").isLoaded)
                {
                    Debug.Log("updating pause scene buttons");
                    UpdateButtonText(resumeButton, Config.Instance.pauseSceneButtonsTxtEnglish[0]);
                    UpdateButtonText(restartButton, Config.Instance.pauseSceneButtonsTxtEnglish[1]);
                    //UpdateButtonText(settingsButton, Config.config.pauseSceneButtonsTxtEnglish[2]);
                    UpdateButtonText(mainMenuButton, Config.Instance.pauseSceneButtonsTxtEnglish[3]);
                }
                break;
            default:
                Debug.Log($"no buttons updated for scene: {activeScene}");
                break;
        }
    }

    private void UpdateButtonText(GameObject button, string text)
    {
        button.transform.GetChild(2).GetComponent<Text>().text = text;
    }

    public void Play()
    {
        Debug.Log("MenuUI play");

        SoundEffectsController.Instance.ButtonPressSound();

        //Preprocessor Directive to make builds work
        #if (UNITY_EDITOR)
            UnityEditor.AssetDatabase.Refresh();
        #endif

        ToggleMainMenuButtons(false);
    }

    private void ToggleMainMenuButtons(bool showMain)
    {
        Debug.Log($"toggling main menu buttons: {showMain}");

        // Main buttons
        playButton.SetActive(showMain);
        tutorialButton.SetActive(showMain);
        aboutButton.SetActive(showMain);

        // Play buttons
        easyButton.SetActive(!showMain);
        normalButton.SetActive(!showMain); 
        hardButton.SetActive(!showMain); 
        returnButton.SetActive(!showMain);

        // Continue button requires a save to exist
        if (showMain)
        {
            continueButton.SetActive(false);
        }
        else if (SaveState.Exists())
        {  
            continueButton.SetActive(true);
        }
    }

    public void NewGame(bool isContinue = false)
    {
        Debug.Log("MenuUI new game");

        SoundEffectsController.Instance.ButtonPressSound();

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

        SceneManager.LoadScene(Constants.loadingScene);
    }

    public void UndoButton()
    {
        Debug.Log("MenuUI undo button");

        if (UtilsScript.Instance.IsInputStopped()) return;

        SoundEffectsController.Instance.UndoPressSound();
        UndoScript.Instance.Undo();
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
        SceneManager.LoadScene(Constants.mainMenuScene);
        MusicController.Instance.MainMenuMusic();
    }

    //possibly be renamed to settings
    public void Settings()
    {
        Debug.Log("MenuUI settings");

        SoundEffectsController.Instance.ButtonPressSound();
        SceneManager.LoadScene(Constants.settingsScene);
    }

    public void About()
    {
        Debug.Log("MenuUI about");

        SoundEffectsController.Instance.ButtonPressSound();
        SceneManager.LoadScene(Constants.aboutScene);
        MusicController.Instance.AboutMusic();
    }

    public void PauseGame()
    {
        Debug.Log("MenuUI pause game");

        if (UtilsScript.Instance.IsInputStopped()) return;

        SoundEffectsController.Instance.PauseMenuButtonSound();
        //TODO save the game scene
        Config.Instance.gamePaused = true;
        SceneManager.LoadScene(Constants.pauseScene, LoadSceneMode.Additive);
    }

    public void ResumeGame()
    {
        Debug.Log("MenuUI resume game");

        Config.Instance.gamePaused = false;
        //TODO load the saved game scene then uncomment the above code
        SoundEffectsController.Instance.ButtonPressSound();
        SceneManager.UnloadSceneAsync(Constants.pauseScene);
    }

    public void Return()
    {
        Debug.Log("MenuUI return");

        // The main menu scene has two sets of buttons that get swapped on/off
        if (SceneManager.GetActiveScene().name == Constants.mainMenuScene)
        {
            ToggleMainMenuButtons(true);
        }
        else
        {
            SceneManager.LoadScene(Constants.mainMenuScene);
            MusicController.Instance.MainMenuMusic();
        }
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
