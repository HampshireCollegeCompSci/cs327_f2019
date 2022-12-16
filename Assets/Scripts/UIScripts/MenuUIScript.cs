using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIScript : MonoBehaviour
{
    // MainMenuScene Main buttons
    [SerializeField]
    private GameObject mainButtons, playButton, tutorialButton, settingsButton, aboutButton;

    // MainMenuScene Play buttons
    [SerializeField]
    private GameObject playButtons, continueButton, easyButton, normalButton, hardButton, backButton;

    // LoadingScene text
    [SerializeField]
    private Text loadingText;

    // PauseScene buttons
    [SerializeField]
    private GameObject resumeButton, restartButton;

    // PauseScene and SummaryScene button
    [SerializeField]
    private GameObject mainMenuButton;

    // SummaryScene buttons
    [SerializeField]
    private GameObject playAgainButton;

    [SerializeField]
    private Text scoreboard;

    private void Start()
    {
        // get the active scene that is on top
        string activeScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1).name;
        Debug.Log($"the activeScene is: {activeScene}");

        // Update Button text from gameValues
        switch (activeScene)
        {
            case Constants.ScenesNames.mainMenu:
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
            case Constants.ScenesNames.pause:
                Debug.Log("updating pause scene buttons");
                UpdateButtonText(resumeButton, Config.GameValues.pauseButtonsTxtEnglish[0]);
                UpdateButtonText(restartButton, Config.GameValues.pauseButtonsTxtEnglish[1]);
                UpdateButtonText(settingsButton, Config.GameValues.pauseButtonsTxtEnglish[2]);
                UpdateButtonText(mainMenuButton, Config.GameValues.pauseButtonsTxtEnglish[3]);
                scoreboard.text = Config.Instance.score.ToString();
                break;
            case Constants.ScenesNames.summary:
                Debug.Log("updating summary scene buttons");
                UpdateButtonText(mainMenuButton, Config.GameValues.summaryButtonsTxtEnglish[0]);
                UpdateButtonText(playAgainButton, Config.GameValues.summaryButtonsTxtEnglish[1]);
                break;
            case Constants.ScenesNames.settings:
                Debug.Log("updating settings scene buttons");
                UpdateButtonText(backButton, Config.GameValues.backButtonTxtEnglish);
                break;
            case Constants.ScenesNames.about:
                Debug.Log("updating about button text");
                UpdateButtonText(backButton, Config.GameValues.backButtonTxtEnglish);
                break;
            default:
                Debug.Log($"no buttons updated for scene: {activeScene}");
                break;
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
        else if (SaveFile.Exists())
        {
            continueButton.SetActive(true);
        }
        else
        {
            continueButton.SetActive(false);
        }
    }

    private void UpdateButtonText(GameObject button, string text)
    {
        button.transform.GetChild(2).GetComponent<Text>().text = text;
    }

    [SerializeField]
    private void ButtonPressEffect()
    {
        SoundEffectsController.Instance.ButtonPressSound();
    }

    [SerializeField]
    private void Play()
    {
        Debug.Log("MenuUI play");

        SoundEffectsController.Instance.ButtonPressSound();

        //Preprocessor Directive to make builds work
#if (UNITY_EDITOR)
        UnityEditor.AssetDatabase.Refresh();
#endif

        ToggleMainMenuButtons(false);
    }

    [SerializeField]
    private void UndoButton()
    {
        Debug.Log("MenuUI undo button");

        if (UtilsScript.Instance.InputStopped || Config.Instance.gamePaused) return;

        SoundEffectsController.Instance.UndoPressSound();
        UndoScript.Instance.Undo();
    }

    [SerializeField]
    private void PlayAgain()
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

    [SerializeField]
    private void Restart()
    {
        Debug.Log("MenuUI restart");
        SoundEffectsController.Instance.ButtonPressSound();
        if (SceneManager.GetSceneByName(Constants.ScenesNames.pause).isLoaded)
        {
            SceneManager.UnloadSceneAsync(Constants.ScenesNames.pause);
            Time.timeScale = 1;
        }
        MusicController.Instance.GameMusic();
        GameLoader.Instance.RestartGame();
        if (MusicController.Instance.Paused)
        {
            MusicController.Instance.Paused = false;
        }
    }

    [SerializeField]
    private void MainMenu()
    {
        Debug.Log("MenuUI main menu");
        SoundEffectsController.Instance.ButtonPressSound();
        if (SceneManager.GetSceneByName(Constants.ScenesNames.pause).isLoaded)
        {
            Time.timeScale = 1;
        }
        SceneManager.LoadScene(Constants.ScenesNames.mainMenu);
        MusicController.Instance.MainMenuMusic();
        if (MusicController.Instance.Paused)
        {
            MusicController.Instance.Paused = false;
        }
    }

    [SerializeField]
    private void Settings()
    {
        Debug.Log("MenuUI settings");

        SoundEffectsController.Instance.ButtonPressSound();
        SceneManager.LoadScene(Constants.ScenesNames.settings, LoadSceneMode.Additive);
    }

    [SerializeField]
    private void About()
    {
        Debug.Log("MenuUI about");

        SoundEffectsController.Instance.ButtonPressSound();
        SceneManager.LoadScene(Constants.ScenesNames.about);
        MusicController.Instance.AboutMusic();
    }

    [SerializeField]
    private void PauseGame()
    {
        Debug.Log("MenuUI pause game");

        if (UtilsScript.Instance.InputStopped || Config.Instance.gamePaused) return;
        Time.timeScale = 0;
        Config.Instance.gamePaused = true;
        SoundEffectsController.Instance.PauseMenuButtonSound();
        MusicController.Instance.Paused = true;
        SceneManager.LoadScene(Constants.ScenesNames.pause, LoadSceneMode.Additive);
    }

    [SerializeField]
    private void ResumeGame()
    {
        Debug.Log("MenuUI resume game");
        Time.timeScale = 1;
        SoundEffectsController.Instance.ButtonPressSound();
        SceneManager.UnloadSceneAsync(Constants.ScenesNames.pause);
        Config.Instance.gamePaused = false;
        MusicController.Instance.Paused = false;
    }

    [SerializeField]
    private void SettingsBackButton()
    {
        Debug.Log("MenuUI Settings Back");
        SoundEffectsController.Instance.ButtonPressSound();
        SceneManager.UnloadSceneAsync(Constants.ScenesNames.settings);
    }

    [SerializeField]
    private void MainMenuBackButton()
    {
        Debug.Log("MenuUI Main Menu Back");
        SoundEffectsController.Instance.ButtonPressSound();
        // The main menu scene has two sets of buttons that get swapped on/off
        ToggleMainMenuButtons(true);
    }

    [SerializeField]
    private void Tutorial()
    {
        NewGame(isTutorial: true);
    }

    [SerializeField]
    private void HardDifficulty()
    {
        Config.Instance.SetDifficulty(2);
        NewGame();
    }

    [SerializeField]
    private void MediumDifficulty()
    {
        Config.Instance.SetDifficulty(1);
        NewGame();
    }

    [SerializeField]
    private void EasyDifficulty()
    {
        Config.Instance.SetDifficulty(0);
        NewGame();
    }

    [SerializeField]
    private void Continue()
    {
        if (SaveFile.Exists())
        {
            if (Application.isEditor)
            {
                //Preprocessor Directive to make builds work
                #if (UNITY_EDITOR)
                    UnityEditor.AssetDatabase.Refresh();
                #endif
            }

            NewGame(isContinue: true);
        }
    }

    [SerializeField]
    private void MakeActionsMax()
    {
        if (UtilsScript.Instance.InputStopped || Config.Instance.gamePaused) return;
        Debug.Log("MenuUI make actions max");
        UtilsScript.Instance.StartNextCycle(manuallyTriggered: true);
    }
}
