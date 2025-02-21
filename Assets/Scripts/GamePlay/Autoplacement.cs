using System;
using UnityEngine;

public static class AutoPlacement
{
    private const string enabledKey = "AutoPlacement",
        speedKey = "AutoPlacementSpeed",
        timeKey = "AutoPlacementTime",
        distanceIndexKey = "AutoPlacementDistance";
    
    private const bool enabledDefault = true;
    private const float speedDefault = 0.4f; // seconds
    private const float timeDefault = 2; // seconds
    private const int distanceIndexDefault = 1; // an index from distances
    private static readonly float[] distances = { 0.1f, 0.25f, 0.5f };
    private static readonly string[] distancesText = { "Small", "Medium", "Large"};

    private static bool _enabled;
    private static float _speed, _time;
    private static int _distanceIndex;

    public static void GameLaunch()
    {
        _enabled = Convert.ToBoolean(PlayerPrefs.GetInt(enabledKey, 
            Convert.ToInt32(enabledDefault)));

        _speed = PlayerPrefs.GetFloat(speedKey, speedDefault);
        if (_speed < 0)
        {
            Debug.LogError($"The unsupported auto placement speed of \"{_speed}\" was saved, defaulting to {speedDefault}.");
            Speed = speedDefault;
        }

        _time = PlayerPrefs.GetFloat(timeKey, timeDefault);
        if (_time < 0)
        {
            Debug.LogError($"The unsupported auto placement speed of \"{_time}\" was saved, defaulting to {timeDefault}.");
            Time = timeDefault;
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

    public static float Speed
    {
        get => _speed;
        set
        {
            if (value == _speed) return;
            _speed = value;
            PlayerPrefs.SetFloat(speedKey, value);
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

    public static int DistanceLength => distances.Length;
    public static string DistanceText => distancesText[DistanceIndex];
    public static float DistanceValue;
}
