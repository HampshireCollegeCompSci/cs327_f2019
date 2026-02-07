using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsScript : MonoBehaviour
{
    [SerializeField]
    private Slider musicSlider, soundEffectsSlider;
    [SerializeField]
    private Text musicVolumeText, soundEffectsVolumeText;

    [SerializeField]
    private Slider frameRateSlider;
    [SerializeField]
    private Text frameRateText;

    [SerializeField]
    private Toggle achievementPopupToggle, matchEffectToggle, vibrationToggle, suitArtToggle, deckOrientationToggle;

    [SerializeField]
    private Toggle saveGameStateToggle;
    [SerializeField]
    private InputField movesUntilSaveInputField;

    [SerializeField]
    private Toggle autoPlacementToggle;
    [SerializeField]
    private InputField autoPlacementTime;
    [SerializeField]
    private Slider autoPlacementSpeedIndexes, autoPlacementDistanceIndexes;
    [SerializeField]
    private Text autoPlacementSpeedText, autoPlacementDistanceText;

    [SerializeField]
    private Toggle hintsToggle;
    [SerializeField]
    private Dropdown colorModeDropdown;
    [SerializeField]
    private Image colorModeMatch, colorModeMove, colorModeOver, colorModeNotify;

    private bool lockout;
    private int musicMultiplier;
    private int soundEffectsMultiplier;

    // Start is called before the first frame update
    void Start()
    {
        lockout = true;
        PersistentSettings.OnGameStart();

        // music volume
        musicSlider.maxValue = GameValues.Settings.musicVolumeDenominator;
        musicMultiplier = 100 / GameValues.Settings.musicVolumeDenominator;
        int volume = PersistentSettings.MusicVolume;
        musicSlider.value = volume;
        musicVolumeText.text = $"{volume * musicMultiplier}%";

        // sound effects volume
        soundEffectsSlider.maxValue = GameValues.Settings.soundEffectsVolumeDenominator;
        soundEffectsMultiplier = 100 / GameValues.Settings.soundEffectsVolumeDenominator;
        volume = PersistentSettings.SoundEffectsVolume;
        soundEffectsSlider.value = volume;
        soundEffectsVolumeText.text = $"{volume * soundEffectsMultiplier}%";

        achievementPopupToggle.isOn = PersistentSettings.AchievementPopupsEnabled;
        matchEffectToggle.isOn = PersistentSettings.MatchEffectEnabled;

        if (Vibration.HasVibrator)
        {
            vibrationToggle.isOn = PersistentSettings.VibrationEnabled;
        }
        else
        {
            // disable vibration toggle section
            vibrationToggle.gameObject.transform.parent.gameObject.SetActive(false);
        }

        suitArtToggle.isOn = PersistentSettings.FoodSuitsEnabled;
        deckOrientationToggle.isOn = PersistentSettings.DeckOrientation;

        SetupFrameRateSettings();

        saveGameStateToggle.isOn = PersistentSettings.SaveGameStateEnabled;
        movesUntilSaveInputField.text = PersistentSettings.MovesUntilSave.ToString();

        autoPlacementToggle.isOn = AutoPlacement.Enabled;
        autoPlacementTime.text = AutoPlacement.Time.ToString();

        autoPlacementSpeedIndexes.minValue = 0;
        autoPlacementSpeedIndexes.maxValue = AutoPlacement.SpeedsLength - 1;
        autoPlacementSpeedIndexes.value = AutoPlacement.SpeedIndex;
        autoPlacementSpeedText.text = AutoPlacement.SpeedText;

        autoPlacementDistanceIndexes.minValue = 0;
        autoPlacementDistanceIndexes.maxValue = AutoPlacement.DistancesLength - 1;
        autoPlacementDistanceIndexes.value = AutoPlacement.DistanceIndex;
        autoPlacementDistanceText.text = AutoPlacement.DistanceText;

        hintsToggle.isOn = PersistentSettings.HintsEnabled;

        var colorModeOptions = new List<Dropdown.OptionData>(GameValues.Colors.Modes.List.Count);
        foreach (var mode in GameValues.Colors.Modes.List)
        {
            colorModeOptions.Add(new Dropdown.OptionData(mode.Name));
        }
        colorModeDropdown.options = colorModeOptions;
        colorModeDropdown.value = PersistentSettings.ColorMode;
        UpdateColorModeImages(GameValues.Colors.Modes.List[PersistentSettings.ColorMode]);

        lockout = false;
    }

    public void MusicVolumeChange(float update)
    {
        if (lockout) return;

        int volumeUpdate = (int)update;
        musicVolumeText.text = $"{volumeUpdate * musicMultiplier}%";
        MusicController.Instance.UpdateMaxVolume(volumeUpdate);
    }

    public void MusicVolumeDone()
    {
        int volumeUpdate = (int)musicSlider.value;
        Debug.Log($"setting music volume setting to: {volumeUpdate}");
        PersistentSettings.MusicVolume = volumeUpdate;
    }

    public void SoundEffectsVolumeChange(float update)
    {
        if (lockout) return;

        int volumeUpdate = (int)update;
        soundEffectsVolumeText.text = $"{volumeUpdate * soundEffectsMultiplier}%";

        SoundEffectsController.Instance.UserVolumeUpdate(volumeUpdate);
    }

    public void SoundEffectsVolumeDone()
    {
        int volumeUpdate = (int)soundEffectsSlider.value;
        Debug.Log($"setting sound effects volume setting to: {volumeUpdate}");
        PersistentSettings.SoundEffectsVolume = volumeUpdate;
    }

    public void AchievementPopupOnToggle(bool update)
    {
        if (lockout) return;
        Debug.Log($"setting achievement popups to: {update}");
        PersistentSettings.AchievementPopupsEnabled = update;
        SoundEffectsController.Instance.ButtonPressSound();
        if (update == false)
            AchievementPopup.Instance.StopPopups();
    }

    public void MatchEffectOnToggle(bool update)
    {
        if (lockout) return;
        Debug.Log($"setting match effect to: {update}");
        PersistentSettings.MatchEffectEnabled = update;
        SoundEffectsController.Instance.ButtonPressSound();
    }

    public void VibrationEnabledOnToggle(bool update)
    {
        if (lockout) return;
        Debug.Log($"setting vibration to: {update}");
        PersistentSettings.VibrationEnabled = update;
        SoundEffectsController.Instance.ButtonPressSound();
    }

    public void SuitArtOnToggle(bool update)
    {
        if (lockout) return;
        Debug.Log($"setting food suits to: {update}");
        PersistentSettings.FoodSuitsEnabled = update;
        SoundEffectsController.Instance.ButtonPressSound();

        if (IsGamePlaySceneActive())
        {
            GameLoader.Instance.ChangeSuitSprites();
        }
    }

    public void DeckOrientationOnToggle(bool update)
    {
        if (lockout) return;
        Debug.Log($"setting deck orientation to: {update}");
        PersistentSettings.DeckOrientation = update;
        SoundEffectsController.Instance.ButtonPressSound();

        if (IsGamePlaySceneActive())
        {
            DeckOrientation.Instance.Flip = update;
        }
    }

    public void FrameRateChange(float update)
    {
        if (lockout) return;

        int frameRateIndex = (int)update;
        if (frameRateIndex < 0 || frameRateIndex >= PersistentSettings.SupportedFrameRates.Length)
        {
            Debug.LogError($"an invalid frame rate index update of {frameRateIndex} was inputted.");
            PersistentSettings.FrameRate = PersistentSettings.DefaultFrameRate;
        }
        else
        {
            PersistentSettings.FrameRate = PersistentSettings.SupportedFrameRates[frameRateIndex];
        }
        UpdateFrameRateText();
    }

    public void SaveGameStateOnToggle(bool update)
    {
        if (lockout) return;
        Debug.Log($"setting save game state to: {update}");
        PersistentSettings.SaveGameStateEnabled = update;
        SoundEffectsController.Instance.ButtonPressSound();

        if (IsGamePlaySceneActive())
        {
            StateLoader.Instance.SetGameStateSaving(update);
        }
        if (!update)
        {
            SaveFile.Delete();
        }
    }

    public void MovesUntilSaveOnEndEdit(string update)
    {
        if (lockout) return;

        if (int.TryParse(update, out int movesUntilSave) &&
            movesUntilSave > 0 && movesUntilSave < 1000)
        {
            Debug.Log($"setting moves until save to: {movesUntilSave}");
            PersistentSettings.MovesUntilSave = movesUntilSave;
            if (IsGamePlaySceneActive())
            {
                StateLoader.Instance.UpdateMovesUntilSave(movesUntilSave);
            }
        }
        else
        {
            Debug.LogWarning($"invalid moves until save input detected: {update}");
            movesUntilSaveInputField.text = PersistentSettings.MovesUntilSave.ToString();
        }
    }

    public void AutoPlacementEnabledOnToggle(bool update)
    {
        if (lockout) return;
        Debug.Log($"setting auto placement to: {update}");
        AutoPlacement.Enabled = update;
        SoundEffectsController.Instance.ButtonPressSound();
    }

    public void AutoPlacementTime(string update)
    {
        if (lockout) return;
        // the input is limited to 3 characters and can take ".01" but displays it as "0.0"
        // so limit and round values to the closet tenth 
        if (float.TryParse(update, out float value) && value >= 0.1)
        {
            value = (float)Math.Round(value, 1);
            Debug.Log($"setting the auto placement time to: {value}");
            autoPlacementTime.text = value.ToString();
            AutoPlacement.Time = value;
        }
        else
        {
            Debug.LogWarning($"invalid auto placement time input detected: {update}");
            autoPlacementTime.text = AutoPlacement.Time.ToString();
        }
    }

    public void AutoPlacementSpeedIndex(float update)
    {
        if (lockout) return;
        int value = (int)update;
        if (value < 0 || value >= AutoPlacement.SpeedsLength)
        {
            Debug.LogWarning($"invalid auto placement speed index input detected: {update}");
            autoPlacementSpeedIndexes.value = AutoPlacement.SpeedIndex;
            autoPlacementSpeedText.text = AutoPlacement.SpeedText;
            return;
        }
        Debug.Log($"setting the auto placement speed index to: {update}");
        AutoPlacement.SpeedIndex = value;
        autoPlacementSpeedText.text = AutoPlacement.SpeedText;
    }

    public void AutoPlacementDistanceIndex(float update)
    {
        if (lockout) return;
        int value = (int)update;
        if (value < 0 || value >= AutoPlacement.DistancesLength)
        {
            Debug.LogWarning($"invalid auto placement distance index input detected: {update}");
            autoPlacementDistanceIndexes.value = AutoPlacement.DistanceIndex;
            autoPlacementDistanceText.text = AutoPlacement.DistanceText;
            return;
        }
        Debug.Log($"setting the auto placement distance index to: {update}");
        AutoPlacement.DistanceIndex = value;
        autoPlacementDistanceText.text = AutoPlacement.DistanceText;
    }

    public void HintsEnabledOnToggle(bool update)
    {
        if (lockout) return;
        Debug.Log($"setting hints enabled to: {update}");
        PersistentSettings.HintsEnabled = update;
        Config.Instance.HintsEnabled = update;
        SoundEffectsController.Instance.ButtonPressSound();
    }

    public void ColorModeOnValueChange(int update)
    {
        if (lockout) return;
        Debug.Log($"setting color mode to: {update}");
        if (update < 0 || update >= GameValues.Colors.Modes.List.Count)
        {
            update = 0;
            Debug.LogError($"the color mode of \"{update}\" is invalid, setting it to 0");
            lockout = true;
            colorModeDropdown.value = 0;
            lockout = false;
        }

        PersistentSettings.ColorMode = update;
        UpdateColorModeImages(GameValues.Colors.Modes.List[update]);
        Config.Instance.CurrentColorMode = GameValues.Colors.Modes.List[update];
        SoundEffectsController.Instance.ButtonPressSound();
    }

    private void SetupFrameRateSettings()
    {
        frameRateSlider.minValue = 0;
        frameRateSlider.maxValue = PersistentSettings.SupportedFrameRates.Length - 1;
        frameRateSlider.value = Array.IndexOf(PersistentSettings.SupportedFrameRates, PersistentSettings.FrameRate);
        UpdateFrameRateText();
    }

    private void UpdateFrameRateText()
    {
        frameRateText.text = PersistentSettings.FrameRate switch
        {
            -1 => "Default",
            _ => PersistentSettings.FrameRate.ToString()
        };
    }

    private void UpdateColorModeImages(ColorMode update)
    {
        colorModeMatch.color = update.Match.Color;
        colorModeMove.color = update.Move.Color;
        colorModeOver.color = update.Over.Color;
        colorModeNotify.color = update.Notify.Color;
    }

    private bool IsGamePlaySceneActive()
    {
        return SceneManager.GetActiveScene().name.Equals(Constants.ScenesNames.gameplay);
    }
}
