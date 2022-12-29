using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGameSequence : MonoBehaviour
{
    // Singleton instance.
    public static StartGameSequence Instance;

    [SerializeField]
    private GameObject cameraObject;

    [SerializeField]
    private GameObject popupWindow;
    [SerializeField]
    private Button popupContinueButton;

    [SerializeField]
    private GameObject mainButtons, playButtons;

    [SerializeField]
    private RectTransform panelTransform;
    [SerializeField]
    private GameObject spaceShipWindowObject;

    [SerializeField]
    private GameObject startSequencePanel, loadingTextObject;

    private Vector3 originalPanelTransformPosition;
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
        MusicController.Instance.FadeMusicOut();
        originalPanelTransformPosition = panelTransform.position;
        StartCoroutine(FadeOutButtons());
        SceneManager.LoadSceneAsync(Constants.ScenesNames.gameplay, LoadSceneMode.Additive);
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
            Debug.Log("unloading gameplay scene");
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(Constants.ScenesNames.gameplay));
            cameraObject.SetActive(false);
            StartGame.Instance.TransitionToGamePlay();
            SceneManager.UnloadSceneAsync(Constants.ScenesNames.mainMenu);
            return true;
        }
        return false;
    }

    public void FailedToLoadGame()
    {
        StopAllCoroutines();
        SceneManager.UnloadSceneAsync(Constants.ScenesNames.gameplay);
        loadingTextObject.SetActive(false);

        CanvasGroup buttonGroup;
        if (mainButtons.activeSelf)
        {
            buttonGroup = mainButtons.GetComponent<CanvasGroup>();
        }
        else
        {
            buttonGroup = playButtons.GetComponent<CanvasGroup>();
        }
        buttonGroup.alpha = 1;
        buttonGroup.interactable = true;

        playButtons.SetActive(false);
        mainButtons.SetActive(true);

        panelTransform.position = originalPanelTransformPosition;
        panelTransform.localScale = Vector3.one;

        startSequencePanel.SetActive(false);
        MusicController.Instance.FadeMusicIn();
        popupContinueButton.interactable = false;
        popupWindow.SetActive(true);
        StartCoroutine(ButtonDelay());
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
            StartCoroutine(PanAndZoomTo(spaceShipWindowObject.transform.position, GameValues.Transforms.zoomFactor));
            yield break;
        }

        buttonGroup.interactable = false;
        yield return Animate.FadeCanvasGroup(buttonGroup, 1, 0, GameValues.AnimationDurataions.buttonFadeOut);
        StartCoroutine(PanAndZoomTo(spaceShipWindowObject.transform.position, GameValues.Transforms.zoomFactor));
    }

    private IEnumerator PanAndZoomTo(Vector3 positionEnd, float zoomFactor)
    {
        startSequencePanel.SetActive(true);
        Image sequenceImage = startSequencePanel.GetComponent<Image>();
        Color startColor = GameValues.FadeColors.blackA0;
        Color endColor = GameValues.FadeColors.blackA1;

        positionEnd = new Vector3(-positionEnd.x * zoomFactor, -positionEnd.y * zoomFactor, positionEnd.z);
        Vector3 zoomEnd = new(zoomFactor, zoomFactor, panelTransform.localScale.z);

        float panDistance = Vector3.Distance(panelTransform.position, positionEnd);
        float zoomDistance = Vector3.Distance(panelTransform.localScale, zoomEnd);
        float zoomSpeed = panDistance > zoomDistance ? GameValues.Transforms.panAndZoomSpeed * zoomDistance / panDistance : GameValues.Transforms.panAndZoomSpeed;
        float panSpeed = zoomDistance > panDistance ? GameValues.Transforms.panAndZoomSpeed * panDistance / zoomDistance : GameValues.Transforms.panAndZoomSpeed;

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
            loadingTextObject.SetActive(true);
        }
    }

    private IEnumerator ButtonDelay()
    {
        yield return new WaitForSeconds(2);
        popupContinueButton.interactable = true;
    }
}
