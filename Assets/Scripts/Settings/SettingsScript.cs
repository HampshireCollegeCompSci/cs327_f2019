using System;
using System.Collections;
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
    private Toggle achievementPopupToggle, vibrationToggle, foodSuitsToggle;

    private List<int> frameRates;

    [SerializeField]
    private Toggle saveGameStateToggle;
    [SerializeField]
    private InputField movesUntilSaveInputField;

    [SerializeField]
    private Toggle hintsToggle;
    [SerializeField]
    private Dropdown colorModeDropdown;
    [SerializeField]
    private Image colorModeMatch, colorModeMove, colorModeOver, colorModeNotify;

    [SerializeField]
    private Button confirmYesButton;

    private bool lockout;
    private Coroutine clearButtonCoroutine;

    private int musicMultiplier;
    private int soundEffectsMultiplier;

    // Start is called before the first frame update
    void Start()
    {
        lockout = true;
        PersistentSettings.TryCheckKeys();

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

        if (Vibration.HasVibrator())
        {
            vibrationToggle.isOn = PersistentSettings.VibrationEnabled;
        }
        else
        {
            // disable vibration toggle
            vibrationToggle.interactable = false;
            vibrationToggle.isOn = false;
        }

        foodSuitsToggle.isOn = PersistentSettings.FoodSuitsEnabled;

        SetupFrameRateSettings();

        saveGameStateToggle.isOn = PersistentSettings.SaveGameStateEnabled;
        movesUntilSaveInputField.text = PersistentSettings.MovesUntilSave.ToString();

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
        PersistentSettings.MusicVolume = volumeUpdate;
        MusicController.Instance.UpdateMaxVolume(volumeUpdate);
    }

    public void SoundEffectsVolumeChange(float update)
    {
        if (lockout) return;

        int volumeUpdate = (int)update;
        soundEffectsVolumeText.text = $"{volumeUpdate * soundEffectsMultiplier}%";
        PersistentSettings.SoundEffectsVolume = volumeUpdate;
        SoundEffectsController.Instance.UpdateMaxVolume(volumeUpdate);
        SoundEffectsController.Instance.ButtonPressSound(vibrate: false);

        if (SpaceBabyController.Instance != null)
        {
            SpaceBabyController.Instance.UpdateMaxVolume(volumeUpdate);
        }
    }

    public void AchievementPopupOnToggle(bool update)
    {
        if (lockout) return;

        PersistentSettings.AchievementPopupsEnabled = update;
        SoundEffectsController.Instance.ButtonPressSound();
    }

    public void VibrationEnabledOnToggle(bool update)
    {
        if (lockout) return;

        PersistentSettings.VibrationEnabled = update;
        SoundEffectsController.Instance.ButtonPressSound();
    }

    public void ThematicSuitArt(bool update)
    {
        if (lockout) return;
        Debug.Log($"seting food suits to: {update}");
        PersistentSettings.FoodSuitsEnabled = update;
        SoundEffectsController.Instance.ButtonPressSound();

        if (SceneManager.GetActiveScene().name.Equals(Constants.ScenesNames.gameplay))
        {
            GameLoader.Instance.ChangeSuitSprites();
        }
    }

    public void TryToClearRecordsButton()
    {
        if (lockout) return;

        confirmYesButton.interactable = false;
        Debug.Log("trying to clear records");
        if (clearButtonCoroutine != null)
        {
            StopCoroutine(clearButtonCoroutine);
        }
        clearButtonCoroutine = StartCoroutine(ButtonDelay());
    }

    public void ClearRecordsConfirmationButton()
    {
        Debug.LogWarning("This doesn't do anything!");
    }

    public void FrameRateChange(float update)
    {
        if (lockout) return;

        int frameRateIndex = (int)update;
        if (frameRateIndex < 0 || frameRateIndex >= frameRates.Count)
        {
            Debug.LogError($"an invalid frame rate index update of {frameRateIndex} was inputted.");
            frameRateIndex = 0;
        }

        int frameRateSetting = frameRates[frameRateIndex];
        Debug.Log($"seting the targetFrameRate to: {frameRateSetting}");
        Application.targetFrameRate = frameRateSetting;
        PersistentSettings.FrameRate = frameRateSetting;
        UpdateFrameRateText(frameRateSetting);
    }

    public void SaveGameStateOnToggle(bool update)
    {
        if (lockout) return;
        Debug.Log($"seting save game state to: {update}");
        PersistentSettings.SaveGameStateEnabled = update;
        SoundEffectsController.Instance.ButtonPressSound();

        if (SceneManager.GetActiveScene().name.Equals(Constants.ScenesNames.gameplay))
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
        Debug.Log($"seting moves until save to: {update}");

        if (Int32.TryParse(update, out int movesUntilSave) && movesUntilSave > 0)
        {
            PersistentSettings.MovesUntilSave = movesUntilSave;
            if (SceneManager.GetActiveScene().name.Equals(Constants.ScenesNames.gameplay))
            {
                StateLoader.Instance.UpdateMovesUntilSave(movesUntilSave);
            }
        }
        else
        {
            movesUntilSaveInputField.text = PersistentSettings.MovesUntilSave.ToString();
        }
    }

    public void HintsEnabledOnToggle(bool update)
    {
        if (lockout) return;
        Debug.Log($"seting hints enabled to: {update}");
        PersistentSettings.HintsEnabled = update;
        Config.Instance.HintsEnabled = update;
        SoundEffectsController.Instance.ButtonPressSound();
    }

    public void ColorModeOnValueChange(int update)
    {
        if (lockout) return;
        Debug.Log($"seting color mode to: {update}");
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
        // target frame rate settings
        // https://docs.unity3d.com/ScriptReference/Application-targetFrameRate.html
        // make a list of most of the supported target frame rates
        // supported means that the screen's maximum refresh rate is divisible by the target

        // refreshRateRatio.value is off from the typical integer by very small amount
        int maxFrameRate = (int) Math.Round(Screen.currentResolution.refreshRateRatio.value);
        frameRates = maxFrameRate switch
        {
            240 => new List<int>(7) { -1, 30, 40, 60, 80, 120, 240 },
            144 => new List<int>(5) { -1, 36, 48, 72, 144 },
            120 => new List<int>(5) { -1, 30, 40, 60, 120 },
            90 => new List<int>(4) { -1, 30, 45, 90 },
            60 => new List<int>(3) { -1, 30, 60 },
            48 => new List<int>(3) { -1, 24, 48 },
            30 => new List<int>(3) { -1, 15, 30 },
            _ => new List<int>(2) { -1, maxFrameRate },
        };

        // -1 is the default for the platform
        int frameRateSetting = PersistentSettings.FrameRate;

        // figure out if the frame rate setting exists in our list of target frame rates
        int frameRateIndex = frameRates.IndexOf(frameRateSetting);
        if (frameRateIndex == -1)
        {
            Debug.LogWarning($"the frame rate of {frameRateSetting} was not found in our list of target frame rates, adding it to them now.");
            bool addedToList = false;
            for (int i = 1; i < frameRates.Count; i++)
            {
                if (frameRateSetting < frameRates[i])
                {
                    frameRates.Insert(i, frameRateSetting);
                    frameRateIndex = i;
                    addedToList = true;
                    break;
                }
            }
            if (!addedToList)
            {
                frameRates.Add(frameRateSetting);
                frameRateIndex = frameRates.Count - 1;
            }
        }

        frameRateSlider.minValue = 0;
        frameRateSlider.maxValue = frameRates.Count - 1;
        frameRateSlider.value = frameRateIndex;

        UpdateFrameRateText(frameRateSetting);
    }

    private void UpdateFrameRateText(int frameRate)
    {
        frameRateText.text = frameRate switch
        {
            -1 => "Default",
            _ => frameRate.ToString()
        };
    }

    private void UpdateColorModeImages(ColorMode update)
    {
        colorModeMatch.color = update.Match.Color;
        colorModeMove.color = update.Move.Color;
        colorModeOver.color = update.Over.Color;
        colorModeNotify.color = update.Notify.Color;
    }

    private IEnumerator ButtonDelay()
    {
        yield return new WaitForSecondsRealtime(2);
        confirmYesButton.interactable = true;
        clearButtonCoroutine = null;
    }
}
