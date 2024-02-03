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
        sequenceDone = false;
        MusicController.Instance.FadeMusicOut();

        allButtons.interactable = false;
        spaceShip.GetComponent<Button>().interactable = false;
        canvas.renderMode = RenderMode.WorldSpace;

        SceneManager.LoadSceneAsync(Constants.ScenesNames.gameplay, LoadSceneMode.Additive);
        StartCoroutine(Animate.FadeCanvasGroup(allButtons, 1, 0, GameValues.AnimationDurataions.buttonFadeOut));
        StartCoroutine(PanAndZoom());
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
        allButtons.interactable = true;
        spaceShip.GetComponent<Button>().interactable = true;
        spaceShip.GetComponent<Image>().sprite = spaceShipOff;

        startSequencePanel.SetActive(false);
        MusicController.Instance.FadeMusicIn();
        popupContinueButton.interactable = false;
        popupWindow.SetActive(true);
        StartCoroutine(ButtonDelay());
    }

    private IEnumerator PanAndZoom()
    {
        float startingSize = originalCameraSize;
        float targetSize = GameValues.Transforms.zoomFactor;

        Vector3 startingPosition = originalCameraPosition;
        Vector3 endingPosition = spaceShipWindowObject.transform.position;
        endingPosition.z = startingPosition.z;
        
        startSequencePanel.SetActive(true);
        Color startColor = GameValues.FadeColors.blackA0;
        Color endColor = GameValues.FadeColors.blackA1;

        float duration = GameValues.AnimationDurataions.zoomAndFade;
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            sequenceImage.color = Color.Lerp(startColor, endColor, t);
            t = t * t * (3f - 2f * t); // Smoothstep formula
            cameraObject.transform.position = Vector3.Lerp(startingPosition, endingPosition, t);
            cam.orthographicSize = Mathf.Lerp(startingSize, targetSize, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        sequenceImage.color = endColor;

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
