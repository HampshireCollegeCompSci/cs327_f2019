using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUIScript : MonoBehaviour
{
    // MainMenuScene Main buttons
    [SerializeField]
    private GameObject mainButtons, playButton, tutorialButton, settingsButton, aboutButton;

    // MainMenuScene Play buttons
    [SerializeField]
    private GameObject playButtons, continueButton, easyButton, normalButton, hardButton, backButton;

    public void Play()
    {
        Debug.Log("UI Button play");
        //Preprocessor Directive to make builds work
        #if (UNITY_EDITOR)
                UnityEditor.AssetDatabase.Refresh();
        #endif

        ToggleMainMenuButtons(false);
    }

    public void About()
    {
        Debug.Log("UI Button about");
        SceneManager.LoadScene(Constants.ScenesNames.about);
        MusicController.Instance.AboutMusic();
    }

    public void MainMenuBackButton()
    {
        Debug.Log("UI Button Main Menu Back");
        // The main menu scene has two sets of buttons that get swapped on/off
        ToggleMainMenuButtons(true);
    }

    public void Tutorial()
    {
        NewGame(isTutorial: true);
    }

    public void HardDifficulty()
    {
        Config.Instance.SetDifficulty(GameValues.GamePlay.difficulties[2]);
        NewGame();
    }

    public void MediumDifficulty()
    {
        Config.Instance.SetDifficulty(GameValues.GamePlay.difficulties[1]);
        NewGame();
    }

    public void EasyDifficulty()
    {
        Config.Instance.SetDifficulty(GameValues.GamePlay.difficulties[0]);
        NewGame();
    }

    public void Continue()
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

    private void NewGame(bool isContinue = false, bool isTutorial = false)
    {
        Debug.Log("UI Button new game");
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
}
