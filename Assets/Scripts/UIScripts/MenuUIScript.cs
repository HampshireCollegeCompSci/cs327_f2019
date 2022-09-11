using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIScript : MonoBehaviour
{
    // MainMenuScene Main buttons
    public GameObject mainButtons, playButton, tutorialButton, settingsButton, aboutButton;

    // MainMenuScene Play buttons
    public GameObject playButtons, continueButton, easyButton, normalButton, hardButton, backButton;

    // LoadingScene text
    public Text loadingText;

    // PauseScene buttons
    public GameObject resumeButton, restartButton;

    // PauseScene and SummaryScene button
    public GameObject mainMenuButton;

    // SummaryScene buttons
    public GameObject playAgainButton;

    public Text scoreboard;

    private void Start()
    {
        // get the active scene that is on top
        string activeScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1).name;
        Debug.Log($"the activeScene is: {activeScene}");

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
            case Constants.pauseScene:
                Debug.Log("updating pause scene buttons");
                UpdateButtonText(resumeButton, Config.GameValues.pauseButtonsTxtEnglish[0]);
                UpdateButtonText(restartButton, Config.GameValues.pauseButtonsTxtEnglish[1]);
                UpdateButtonText(settingsButton, Config.GameValues.pauseButtonsTxtEnglish[2]);
                UpdateButtonText(mainMenuButton, Config.GameValues.pauseButtonsTxtEnglish[3]);
                scoreboard.text = Config.Instance.score.ToString();
                break;
            case Constants.summaryScene:
                Debug.Log("updating summary scene buttons");
                UpdateButtonText(mainMenuButton, Config.GameValues.summaryButtonsTxtEnglish[0]);
                UpdateButtonText(playAgainButton, Config.GameValues.summaryButtonsTxtEnglish[1]);
                break;
            case Constants.settingsScene:
                Debug.Log("updating settings scene buttons");
                UpdateButtonText(backButton, Config.GameValues.backButtonTxtEnglish);
                break;
            case Constants.aboutScene:
                Debug.Log("updating about button text");
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

    public void ButtonPressEffect()
    {
        SoundEffectsController.Instance.ButtonPressSound();
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
        mainButtons.SetActive(showMain);

        // Play buttons
        playButtons.SetActive(!showMain);

        // Continue button requires a save to exist
        if (showMain)
        {
            continueButton.SetActive(false);
        }
        else if (SaveState.Exists())
        {
            continueButton.SetActive(true);
        }
        else
        {
            continueButton.SetActive(false);
        }
    }

    public void NewGame(bool isContinue = false, bool isTutorial = false)
    {
        Debug.Log("MenuUI new game");

        SoundEffectsController.Instance.ButtonPressSound();

        Config.Instance.continuing = isContinue;
        Config.Instance.tutorialOn = isTutorial;

        if (StartGameSequence.Instance != null)
        {
            StartGameSequence.Instance.StartLoadingGame();
        }
        else
        {
            throw new System.NullReferenceException("A sequence Instance does not exist!");
        }
    }

    public void UndoButton()
    {
        Debug.Log("MenuUI undo button");

        if (UtilsScript.Instance.InputStopped || Config.Instance.gamePaused) return;

        SoundEffectsController.Instance.UndoPressSound();
        UndoScript.Instance.Undo();
    }

    public void PlayAgain()
    {
        Debug.Log("MenuUI play again");
        SoundEffectsController.Instance.ButtonPressSound();

        if (PlayAgainSequence.Instance != null)
        {
            PlayAgainSequence.Instance.StartLoadingGame();
        }
        else
        {
            throw new System.NullReferenceException("A sequence Instance does not exist!");
        }
    }

    public void Restart()
    {
        Debug.Log("MenuUI restart");
        SoundEffectsController.Instance.ButtonPressSound();
        if (SceneManager.GetSceneByName(Constants.pauseScene).isLoaded)
        {
            SceneManager.UnloadSceneAsync(Constants.pauseScene);
        }
        MusicController.Instance.GameMusic();
        GameLoader.Instance.RestartGame();
    }

    public void MainMenu()
    {
        Debug.Log("MenuUI main menu");
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

        if (UtilsScript.Instance.InputStopped || Config.Instance.gamePaused) return;
        
        Config.Instance.gamePaused = true;
        SoundEffectsController.Instance.PauseMenuButtonSound();
        MusicController.Instance.PauseMusic();
        SceneManager.LoadScene(Constants.pauseScene, LoadSceneMode.Additive);
    }

    public void ResumeGame()
    {
        Debug.Log("MenuUI resume game");

        SoundEffectsController.Instance.ButtonPressSound();
        SceneManager.UnloadSceneAsync(Constants.pauseScene);
        Config.Instance.gamePaused = false;
        MusicController.Instance.PlayMusic();
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
        NewGame(isTutorial: true);
    }

    public void HardDifficulty()
    {
        Config.Instance.SetDifficulty(2);
        NewGame();
    }

    public void MediumDifficulty()
    {
        Config.Instance.SetDifficulty(1);
        NewGame();
    }

    public void EasyDifficulty()
    {
        Config.Instance.SetDifficulty(0);
        NewGame();
    }

    public void Continue()
    {
        if (SaveState.Exists())
        {
            if (Constants.inEditor)
            {
                //Preprocessor Directive to make builds work
                #if (UNITY_EDITOR)
                   UnityEditor.AssetDatabase.Refresh();
                #endif
            }

            NewGame(isContinue: true);
        }
    }

    public void MakeActionsMax()
    {
        if (UtilsScript.Instance.InputStopped || Config.Instance.gamePaused) return;
        Debug.Log("MenuUI make actions max");
        UtilsScript.Instance.StartNextCycle(manuallyTriggered: true);
    }
}
