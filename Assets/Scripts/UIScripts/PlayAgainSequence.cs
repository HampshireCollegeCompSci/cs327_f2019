using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayAgainSequence : MonoBehaviour
{
    // Singleton instance.
    public static PlayAgainSequence Instance { get; private set; }

    [SerializeField]
    private GameObject cameraObject, sequencePanel, LoadingTextObject;

    [SerializeField]
    private GameObject spaceShip, spaceBaby, spaceBabyPlanet, foodObjects;

    [SerializeField]
    private SummaryTransition summaryTransition;

    [SerializeField]
    private WinSequence winSequence;

    private bool sequenceDone;
    private bool gameplayLoaded;

    // Initialize the singleton instance.
    private void Awake()
    {
        // If there is not already an instance, set it to this.
        if (Instance == null)
        {
            Instance = this;
        }
        //If an instance already exists, destroy whatever this object is to enforce the singleton.
        else if (Instance != this)
        {
            throw new System.Exception("two of these scripts should not exist at the same time");
        }
    }

    public void StartLoadingGame()
    {
        sequenceDone = false;
        SpaceBabyController.Instance.SetInstanceNull();

        SceneManager.LoadSceneAsync(Constants.ScenesNames.gameplay, LoadSceneMode.Additive);

        MusicController.Instance.FadeMusicOut();
        StartCoroutine(FadeOut());
    }

    public void GameplayLoaded()
    {
        Debug.Log("Game is loaded");
        gameplayLoaded = true;
        TryEndSequence();
    }

    public bool TryEndSequence()
    {
        if (gameplayLoaded && sequenceDone)
        {
            Debug.Log("unloading summary scene");
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(Constants.ScenesNames.gameplay));
            cameraObject.SetActive(false);
            StartGame.Instance.TransitionToGamePlay();
            SceneManager.UnloadSceneAsync(Constants.ScenesNames.summary);
            return true;
        }
        return false;
    }

    private IEnumerator FadeOut()
    {
        sequencePanel.SetActive(true);
        yield return Animate.FadeImage(sequencePanel.GetComponent<Image>(),
            GameValues.FadeColors.backFadeIn,
            GameValues.AnimationDurataions.playAgainFadeOut);

        winSequence.StopAllCoroutines();
        spaceBaby.GetComponent<SpaceBabyController>().StopAllCoroutines();
        // these gameobject's images can flash on screen for one frame on scene transition if they aren't disabled
        spaceBaby.SetActive(false);
        spaceShip.SetActive(false);
        spaceBabyPlanet.SetActive(false);
        foodObjects.SetActive(false);
        summaryTransition.StopExplosion();
        yield return null;

        Debug.Log("play again sequence done");
        sequenceDone = true;
        if (!TryEndSequence())
        {
            LoadingTextObject.SetActive(true);
        }
    }
}
