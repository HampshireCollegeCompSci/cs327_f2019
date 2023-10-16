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

    public void BackButton()
    {
        Scene lastSceneToBeLoaded = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        SceneManager.UnloadSceneAsync(lastSceneToBeLoaded);
        if (SceneManager.GetSceneByName(Constants.ScenesNames.mainMenu).isLoaded)
        {
            MusicController.Instance.MainMenuMusic();
        }
    }

    public void MainMenuButton()
    {
        if (Config.Instance.IsGamePlayActive)
        {
            StateLoader.Instance.TryForceWriteState();
            Time.timeScale = 1;
            Config.Instance.IsGamePlayActive = false;
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
        SceneManager.LoadScene(Constants.ScenesNames.settings, LoadSceneMode.Additive);
    }

    public void AchievementsButton()
    {
        TryPlayAboutMusic();
        SceneManager.LoadScene(Constants.ScenesNames.achievement, LoadSceneMode.Additive);
    }

    public void StatsButton()
    {
        TryPlayAboutMusic();
        SceneManager.LoadScene(Constants.ScenesNames.stats, LoadSceneMode.Additive);
    }

    public void AboutButton()
    {
        SceneManager.LoadScene(Constants.ScenesNames.about, LoadSceneMode.Additive);
    }

    public void RestartGameButton()
    {
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
        GameInput.Instance.InputStopped = false;
    }

    public void PlayAgain()
    {
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

    private void TryPlayAboutMusic()
    {
        if (SceneManager.GetSceneByName(Constants.ScenesNames.mainMenu).isLoaded)
        {
            MusicController.Instance.AboutMusic();
        }
    }
}
