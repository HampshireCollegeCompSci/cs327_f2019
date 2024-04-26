﻿using System;
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
    private Toggle achievementPopupToggle, vibrationToggle, suitArtToggle, deckOrientationToggle;

    private List<int> frameRates;

    [SerializeField]
    private Toggle saveGameStateToggle;
    [SerializeField]
    private InputField movesUntilSaveInputField;

    [SerializeField]
    private Toggle autoPlacementToggle, hintsToggle;
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

        autoPlacementToggle.isOn = PersistentSettings.AutoPlacementEnabled;
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
        Debug.Log($"setting achievement popups to: {update}");
        PersistentSettings.AchievementPopupsEnabled = update;
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
        Debug.Log($"seting food suits to: {update}");
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
        Debug.Log($"seting deck orientation to: {update}");
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

        if (int.TryParse(update, out int movesUntilSave) && movesUntilSave > 0)
        {
            if (PersistentSettings.MovesUntilSave == movesUntilSave) return;
            Debug.Log($"seting moves until save to: {movesUntilSave}");
            PersistentSettings.MovesUntilSave = movesUntilSave;
            if (IsGamePlaySceneActive())
            {
                StateLoader.Instance.UpdateMovesUntilSave(movesUntilSave);
            }
        }
        else
        {
            Debug.LogWarning($"invalid moves until save detected: {update}");
            movesUntilSaveInputField.text = PersistentSettings.MovesUntilSave.ToString();
        }
    }

    public void AutoPlacementEnabledOnToggle(bool update)
    {
        if (lockout) return;
        Debug.Log($"auto placement set to: {update}");
        PersistentSettings.AutoPlacementEnabled = update;
        Config.Instance.AutoPlacementEnabled = update;
        SoundEffectsController.Instance.ButtonPressSound();
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
            240 => new(7) { -1, 30, 60, 120, 240 },
            144 => new(5) { -1, 36, 48, 72, 144 },
            120 => new(5) { -1, 30, 40, 60, 120 },
            90 => new(4) { -1, 30, 45, 90 },
            60 => new(3) { -1, 30, 60 },
            48 => new(3) { -1, 24, 48 },
            30 => new(3) { -1, 15, 30 },
            _ => new(3) { -1, maxFrameRate / 2, maxFrameRate },
        };

        if (maxFrameRate % 2 != 0)
        {
            Debug.LogError($"this screen has a max refresh rate of {maxFrameRate}, really?");
            frameRates = new(2) { -1, maxFrameRate };
        }

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

    private bool IsGamePlaySceneActive()
    {
        return SceneManager.GetActiveScene().name.Equals(Constants.ScenesNames.gameplay);
    }
}
