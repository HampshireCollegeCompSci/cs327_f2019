using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIScript : MonoBehaviour
{
    // MainMenuScene Main buttons
    public GameObject playButton, tutorialButton, settingsButton, aboutButton;

    // MainMenuScene Play buttons
    public GameObject continueButton, easyButton, normalButton, hardButton, backButton;

    // LoadingScene text
    public Text loadingText;

    // PauseScene buttons
    public GameObject resumeButton, restartButton;

    // PauseScene and SummaryScene button
    public GameObject mainMenuButton;

    // SummaryScene buttons
    public GameObject playAgainButton;

    private void Start()
    {
        Debug.Log("starting MenuUIScript");

        string activeScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1).name;
        Debug.Log($"activeScene {activeScene}");
        // Update Button text from gameValues
        switch (activeScene)
        {
            case Constants.mainMenuScene:
                Debug.Log("updating main menu buttons");
                // Main buttons
                UpdateButtonText(playButton, Config.GameValues.menuButtonsTxtEnglish[0]);
                UpdateButtonText(tutorialButton, Config.GameValues.menuButtonsTxtEnglish[1]);
                UpdateButtonText(settingsButton, Config.GameValues.menuButtonsTxtEnglish[2]);
                UpdateButtonText(aboutButton, Config.GameValues.menuButtonsTxtEnglish[3]);
                // Play buttons
                UpdateButtonText(continueButton, Config.GameValues.levelButtonsTxtEnglish[0]);
                UpdateButtonText(easyButton, Config.GameValues.levelButtonsTxtEnglish[1]);
                UpdateButtonText(normalButton, Config.GameValues.levelButtonsTxtEnglish[2]);
                UpdateButtonText(hardButton, Config.GameValues.levelButtonsTxtEnglish[3]);
                UpdateButtonText(backButton, Config.GameValues.backButtonTxtEnglish);
                break;
            case Constants.loadingScene:
                Debug.Log("updating loading text");
                loadingText.text = Config.GameValues.backButtonTxtEnglish;
                break;
            case Constants.summaryScene:
                Debug.Log("updating summary scene buttons");
                UpdateButtonText(mainMenuButton, Config.GameValues.summaryButtonsTxtEnglish[0]);
                UpdateButtonText(playAgainButton, Config.GameValues.summaryButtonsTxtEnglish[1]);
                break;
            case Constants.pauseScene:
                Debug.Log("updating pause scene buttons");
                UpdateButtonText(resumeButton, Config.GameValues.pauseButtonsTxtEnglish[0]);
                UpdateButtonText(restartButton, Config.GameValues.pauseButtonsTxtEnglish[1]);
                UpdateButtonText(settingsButton, Config.GameValues.pauseButtonsTxtEnglish[2]);
                UpdateButtonText(mainMenuButton, Config.GameValues.pauseButtonsTxtEnglish[3]);
                break;
            case Constants.settingsScene:
                Debug.Log("updating settings scene buttons");
                UpdateButtonText(backButton, Config.GameValues.backButtonTxtEnglish);
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
        backButton.SetActive(!showMain);

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
        SceneManager.LoadScene(Constants.settingsScene, LoadSceneMode.Additive);
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

    public void SettingsBackButton()
    {
        Debug.Log("MenuUI Settings Back");
        SoundEffectsController.Instance.ButtonPressSound();
        SceneManager.UnloadSceneAsync(Constants.settingsScene);
    }

    public void MainMenuBackButton()
    {
        Debug.Log("MenuUI Main Menu Back");
        SoundEffectsController.Instance.ButtonPressSound();
        // The main menu scene has two sets of buttons that get swapped on/off
        ToggleMainMenuButtons(true);
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
        Config.Instance.SetDifficulty(2);
        Config.Instance.tutorialOn = false;
        SaveState.Delete();
        NewGame();
    }

    public void MediumDifficulty()
    {
        Config.Instance.SetDifficulty(1);
        Config.Instance.tutorialOn = false;
        SaveState.Delete();
        NewGame();
    }

    public void EasyDifficulty()
    {
        Config.Instance.SetDifficulty(0);
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
