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

        if (!PlayerPrefs.HasKey(Constants.soundEffectsVolumeKey))
        {
            PlayerPrefs.SetInt(Constants.soundEffectsVolumeKey, Config.GameValues.soundEffectsDefaultVolume);
        }

        if (!PlayerPrefs.HasKey(Constants.musicVolumeKey))
        {
            PlayerPrefs.SetInt(Constants.musicVolumeKey, Config.GameValues.musicDefaultVolume);
        }

        if (!PlayerPrefs.HasKey(Constants.vibrationEnabledKey))
        {
            if (Vibration.HasVibrator())
            {
                PlayerPrefs.SetString(Constants.vibrationEnabledKey, Config.GameValues.vibrationEnabledDefault.ToString());
            }
            else
            {
                PlayerPrefs.SetString(Constants.vibrationEnabledKey, false.ToString());
            }
        }

        if (!PlayerPrefs.HasKey(Constants.foodSuitsEnabledKey))
        {
            PlayerPrefs.SetString(Constants.foodSuitsEnabledKey, Config.GameValues.foodSuitsEnabledDefault.ToString());
        }

        if (!PlayerPrefs.HasKey(Constants.frameRateKey))
        {
            PlayerPrefs.SetInt(Constants.frameRateKey, -1);
        }
    }

    public static float GetMusicVolume()
    {
        return ((float)PlayerPrefs.GetInt(Constants.musicVolumeKey)) / Constants.musicVolumeDenominator;
    }

    public static float GetSoundEffectsVolume()
    {
        return ((float)PlayerPrefs.GetInt(Constants.soundEffectsVolumeKey)) / Constants.soundEffectsVolumeDenominator;
    }

    /// <summary>
    /// Returns the correct HighScore key corresponding to the given difficulty.
    /// </summary>
    /// <param name="difficulty">The difficulty you want the key for.</param>
    /// <returns>Difficulty + HighScoreKey</returns>
    public static string GetHighScoreKey(string difficulty)
    {
        return difficulty + Constants.highScoreKey;
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
        return difficulty + Constants.leastMovesKey;
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
