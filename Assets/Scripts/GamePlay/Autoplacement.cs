using System;
using UnityEngine;

public static class AutoPlacement
{
    public const string enabledKey = "AutoPlacement",
        timeKey = "AutoPlacementTime",
        speedIndexKey = "SpeedIndex",
        distanceIndexKey = "AutoPlacementDistance";
    
    public const bool enabledDefault = true;
    public const float timeDefault = 0.5f; // seconds

    public const int speedIndexDefault = 2; // an index from speeds
    private static readonly float[] speeds = { 0.20f, 0.15f, 0.1f, 0.07f, 0 };
    private static readonly string[] speedsText = { "Slower", "Slow", "Default", "Fast", "Instant" };
    private static readonly float[] wastePileSpeedMulti = { 1.4f, 1.2f, 1, 0.6f, 0 };

    public const int distanceIndexDefault = 1; // an index from distances
    private static readonly float[] distances = { 0.1f, 0.25f, 0.5f };
    private static readonly string[] distancesText = { "Small", "Medium", "Large"};

    private static bool _enabled;
    private static float _time;
    private static int _speedIndex, _distanceIndex;

    public static void GameLaunch()
    {
        _enabled = Convert.ToBoolean(PlayerPrefs.GetInt(enabledKey, 
            Convert.ToInt32(enabledDefault)));

        _time = PlayerPrefs.GetFloat(timeKey, timeDefault);
        if (_time < 0)
        {
            Debug.LogError($"The unsupported auto placement speed of \"{_time}\" was saved, defaulting to {timeDefault}.");
            Time = timeDefault;
        }

        _speedIndex = PlayerPrefs.GetInt(speedIndexKey, speedIndexDefault);
        if (_speedIndex < 0 || _speedIndex >= speeds.Length)
        {
            Debug.LogError($"The unsupported auto placement speed of \"{_speedIndex}\" was saved, defaulting to {speedIndexDefault}.");
            SpeedIndex = speedIndexDefault;
        }
        else
        {
            SpeedValue = speeds[_speedIndex];
            WastePileSpeed = wastePileSpeedMulti[_speedIndex];
        }

        _distanceIndex = PlayerPrefs.GetInt(distanceIndexKey, distanceIndexDefault);
        if (_distanceIndex < 0 || _distanceIndex >= distances.Length)
        {
            Debug.LogError($"The unsupported auto placement distance of \"{_distanceIndex}\" was saved, defaulting to {distanceIndexDefault}.");
            DistanceIndex = distanceIndexDefault;
        }
        else
        {
            DistanceValue = distances[_distanceIndex];
        }
    }

    public static bool Enabled
    {
        get => _enabled;
        set
        {
            if (value == _enabled) return;
            _enabled = value;
            PlayerPrefs.SetInt(enabledKey, Convert.ToInt32(value));

            if (value && Config.Instance.IsGamePlayActive)
            {
                AchievementsManager.FailedNoHints();
            }
        }
    }

    public static float Time
    {
        get => _time;
        set
        {
            if (value == _time) return;
            _time = value;
            PlayerPrefs.SetFloat(timeKey, value);
        }
    }

    public static int SpeedIndex
    {
        get => _speedIndex;
        set
        {
            if (value == _speedIndex) return;
            _speedIndex = value;
            SpeedValue = speeds[value];
            WastePileSpeed = wastePileSpeedMulti[value];
            PlayerPrefs.SetInt(speedIndexKey, value);
        }
    }

    public static int SpeedsLength => speeds.Length;

    public static string SpeedText => speedsText[SpeedIndex];

    public static float SpeedValue { get; private set; }

    public static float WastePileSpeed { get; private set; }

    public static int DistanceIndex
    {
        get => _distanceIndex;
        set
        {
            if (value == _distanceIndex) return;
            _distanceIndex = value;
            DistanceValue = distances[value];
            PlayerPrefs.SetInt(distanceIndexKey, value);
        }
    }

    public static int DistancesLength => distances.Length;

    public static string DistanceText => distancesText[DistanceIndex];

    public static float DistanceValue;
}
