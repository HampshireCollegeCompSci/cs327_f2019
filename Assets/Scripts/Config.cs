using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Config : MonoBehaviour
{
    // Singleton instance.
    public static Config Instance;

    // game settings
    public bool tutorialOn, nextCycleEnabled;
    public bool continuing;

    public bool prettyColors;

    // game values
    public bool gameOver;
    public bool gameWin;

    public int actions;
    public int score;
    public int consecutiveMatches;

    // long term tracking
    public int moveCounter;
    public int matchCounter;

    private ColorMode _currentColorMode;
    private bool _hintsEnabled;

    private Difficulty _currentDifficulty;
    private int _selectedCardsLayer, _cardLayer;

    private List<Camera> cameras;

    // Initialize the singleton instance.
    private void Awake()
    {
        if (Instance == null)
        {
            // make instance persist across scenes
            DontDestroyOnLoad(this.gameObject);
            Instance = this;

            // These must be done in this order
            // Setup the Vibration Package
            Vibration.Init();
            // Check Player Preferences
            PersistentSettings.TryCheckKeys();
            // Check if the game state version needs updating and if the save file needs deleting
            SaveFile.CheckNewGameStateVersion();
            // Set the application frame rate to what was saved
            Debug.Log($"setting frame rate to: {PersistentSettings.FrameRate}");
            Application.targetFrameRate = PersistentSettings.FrameRate;

            SetHints(PersistentSettings.HintsEnabled);
            SetColorMode(GameValues.Colors.Modes.List[PersistentSettings.ColorMode]);

            _selectedCardsLayer = SortingLayer.NameToID(Constants.SortingLayers.selectedCards);
            _cardLayer = SortingLayer.NameToID(Constants.SortingLayers.card);
            cameras = new List<Camera>(6);
        }
        else if (Instance != this)
        {
            Destroy(gameObject); //deletes copies of global which do not need to exist, so right version is used to get info from
        }
    }

    public bool HintsEnabled => _hintsEnabled;

    public ColorMode CurrentColorMode => _currentColorMode;

    public Difficulty CurrentDifficulty => _currentDifficulty;

    public int SelectedCardsLayer => _selectedCardsLayer;

    public int CardLayer => _cardLayer;

    public void AddCamera(Camera newCamera)
    {
        if (cameras.Count != 0)
        {
            cameras[^1].enabled = false;
        }
        cameras.Add(newCamera);
        newCamera.enabled = true;
    }

    public void RemoveCamera(Camera oldCamera)
    {
        if (cameras.Count == 0) return;
        int oldCameraIndex = cameras.LastIndexOf(oldCamera);
        if (oldCameraIndex == -1) throw new System.Exception("tried to remove a camera that is not being tracked");
        cameras.RemoveAt(oldCameraIndex);
        if (cameras.Count == 0) return;
        cameras[^1].enabled = true;
    }

    public void SetDifficulty(Difficulty dif)
    {
        Debug.Log($"setting difficulty to: {dif}");
        _currentDifficulty = dif;
    }

    public void SetDifficulty(string dif)
    {
        for (int i = 0; i < GameValues.GamePlay.difficulties.Count; i++)
        {
            if (dif == GameValues.GamePlay.difficulties[i].Name)
            {
                SetDifficulty(GameValues.GamePlay.difficulties[i]);
                return;
            }
        }

        throw new KeyNotFoundException($"the difficulty \"{dif}\" was not found");
    }

    public void SetTutorialOn(bool value)
    {
        tutorialOn = value;
        _hintsEnabled = value || PersistentSettings.HintsEnabled;
    }

    public void SetColorMode(ColorMode value)
    {
        if (_currentColorMode.Equals(value)) return;
        _currentColorMode = value;
        TryUpdateGameplayColors();
    }

    public void SetHints(bool update)
    {
        if (_hintsEnabled == update) return;
        _hintsEnabled = update;
        TryUpdateGameplayColors();
    }

    private void TryUpdateGameplayColors()
    {
        if (!SceneManager.GetActiveScene().name.Equals(Constants.ScenesNames.gameplay))
            return;

        // reactor's score color
        foreach (var reactor in UtilsScript.Instance.reactorScripts)
        {
            // toggle the alerts if they're on so that their text color is updated
            if (reactor.Alert)
            {
                reactor.Alert = false;
                reactor.Alert = true;
            }
        }
        // actions colors
        if (ActionCountScript.Instance.AlertLevel.ColorLevel != Constants.ColorLevel.None)
        {
            // toggle the level so the updated color will take effect
            Constants.ColorLevel level = ActionCountScript.Instance.AlertLevel.ColorLevel;
            ActionCountScript.Instance.AlertLevel = GameValues.Colors.normal;
            ActionCountScript.Instance.AlertLevel = level switch
            {
                Constants.ColorLevel.Move => CurrentColorMode.Move,
                Constants.ColorLevel.Over => CurrentColorMode.Over,
                _ => throw new System.ArgumentException($"the color level of {level} is not supported")
            };
        }
    }
}
