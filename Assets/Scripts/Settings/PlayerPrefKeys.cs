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
            PlayerPrefs.SetFloat(Constants.soundEffectsVolumeKey, 0.5f);
        }

        if (!PlayerPrefs.HasKey(Constants.musicVolumeKey))
        {
            PlayerPrefs.SetFloat(Constants.musicVolumeKey, 0.5f);
        }

        if (!PlayerPrefs.HasKey(Constants.vibrationEnabledKey))
        {
            PlayerPrefs.SetString(Constants.vibrationEnabledKey, true.ToString());
        }

        if (!PlayerPrefs.HasKey(Constants.foodSuitsEnabledKey))
        {
            PlayerPrefs.SetString(Constants.foodSuitsEnabledKey, true.ToString());
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
