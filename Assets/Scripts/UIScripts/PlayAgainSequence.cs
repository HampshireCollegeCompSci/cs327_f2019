using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayAgainSequence : MonoBehaviour
{
    public GameObject cameraObject;

    public GameObject sequencePanel;
    public GameObject LoadingTextObject;

    public GameObject spaceShip;
    public GameObject spaceBaby;
    public GameObject spaceBabyPlanet;
    public GameObject foodObjects;
    public SummaryTransition summaryTransition;

    public WinSequence winSequence;

    private bool sequenceDone;
    private bool gameplayLoaded;

    // Singleton instance.
    public static PlayAgainSequence Instance = null;

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
        SpaceBabyController.Instance = null;

        SceneManager.LoadSceneAsync(Constants.gameplayScene, LoadSceneMode.Additive);

        MusicController.Instance.FadeMusicOut();
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        sequencePanel.SetActive(true);
        Image panelImage = sequencePanel.GetComponent<Image>();
        Color panelColor = Config.GameValues.fadeDarkColor;
        panelColor.a = 0;
        panelImage.color = panelColor;
        yield return null;

        while (panelColor.a < 1)
        {
            panelColor.a += Time.deltaTime * Config.GameValues.summaryTransitionSpeed;
            panelImage.color = panelColor;
            yield return null;
        }

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
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(Constants.gameplayScene));
            cameraObject.SetActive(false);
            StartGame.Instance.TransitionToGamePlay();
            SceneManager.UnloadSceneAsync(Constants.summaryScene);
            return true;
        }
        return false;
    }
}
