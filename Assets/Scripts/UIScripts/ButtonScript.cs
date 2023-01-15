using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour
{
    [SerializeField]
    private GameObject pressAnimation;

    public void ButtonClicked()
    {
        SoundEffectsController.Instance.ButtonPressSound();
    }

    public void MainMenuButton()
    {
        Debug.Log("UI Button main menu");
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

    public void SettingsButton()
    {
        Debug.Log("UI Button settings");
        SceneManager.LoadScene(Constants.ScenesNames.settings, LoadSceneMode.Additive);
    }

    public void PauseGameButton()
    {
        Debug.Log("UI Button pause game");
        if (UtilsScript.Instance.InputStopped || Config.Instance.gamePaused) return;
        Time.timeScale = 0;
        Config.Instance.gamePaused = true;
        SoundEffectsController.Instance.PauseMenuButtonSound();
        MusicController.Instance.Paused = true;
        SceneManager.LoadScene(Constants.ScenesNames.pause, LoadSceneMode.Additive);
    }

    public void ResumeGameButton()
    {
        Debug.Log("UI Button resume game");
        Time.timeScale = 1;
        SceneManager.UnloadSceneAsync(Constants.ScenesNames.pause);
        Config.Instance.gamePaused = false;
        MusicController.Instance.Paused = false;
    }

    public void RestartGameButton()
    {
        Debug.Log("UI Button restart");
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

    public void PlayAgain()
    {
        Debug.Log("UI Button play again");
        if (PlayAgainSequence.Instance != null)
        {
            PlayAgainSequence.Instance.StartLoadingGame();
        }
        else
        {
            throw new System.NullReferenceException("A sequence Instance does not exist!");
        }
    }

    public void ButtonPressed()
    {
        pressAnimation.SetActive(true);
    }

    public void ButtonReleased()
    {
        pressAnimation.SetActive(false);
    }
}
