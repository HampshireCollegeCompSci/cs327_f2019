using System;
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
    public static void TryCheckKeys()
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

        if (Vibration.HasVibrator())
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

        _frameRate = PlayerPrefs.GetInt(Constants.Settings.frameRateKey, -1);
        int maxDeviceScreenRefreshRate = (int)Math.Round(Screen.currentResolution.refreshRateRatio.value);
        if (FrameRate == 0 || FrameRate < -1 || maxDeviceScreenRefreshRate % FrameRate != 0)
        {
            Debug.LogError($"the unsupported frame rate of {FrameRate} was saved, defaulting to the device's default");
            FrameRate = -1;
        }

        Convert.ToBoolean(10);

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

    private static int _frameRate;
    public static int FrameRate
    {
        get => _frameRate;
        set
        {
            if (_frameRate != value)
            {
                _frameRate = value;
                PlayerPrefs.SetInt(Constants.Settings.frameRateKey, value);
            }
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
            if (_movesUntilSave != value)
            {
                _movesUntilSave = value;
                PlayerPrefs.SetInt(Constants.Settings.movesUntilSaveKey, value);
            }
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
        if (PlayerPrefs.GetString(Constants.GameStates.versionKey, defaultValue: "NULL") != Constants.GameStates.version)
        {
            PlayerPrefs.SetString(Constants.GameStates.versionKey, Constants.GameStates.version);
            return true;
        }
        return false;
    }

    public static int GetHighScore(Difficulty difficulty)
    {
        return PlayerPrefs.GetInt(GetHighScoreKey(difficulty), 0);
    }

    public static int GetLeastMoves(Difficulty difficulty)
    {
        return PlayerPrefs.GetInt(GetLeastMovesKey(difficulty), 0);
    }

    public static void SetHighScore(Difficulty difficulty, int value)
    {
        PlayerPrefs.SetInt(GetHighScoreKey(difficulty), value);
    }

    public static void SetLeastMoves(Difficulty difficulty, int value)
    {
        PlayerPrefs.SetInt(GetLeastMovesKey(difficulty), value);
    }

    public static void ClearScores()
    {
        foreach (Difficulty difficulty in GameValues.GamePlay.difficulties)
        {
            SetHighScore(difficulty, 0);
            SetLeastMoves(difficulty, 0);
        }
    }

    /// <summary>
    /// Tries to updates the HighScore key corresponding to the given difficulty with the given update value.
    /// The value will only be updated if the key is blank (0) or if the new value is greater than the current.
    /// </summary>
    public static void TrySetHighScore(Difficulty difficulty, int newScore)
    {
        int oldHighScore = GetHighScore(difficulty);
        if (oldHighScore == 0 || oldHighScore < newScore)
        {
            SetHighScore(difficulty, newScore);
        }
    }
    /// <summary>
    /// Tries to updates the LeastMoves key corresponding to the given difficulty with the given update value.
    /// The value will only be updated if the key is blank (0) or if the new value is greater than the current.
    /// </summary>
    public static void TrySetLeastMoves(Difficulty difficulty, int newMoves)
    {
        int oldLeastMoves = GetLeastMoves(difficulty);
        if (oldLeastMoves == 0 || oldLeastMoves < newMoves)
        {
            SetLeastMoves(difficulty, newMoves);
        }
    }

    /// <summary>
    /// Returns the correct HighScore key corresponding to the given difficulty.
    /// </summary>
    /// <param name="difficulty">The difficulty you want the key for.</param>
    /// <returns>Difficulty name + HighScoreKey</returns>
    private static string GetHighScoreKey(Difficulty difficulty)
    {
        return difficulty.Name + Constants.Summary.highScoreKey;
    }

    /// <summary>
    /// Returns the correct LeastMove key corresponding to the given difficulty.
    /// </summary>
    /// <param name="difficulty">The difficulty you want the key for.</param>
    /// <returns>Difficulty name + LeastMovesKey</returns>
    private static string GetLeastMovesKey(Difficulty difficulty)
    {
        return difficulty.Name + Constants.Summary.leastMovesKey;
    }
}
