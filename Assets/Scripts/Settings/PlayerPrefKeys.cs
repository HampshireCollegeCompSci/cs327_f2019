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
            PlayerPrefs.SetFloat(Constants.soundEffectsVolumeKey, Config.GameValues.soundEffectsDefaultVolume);
        }

        if (!PlayerPrefs.HasKey(Constants.musicVolumeKey))
        {
            PlayerPrefs.SetFloat(Constants.musicVolumeKey, Config.GameValues.musicDefaultVolume);
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
    /// Returns the correct LeastMove key corresponding to the given difficulty.
    /// </summary>
    /// <param name="difficulty">The difficulty you want the key for.</param>
    /// <returns>Difficulty + LeastMovesKey</returns>
    public static string GetLeastMovesKey(string difficulty)
    {
        return difficulty + Constants.leastMovesKey;
    }
}
