using System.Collections.Generic;
using UnityEngine;

public class Config : MonoBehaviour
{
    // Singleton instance.
    public static Config Instance { get; private set; }

    // game settings
    public bool continuing;
    public bool prettyColors;

    private bool _hintsEnabled;

    private ColorMode _currentColorMode;

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
        PersistentSettings.OnGameStart();
        AutoPlacement.GameLaunch();
        // Check if the game state version needs updating and if the save file needs deleting
        SaveFile.CheckNewGameStateVersion();
        // Set the application frame rate to what was saved
        Application.targetFrameRate = PersistentSettings.FrameRate;

        HintsEnabled = PersistentSettings.HintsEnabled;
        CurrentColorMode = GameValues.Colors.Modes.List[PersistentSettings.ColorMode];
    }

    public bool IsGamePlayActive { get; set; }

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

    public void SetDifficulty(Difficulty dif)
    {
        Debug.Log($"setting difficulty to: {dif.Name}");
        CurrentDifficulty = dif;
    }

    public void SetDifficulty(string dif)
    {
        foreach (Difficulty difficulty in Difficulties.difficultyArray)
        {
            if (dif == difficulty.Name)
            {
                SetDifficulty(difficulty);
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
