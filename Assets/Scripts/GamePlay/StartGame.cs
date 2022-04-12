using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    public Camera mainCamera;
    public AudioListener audioListener;

    // Singleton instance.
    public static StartGame Instance = null;

    private void Awake()
    {
        // Initialize the singleton instance.
        // If there is not already an instance, set it to this.
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            throw new Exception("two of these scripts should not exist at the same time");
        }

        // disable the camera and audio listener because the previous scene is still loaded
        // and there need to be only one of each
        if (StartGameSequence.Instance != null || PlayAgainSequence.Instance != null)
        {
            Debug.Log("disabling gameplay camera and audio listener");
            mainCamera.enabled = false;
            audioListener.enabled = false;
        }
    }

    private void Start()
    {
        // pause the game until the game is loaded and the transition is complete
        Config.Instance.gamePaused = true;

        // load the game to the point that it can be played
        GameLoader.Instance.LoadGame();

        // Inform that the gameplay scene is done loading
        // and try to start the transition to gameplay
        if (StartGameSequence.Instance != null)
        {
            StartGameSequence.Instance.GameplayLoaded();
        }
        else if (PlayAgainSequence.Instance != null)
        {
            PlayAgainSequence.Instance.GameplayLoaded();
        }
        else
        {
            throw new NullReferenceException("A sequence Instance does not exist!");
        }
    }

    public void TransitionToGamePlay()
    {
        mainCamera.enabled = true;
        audioListener.enabled = true;
        // the alert music starts playing ASAP if triggered so make sure not to override it
        if (Config.Instance.tutorialOn)
        {
            MusicController.Instance.TutorialMusic();
        }
        else
        {
            MusicController.Instance.GameMusic(noOverrideAlert: true);
        }

        StartCoroutine(FadeGameplayIn());
    }

    private IEnumerator FadeGameplayIn()
    {
        Image fadeInScreen = this.gameObject.GetComponent<Image>();
        fadeInScreen.enabled = true;
        Color fadeColor = Color.black;

        while (fadeColor.a > 0)
        {
            fadeColor.a -= Time.deltaTime * Config.GameValues.startGameFadeInSpeed;
            fadeInScreen.color = fadeColor;
            yield return null;
        }

        fadeInScreen.enabled = false;
        Config.Instance.gamePaused = false;
    }
}
