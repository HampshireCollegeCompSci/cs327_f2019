using UnityEngine;

/// <summary>
/// Stores All PlayerPref keys that are used.
/// </summary>
public static class PlayerPrefKeys
{
    // summary stat keys
    private const string highScoreKey = "HighScore";
    private const string leastMovesKey = "LeastMoves";

    // settings keys
    public const string effectsVolumeKey = "EffectsVolume";
    public const string musicVolumeKey = "MusicVolume";
    public const string vibrationEnabledKey = "VibrationEnabled";
    public const string oldSuitsEnabledKey = "OldSuitsEnabled";

    /// <summary>
    /// Checks if some keys already exist and sets them to a default value if they don't. 
    /// </summary>
    public static void CheckKeys()
    {
        if (!PlayerPrefs.HasKey(effectsVolumeKey))
        {
            PlayerPrefs.SetFloat(effectsVolumeKey, 5);
        }

        if (!PlayerPrefs.HasKey(musicVolumeKey))
        {
            PlayerPrefs.SetFloat(musicVolumeKey, 5);
        }

        if (!PlayerPrefs.HasKey(vibrationEnabledKey))
        {
            PlayerPrefs.SetString(vibrationEnabledKey, true.ToString());
        }

        if (!PlayerPrefs.HasKey(oldSuitsEnabledKey))
        {
            PlayerPrefs.SetString(oldSuitsEnabledKey, true.ToString());
        }
    }

    /// <summary>
    /// Returns the correct HighScore key corresponding to the given difficulty.
    /// </summary>
    /// <param name="difficulty">The difficulty you want the key for.</param>
    /// <returns>Difficulty + HighScoreKey</returns>
    public static string GetHighScoreKey(string difficulty)
    {
        return difficulty + highScoreKey;
    }

    /// <summary>
    /// Returns the correct LeastMove key corresponding to the given difficulty.
    /// </summary>
    /// <param name="difficulty">The difficulty you want the key for.</param>
    /// <returns>Difficulty + LeastMovesKey</returns>
    public static string GetLeastMovesKey(string difficulty)
    {
        return difficulty + leastMovesKey;
    }
}
