using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StartGameSequence : MonoBehaviour
{
    public GameObject camera;

    public GameObject mainButtons;
    public GameObject playButtons;

    public RectTransform panelTransform;
    public GameObject spaceShipWindowObject;

    public GameObject startSequencePanel;
    public GameObject LoadingTextObject;

    private bool sequenceDone;
    private bool gameplayLoaded;

    // Singleton instance.
    public static StartGameSequence Instance = null;

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
        camera.GetComponent<EventSystem>().enabled = false;
        SceneManager.LoadSceneAsync(Constants.gameplayScene, LoadSceneMode.Additive);
        MusicController.Instance.FadeMusicOut();
        StartCoroutine(FadeOutButtons());
    }

    private IEnumerator FadeOutButtons()
    {
        CanvasGroup buttonGroup;
        if (mainButtons.activeSelf)
        {
            buttonGroup = mainButtons.GetComponent<CanvasGroup>();
        }
        else if (playButtons.activeSelf)
        {
            buttonGroup = playButtons.GetComponent<CanvasGroup>();
        }
        else
        {
            Debug.LogError("both button groups are inactive");
            StartCoroutine(PanAndZoomTo(spaceShipWindowObject.transform.position, Config.GameValues.zoomFactor));
            yield break;
        }

        while (buttonGroup.alpha != 0)
        {
            buttonGroup.alpha -= Time.deltaTime * Config.GameValues.fadeOutButtonsSpeed;
            yield return null;
        }
        StartCoroutine(PanAndZoomTo(spaceShipWindowObject.transform.position, Config.GameValues.zoomFactor));
    }

    private IEnumerator PanAndZoomTo(Vector3 positionEnd, float zoomFactor)
    {
        startSequencePanel.SetActive(true);
        Image sequenceImage = startSequencePanel.GetComponent<Image>();
        Color startColor = sequenceImage.color;
        Color endColor = Color.black;

        positionEnd = new Vector3(-positionEnd.x * zoomFactor, -positionEnd.y * zoomFactor, positionEnd.z);
        Vector3 zoomEnd = new Vector3(zoomFactor, zoomFactor, panelTransform.localScale.z);

        float panDistance = Vector3.Distance(panelTransform.position, positionEnd);
        float zoomDistance = Vector3.Distance(panelTransform.localScale, zoomEnd);
        float zoomSpeed = panDistance > zoomDistance ? Config.GameValues.panAndZoomSpeed * zoomDistance / panDistance : Config.GameValues.panAndZoomSpeed;
        float panSpeed = zoomDistance > panDistance ? Config.GameValues.panAndZoomSpeed * panDistance / zoomDistance : Config.GameValues.panAndZoomSpeed;

        Debug.Log("starting p&z");
        float startDistance = Vector3.Distance(panelTransform.localScale, zoomEnd);
        float distanceToEnd = startDistance;

        while (distanceToEnd > 0.001f)
        {
            sequenceImage.color = Color.Lerp(endColor, startColor, distanceToEnd / startDistance);
            panelTransform.position = Vector3.MoveTowards(panelTransform.position, positionEnd, Time.deltaTime * panSpeed);
            panelTransform.localScale = Vector3.MoveTowards(panelTransform.localScale, zoomEnd, Time.deltaTime * zoomSpeed);
            distanceToEnd = Vector3.Distance(panelTransform.localScale, zoomEnd);
            yield return null;
        }

        Debug.Log("start game sequence done");
        sequenceDone = true;
        if (!TryEndSequence())
        {
            LoadingTextObject.SetActive(true);
            //SceneManager.LoadSceneAsync(Constants.gameplayScene, LoadSceneMode.Additive);
        }
    }

    public void GameplayLoaded()
    {
        Debug.Log("gameplay scene is loaded");
        gameplayLoaded = true;
        TryEndSequence();
    }

    public bool TryEndSequence()
    {
        if (gameplayLoaded && sequenceDone)
        {
            Debug.Log("unloading menu scene");
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(Constants.gameplayScene));
            camera.GetComponent<AudioListener>().enabled = false;
            StartGame.Instance.TransitionToGamePlay();
            SceneManager.UnloadSceneAsync(Constants.mainMenuScene);
            return true;
        }
        return false;
    }
}
