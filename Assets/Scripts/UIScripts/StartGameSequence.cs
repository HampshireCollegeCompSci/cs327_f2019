using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGameSequence : MonoBehaviour
{
    // Singleton instance.
    public static StartGameSequence Instance { get; private set; }

    [SerializeField]
    private GameObject cameraObject;
    private Camera cam;

    [SerializeField]
    private Canvas canvas;
    private RenderMode originalCanvasRenderMode;

    [SerializeField]
    private GameObject popupWindow;
    [SerializeField]
    private Button popupContinueButton;

    [SerializeField]
    private CanvasGroup allButtons;
    [SerializeField]
    private GameObject mainButtons, playButtons;
    [SerializeField]
    private GameObject spaceShip;
    [SerializeField]
    private Sprite spaceShipOff;

    [SerializeField]
    private GameObject spaceShipWindowObject;

    [SerializeField]
    private GameObject startSequencePanel, loadingTextObject;
    private Image sequenceImage;

    private Vector3 originalCameraPosition;
    private float originalCameraSize;
    private bool sequenceDone;
    private bool gameplayLoaded;

    // Initialize the singleton instance.
    private void Awake()
    {
        if (Instance != null)
            throw new System.ArgumentException("there should not already be an instance of this");
        Instance = this;
    }

    void Start()
    {
        originalCameraPosition = cameraObject.transform.position;
        cam = cameraObject.GetComponent<Camera>();
        originalCameraSize = cam.orthographicSize;
        sequenceImage = startSequencePanel.GetComponent<Image>();
        originalCanvasRenderMode = canvas.renderMode;
    }

    public void StartLoadingGame()
    {
        startSequencePanel.SetActive(true);
        sequenceDone = false;
        MusicController.Instance.FadeMusicOut();
        canvas.renderMode = RenderMode.WorldSpace;

        StartCoroutine(Animate.FadeCanvasGroup(allButtons, 1, 0, GameValues.AnimationDurataions.buttonFadeOut));
        StartCoroutine(PanAndZoom());
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
        canvas.renderMode = originalCanvasRenderMode;
        cam.orthographicSize = originalCameraSize;
        cameraObject.transform.position = originalCameraPosition;

        playButtons.SetActive(false);
        mainButtons.SetActive(true);
        allButtons.alpha = 1;
        spaceShip.GetComponent<Image>().sprite = spaceShipOff;

        startSequencePanel.SetActive(false);
        MusicController.Instance.FadeMusicIn();
        popupContinueButton.interactable = false;
        popupWindow.SetActive(true);
        StartCoroutine(ButtonDelay());
    }

    private IEnumerator PanAndZoom()
    {
        yield return new WaitForSeconds(0.2f);
        float startingSize = originalCameraSize;
        float targetSize = GameValues.Transforms.zoomFactor;

        Vector3 startingPosition = originalCameraPosition;
        Vector3 endingPosition = spaceShipWindowObject.transform.position;
        endingPosition.z = startingPosition.z;
        
        float duration = GameValues.AnimationDurataions.zoomAndFade;
        float timeElapsed = 0, durationFraction;
        while (timeElapsed < duration)
        {
            durationFraction = timeElapsed / duration;
            sequenceImage.color = Color.Lerp(GameValues.FadeColors.blackA0, GameValues.FadeColors.blackA1, durationFraction);
            cameraObject.transform.position = Animate.Vector3SmoothStep(startingPosition, endingPosition, durationFraction);
            cam.orthographicSize = Mathf.SmoothStep(startingSize, targetSize, durationFraction);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        sequenceImage.color = GameValues.FadeColors.blackA1;

        Debug.Log("start game sequence done");
        sequenceDone = true;
        if (!TryEndSequence())
        {
            loadingTextObject.SetActive(true);
            // reset to starting to that the loading text can be seen
            cam.orthographicSize = startingSize;
            cameraObject.transform.position = startingPosition;
        }
    }

    private IEnumerator ButtonDelay()
    {
        yield return new WaitForSeconds(2);
        popupContinueButton.interactable = true;
    }
}
