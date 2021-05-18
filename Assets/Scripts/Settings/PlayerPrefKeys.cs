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
    public const string soundEffectsVolumeKey = "SoundEffectsVolume";
    public const string musicVolumeKey = "MusicVolume";
    public const string vibrationEnabledKey = "VibrationEnabled";
    public const string foodSuitsEnabledKey = "FoodSuitsEnabled";

    /// <summary>
    /// Checks if some keys already exist and sets them to a default value if they don't. 
    /// </summary>
    public static void CheckKeys()
    {
        Debug.Log("checking keys");

        if (!PlayerPrefs.HasKey(soundEffectsVolumeKey))
        {
            PlayerPrefs.SetFloat(soundEffectsVolumeKey, 0.5f);
        }

        if (!PlayerPrefs.HasKey(musicVolumeKey))
        {
            PlayerPrefs.SetFloat(musicVolumeKey, 0.5f);
        }

        if (!PlayerPrefs.HasKey(vibrationEnabledKey))
        {
            PlayerPrefs.SetString(vibrationEnabledKey, true.ToString());
        }

        if (!PlayerPrefs.HasKey(foodSuitsEnabledKey))
        {
            PlayerPrefs.SetString(foodSuitsEnabledKey, true.ToString());
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
