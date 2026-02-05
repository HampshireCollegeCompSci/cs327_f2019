using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages and stores all settings that are used.
/// </summary>
public static class PersistentSettings
{
    private static bool hasChecked = false;

    /// <summary>
    /// Sets the keys up and stores their values for repeated use.
    /// </summary>
    public static void OnGameStart()
    {
        if (hasChecked) return;
        Debug.Log("checking keys");

        _soundEffectsVolume = PlayerPrefs.GetInt(Constants.Settings.soundEffectsVolumeKey,
            GameValues.Settings.soundEffectsDefaultVolume);
        if (_soundEffectsVolume < 0 || _soundEffectsVolume > GameValues.Settings.soundEffectsVolumeDenominator)
        {
            SoundEffectsVolume = GameValues.Settings.soundEffectsDefaultVolume;
        }

        _musicVolume = PlayerPrefs.GetInt(Constants.Settings.musicVolumeKey,
            GameValues.Settings.musicDefaultVolume);
        if (_musicVolume < 0 || _musicVolume > GameValues.Settings.musicVolumeDenominator)
        {
            MusicVolume = GameValues.Settings.musicDefaultVolume;
        }

        _achievementPopupsEnabled = Convert.ToBoolean(PlayerPrefs.GetInt(Constants.Settings.achievementPopupsEnabledKey,
                Convert.ToInt32(GameValues.Settings.achievementPopupsEnabledDefault)));

        _matchEffectEnabled = Convert.ToBoolean(PlayerPrefs.GetInt(Constants.Settings.matchEffectEnabledKey,
            Convert.ToInt32(GameValues.Settings.matchEffectEnabledDefault)));

        if (Vibration.HasVibrator)
        {
            _vibrationEnabled = Convert.ToBoolean(PlayerPrefs.GetInt(Constants.Settings.vibrationEnabledKey,
                Convert.ToInt32(GameValues.Settings.vibrationEnabledDefault)));
        }
        else
        {
            _vibrationEnabled = false;
        }

        _foodSuitsEnabled = Convert.ToBoolean(PlayerPrefs.GetInt(Constants.Settings.foodSuitsEnabledKey,
                Convert.ToInt32(GameValues.Settings.foodSuitsEnabledDefault)));

        _deckOrientation = Convert.ToBoolean(PlayerPrefs.GetInt(Constants.Settings.deckOrientationKey,
                Convert.ToInt32(GameValues.Settings.deckOrientationDefault)));

        SupportedFrameRates = GetSupportedRefreshRates();
        DefaultFrameRate = Application.platform == RuntimePlatform.WebGLPlayer ? -1 : SupportedFrameRates[^1];
        int savedFrameRate = PlayerPrefs.GetInt(Constants.Settings.frameRateKey, DefaultFrameRate);
        Debug.Log($"max device frame rate: {SupportedFrameRates[^1]}, our default: {DefaultFrameRate}, saved setting: {FrameRate}");

        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 2;

        if (IsFrameRateSupported(savedFrameRate))
        {
            _frameRate = savedFrameRate;
            Application.targetFrameRate = savedFrameRate;
        }
        else
        {
            Debug.LogWarning($"the unsupported frame rate of {FrameRate} was saved, setting to our default");
            FrameRate = DefaultFrameRate;
        }

        _saveGameStateEnabled = Convert.ToBoolean(PlayerPrefs.GetInt(Constants.Settings.saveGameStateKey,
                Convert.ToInt32(GameValues.Settings.saveGameStateDefault)));

        _movesUntilSave = PlayerPrefs.GetInt(Constants.Settings.movesUntilSaveKey,
            GameValues.Settings.movesUntilSaveDefault);
        if (_movesUntilSave <= 0)
        {
            MovesUntilSave = GameValues.Settings.movesUntilSaveDefault;
        }

        _hintsEnabled = Convert.ToBoolean(PlayerPrefs.GetInt(Constants.Settings.hintsEnabledKey,
                Convert.ToInt32(GameValues.Settings.hintsEnabledDefault)));
        _colorMode = PlayerPrefs.GetInt(Constants.Settings.colorMode, 0);

        hasChecked = true;
    }

    private static int _musicVolume;
    public static int MusicVolume
    {
        get => _musicVolume;
        set
        {
            if (_musicVolume != value)
            {
                _musicVolume = value;
                PlayerPrefs.SetInt(Constants.Settings.musicVolumeKey, value);
            }
        }
    }

    private static int _soundEffectsVolume;
    public static int SoundEffectsVolume
    {
        get => _soundEffectsVolume;
        set
        {
            if (_soundEffectsVolume != value)
            {
                _soundEffectsVolume = value;
                PlayerPrefs.SetInt(Constants.Settings.soundEffectsVolumeKey, value);
            }
        }
    }

    private static bool _achievementPopupsEnabled;
    public static bool AchievementPopupsEnabled
    {
        get => _achievementPopupsEnabled;
        set
        {
            if (_achievementPopupsEnabled != value)
            {
                _achievementPopupsEnabled = value;
                PlayerPrefs.SetInt(Constants.Settings.achievementPopupsEnabledKey,
                    Convert.ToInt32(value));
            }
        }
    }

    private static bool _matchEffectEnabled;
    public static bool MatchEffectEnabled
    {
        get => _matchEffectEnabled;
        set
        {
            if (_matchEffectEnabled != value)
            {
                _matchEffectEnabled = value;
                PlayerPrefs.SetInt(Constants.Settings.matchEffectEnabledKey,
                    Convert.ToInt32(value));
            }
        }
    }

    private static bool _vibrationEnabled;
    public static bool VibrationEnabled
    {
        get => _vibrationEnabled;
        set
        {
            if (_vibrationEnabled != value)
            {
                _vibrationEnabled = value;
                PlayerPrefs.SetInt(Constants.Settings.vibrationEnabledKey,
                    Convert.ToInt32(value));
            }
        }
    }

    private static bool _foodSuitsEnabled;
    public static bool FoodSuitsEnabled
    {
        get => _foodSuitsEnabled;
        set
        {
            if (_foodSuitsEnabled != value)
            {
                _foodSuitsEnabled = value;
                PlayerPrefs.SetInt(Constants.Settings.foodSuitsEnabledKey,
                    Convert.ToInt32(value));
            }
        }
    }

    private static bool _deckOrientation;
    public static bool DeckOrientation
    {
        get => _deckOrientation;
        set
        {
            if (_deckOrientation != value)
            {
                _deckOrientation = value;
                PlayerPrefs.SetInt(Constants.Settings.deckOrientationKey,
                    Convert.ToInt32(value));
            }
        }
    }

    public static int[] SupportedFrameRates { get; private set; }

    public static int DefaultFrameRate;

    private static int _frameRate;
    public static int FrameRate
    {
        get => _frameRate;
        set
        {
            if (value == _frameRate) return;
            if (IsFrameRateSupported(value))
            {
                _frameRate = value;
            }
            else
            {
                Debug.LogWarning($"the frame rate of {value} was not found in our list of supported frame rates, switching to default.");
                _frameRate = DefaultFrameRate;
            }
            PlayerPrefs.SetInt(Constants.Settings.frameRateKey, value);
            Debug.Log($"setting the targetFrameRate to: {value}");
            Application.targetFrameRate = value;
        }
    }

    private static bool _saveGameStateEnabled;
    public static bool SaveGameStateEnabled
    {
        get => _saveGameStateEnabled;
        set
        {
            if (_saveGameStateEnabled != value)
            {
                _saveGameStateEnabled = value;
                PlayerPrefs.SetInt(Constants.Settings.saveGameStateKey,
                    Convert.ToInt32(value));
            }
        }
    }

    private static int _movesUntilSave;
    public static int MovesUntilSave
    {
        get => _movesUntilSave;
        set
        {
            if (_movesUntilSave == value) return;
            _movesUntilSave = value;
            PlayerPrefs.SetInt(Constants.Settings.movesUntilSaveKey, value);
        }
    }

    private static bool _hintsEnabled;
    public static bool HintsEnabled
    {
        get => _hintsEnabled;
        set
        {
            if (_hintsEnabled != value)
            {
                _hintsEnabled = value;
                PlayerPrefs.SetInt(Constants.Settings.hintsEnabledKey,
                    Convert.ToInt32(value));
            }
        }
    }

    private static int _colorMode;
    public static int ColorMode
    {
        get => _colorMode;
        set
        {
            if (_colorMode != value)
            {
                _colorMode = value;
                PlayerPrefs.SetInt(Constants.Settings.colorMode, value);
            }
        }
    }

    public static bool NewGameStateVersion()
    {
        if (PlayerPrefs.GetInt(Constants.GameStates.versionKey, defaultValue: 0) != Constants.GameStates.version)
        {
            PlayerPrefs.SetInt(Constants.GameStates.versionKey, Constants.GameStates.version);
            return true;
        }
        return false;
    }

    public static bool IsFrameRateSupported(int value)
    {
        return Array.IndexOf(SupportedFrameRates, value) != -1;
    }

    private static int[] GetSupportedRefreshRates()
    {
        Resolution[] resolutions = Screen.resolutions;
        List<int> uniqueRates = new(resolutions.Length) { -1 }; // -1 is device default, typically 30hz

        for (int i = 0; i < resolutions.Length; i++)
        {
            int hz = (int)Math.Round(resolutions[i].refreshRateRatio.value);
            if (hz > 0 && !uniqueRates.Contains(hz))
            {
                uniqueRates.Add(hz);
            }
        }

        uniqueRates.Sort();
        return uniqueRates.ToArray();
    }

}
