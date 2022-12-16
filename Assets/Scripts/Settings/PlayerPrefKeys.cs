using UnityEngine;

/// <summary>
/// Stores All PlayerPref keys that are used.
/// </summary>
public static class PlayerPrefKeys
{
    /// <summary>
    /// Checks if some keys already exist and sets them to a default value if they don't. 
    /// </summary>
    public static void CheckKeys()
    {
        Debug.Log("checking keys");

        if (!PlayerPrefs.HasKey(Constants.Settings.soundEffectsVolumeKey))
        {
            PlayerPrefs.SetInt(Constants.Settings.soundEffectsVolumeKey, Config.GameValues.soundEffectsDefaultVolume);
        }

        if (!PlayerPrefs.HasKey(Constants.Settings.musicVolumeKey))
        {
            PlayerPrefs.SetInt(Constants.Settings.musicVolumeKey, Config.GameValues.musicDefaultVolume);
        }

        if (!PlayerPrefs.HasKey(Constants.Settings.vibrationEnabledKey))
        {
            if (Vibration.HasVibrator())
            {
                PlayerPrefs.SetString(Constants.Settings.vibrationEnabledKey, Config.GameValues.vibrationEnabledDefault.ToString());
            }
            else
            {
                PlayerPrefs.SetString(Constants.Settings.vibrationEnabledKey, false.ToString());
            }
        }

        if (!PlayerPrefs.HasKey(Constants.Settings.foodSuitsEnabledKey))
        {
            PlayerPrefs.SetString(Constants.Settings.foodSuitsEnabledKey, Config.GameValues.foodSuitsEnabledDefault.ToString());
        }

        if (!PlayerPrefs.HasKey(Constants.Settings.frameRateKey))
        {
            PlayerPrefs.SetInt(Constants.Settings.frameRateKey, -1);
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

    public static float GetMusicVolume()
    {
        return ((float)PlayerPrefs.GetInt(Constants.Settings.musicVolumeKey)) / Constants.Settings.musicVolumeDenominator;
    }

    public static float GetSoundEffectsVolume()
    {
        return ((float)PlayerPrefs.GetInt(Constants.Settings.soundEffectsVolumeKey)) / Constants.Settings.soundEffectsVolumeDenominator;
    }

    /// <summary>
    /// Returns the correct HighScore key corresponding to the given difficulty.
    /// </summary>
    /// <param name="difficulty">The difficulty you want the key for.</param>
    /// <returns>Difficulty + HighScoreKey</returns>
    public static string GetHighScoreKey(string difficulty)
    {
        return difficulty + Constants.Summary.highScoreKey;
    }

    /// <summary>
    /// Tries to updates the HighScore key corresponding to the given difficulty with the given update value.
    /// The value will only be updated if the key is empty or the new value is greater than the current.
    /// </summary>
    public static void TrySetHighScore(string difficulty, int update)
    {
        string highScoreKey = GetHighScoreKey(difficulty);
        if (PlayerPrefs.HasKey(highScoreKey))
        {
            if (update > PlayerPrefs.GetInt(highScoreKey))
            {
                PlayerPrefs.SetInt(highScoreKey, update);
            }
        }
        else
        {
            PlayerPrefs.SetInt(highScoreKey, update);
        }
    }

    /// <summary>
    /// Returns the correct LeastMove key corresponding to the given difficulty.
    /// </summary>
    /// <param name="difficulty">The difficulty you want the key for.</param>
    /// <returns>Difficulty + LeastMovesKey</returns>
    public static string GetLeastMovesKey(string difficulty)
    {
        return difficulty + Constants.Summary.leastMovesKey;
    }

    /// <summary>
    /// Updates the LeastMoves key corresponding to the given difficulty with the given update value.
    /// </summary>
    public static void TrySetLeastMoves(string difficulty, int update)
    {
        string leastMovesKey = GetLeastMovesKey(difficulty);
        if (PlayerPrefs.HasKey(leastMovesKey))
        {
            if (update < PlayerPrefs.GetInt(leastMovesKey))
            {
                PlayerPrefs.SetInt(leastMovesKey, update);
            }
        }
        else
        {
            PlayerPrefs.SetInt(leastMovesKey, update);
        }
    }
}
