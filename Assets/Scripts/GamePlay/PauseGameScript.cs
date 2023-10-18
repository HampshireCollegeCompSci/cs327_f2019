using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseGameScript : MonoBehaviour
{
    public void PauseGameButton()
    {
        if (GameInput.Instance.InputStopped) return;
        GameInput.Instance.InputStopped = true;
        Time.timeScale = 0;
        Timer.PauseWatch();
        SoundEffectsController.Instance.PauseMenuButtonSound();
        MusicController.Instance.Paused = true;
        SceneManager.LoadScene(Constants.ScenesNames.pause, LoadSceneMode.Additive);
    }

    public void ResumeGameButton()
    {
        Time.timeScale = 1;
        Timer.UnPauseWatch();
        SceneManager.UnloadSceneAsync(Constants.ScenesNames.pause);
        MusicController.Instance.Paused = false;
        GameInput.Instance.InputStopped = false;
    }
}
