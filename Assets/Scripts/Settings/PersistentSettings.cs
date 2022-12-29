using System;
using UnityEngine;

/// <summary>
/// Manages and stores all settings that are used.
/// </summary>
public static class PersistentSettings
{
    /// <summary>
    /// Sets the keys up and stores their values for repeated use.
    /// </summary>
    public static void CheckKeys()
    {
        Debug.Log("checking keys");

        _soundEffectsVolume = PlayerPrefs.GetInt(Constants.Settings.soundEffectsVolumeKey,
            GameValues.Settings.soundEffectsDefaultVolume);

        _musicVolume = PlayerPrefs.GetInt(Constants.Settings.musicVolumeKey,
            GameValues.Settings.musicDefaultVolume);

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
        if (FrameRate == 0 || FrameRate < -1 || Screen.currentResolution.refreshRate % FrameRate != 0)
        {
            Debug.LogError($"the unsupported frame rate of {FrameRate} was saved, defaulting to the device's default");
            FrameRate = -1;
        }
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

    public static bool NewGameStateVersion()
    {
        if (PlayerPrefs.GetString(Constants.GameStates.versionKey, defaultValue : "NULL") != Constants.GameStates.version)
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
        int oldLeastMoves= GetLeastMoves(difficulty);
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
