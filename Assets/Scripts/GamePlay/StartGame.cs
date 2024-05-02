using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    // Singleton instance.
    public static StartGame Instance { get; private set; }

    [SerializeField]
    private GameObject mainCameraObject;

    private void Awake()
    {
        if (Instance != null)
        {
            throw new ArgumentException("there should not already be an instance of this");
        }

        Instance = this;

        // disable the camera and audio listener because the previous scene is still loaded
        // and there need to be only one of each
        if (StartGameSequence.Instance != null || PlayAgainSequence.Instance != null)
        {
            Debug.Log("disabling gameplay camera, audio listener and event system");
            mainCameraObject.SetActive(false);
        }
    }

    private void Start()
    {
        // load the game to the point that it can be played
        if (!GameLoader.Instance.LoadGame())
        {
            if (StartGameSequence.Instance != null)
            {
                StartGameSequence.Instance.FailedToLoadGame();
                return;
            }
            else
            {
                throw new Exception("a start game sequence instance doesn't exist!");
            }
        }

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
        Config.Instance.IsGamePlayActive = true;
    }

    public void TransitionToGamePlay()
    {
        mainCameraObject.SetActive(true);
        mainCameraObject.GetComponent<EventSystem>().enabled = true;
        // the alert music starts playing ASAP if triggered so make sure not to override it
        if (Config.Instance.TutorialOn)
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
        yield return Animate.FadeImage(fadeInScreen, GameValues.FadeColors.blackFadeOut, GameValues.AnimationDurataions.startGameFadeIn);
        fadeInScreen.enabled = false;
        GameInput.Instance.InputStopped = false;
        Timer.StartWatch();
    }
}
