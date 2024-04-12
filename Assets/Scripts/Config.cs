using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Config : MonoBehaviour
{
    // Singleton instance.
    public static Config Instance { get; private set; }

    // game settings
    public bool continuing;
    public bool prettyColors;

    private bool _autoPlacementEnabled, _hintsEnabled;
    private ColorMode _currentColorMode;
    private List<Camera> cameras;

    // Initialize the singleton instance.
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        // make instance persist across scenes
        DontDestroyOnLoad(this.gameObject);

#if !UNITY_WEBGL
        // all non-mobile platforms write debug to log files without this
        // WEBGL only writes to the console
        Debug.unityLogger.logEnabled = Debug.isDebugBuild;
#endif

        // These must be done in this order
        // Setup the Vibration Package
        Vibration.Init();
        // Check Player Preferences
        PersistentSettings.TryCheckKeys();
        // Check if the game state version needs updating and if the save file needs deleting
        SaveFile.CheckNewGameStateVersion();
        // Set the application frame rate to what was saved
        Application.targetFrameRate = PersistentSettings.FrameRate;

        AutoPlacementEnabled = PersistentSettings.AutoPlacementEnabled;
        HintsEnabled = PersistentSettings.HintsEnabled;
        CurrentColorMode = GameValues.Colors.Modes.List[PersistentSettings.ColorMode];

        cameras = new List<Camera>(SceneManager.sceneCountInBuildSettings);
    }

    public bool IsGamePlayActive { get; set; }

    public bool AutoPlacementEnabled
    {
        get => _autoPlacementEnabled;
        set
        {
            if (value == _autoPlacementEnabled) return;
            _autoPlacementEnabled = value;

            if (value && IsGamePlayActive)
            {
                AchievementsManager.FailedNoHints();
            }
        }
    }

    public bool HintsEnabled
    {
        get => _hintsEnabled;
        set
        {
            if (value == _hintsEnabled) return;
            _hintsEnabled = value;
            TryUpdateGameplayColors();

            if (value && IsGamePlayActive)
            {
                AchievementsManager.FailedNoHints();
            }
        }
    }

    public ColorMode CurrentColorMode
    {
        get => _currentColorMode;
        set
        {
            if (_currentColorMode.Equals(value)) return;
            _currentColorMode = value;
            TryUpdateGameplayColors();
        }
    }

    public Difficulty CurrentDifficulty { get; private set; }

    public bool TutorialOn { get; private set; }

    public string TutorialFileName { get; private set; }

    public Stats OldStats { get; private set; }

    public void AddCamera(Camera newCamera)
    {
        if (cameras.Count != 0)
        {
            cameras[^1].enabled = false;
        }
        cameras.Add(newCamera);
        newCamera.enabled = true;
        AchievementPopup.Instance.CameraChange(newCamera);
    }

    public void RemoveCamera(Camera oldCamera)
    {
        if (cameras.Count == 0) return;
        int oldCameraIndex = cameras.LastIndexOf(oldCamera);
        if (oldCameraIndex == -1)
        {
            Debug.LogError("tried to remove a camera that is not being tracked");
        }
        else
        {
            cameras.RemoveAt(oldCameraIndex);
        }
        if (cameras.Count == 0) return;
        cameras[^1].enabled = true;
        AchievementPopup.Instance.CameraChange(cameras[^1]);
    }

    public void SetDifficulty(Difficulty dif)
    {
        Debug.Log($"setting difficulty to: {dif.Name}");
        CurrentDifficulty = dif;
    }

    public void SetDifficulty(string dif)
    {
        foreach (Difficulty difficlty in Difficulties.difficultyArray)
        {
            if (dif == difficlty.Name)
            {
                SetDifficulty(difficlty);
                return;
            }
        }

        throw new KeyNotFoundException($"the difficulty \"{dif}\" was not found");
    }

    public void PreserveOldStats()
    {
        OldStats = CurrentDifficulty.Stats.ShallowCopy();
    }

    public void SetTutorialOn(string tutorialCommandsFileToLoad)
    {
        TutorialOn = true;
        TutorialFileName = tutorialCommandsFileToLoad;
        HintsEnabled = true;
    }

    public void SetTutorialOff()
    {
        TutorialOn = false;
        HintsEnabled = PersistentSettings.HintsEnabled;
    }
    
    private void TryUpdateGameplayColors()
    {
        if (!IsGamePlayActive) return;

        // reactor's score color
        foreach (var reactor in GameInput.Instance.reactorScripts)
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
