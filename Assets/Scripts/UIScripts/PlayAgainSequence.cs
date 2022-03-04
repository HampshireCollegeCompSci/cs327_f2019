using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayAgainSequence : MonoBehaviour
{
    public AudioListener audioListener;

    public GameObject sequencePanel;
    public GameObject LoadingTextObject;

    public GameObject spaceBaby;
    public GameObject spaceBabyPlanet;

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

        Config.Instance.tutorialOn = false;
        Config.Instance.continuing = false;
        SceneManager.LoadSceneAsync(Constants.gameplayScene, LoadSceneMode.Additive);

        MusicController.Instance.FadeMusicOut();
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        sequencePanel.SetActive(true);
        Image panelImage = sequencePanel.GetComponent<Image>();
        Color panelColor = new Color(0, 0, 0, 0);
        panelImage.color = panelColor;

        while (panelColor.a < 1)
        {
            panelColor.a += Time.deltaTime * 0.5f;
            panelImage.color = panelColor;
            yield return null;
        }

        // they will flash on screen for one frame during the scene transition without this
        spaceBaby.SetActive(false);
        spaceBabyPlanet.SetActive(false);

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
            audioListener.enabled = false;
            StartScript.Instance.TransitionToGamePlay();
            SceneManager.UnloadSceneAsync(Constants.summaryScene);
            return true;
        }
        return false;
    }
}
