using System;
using UnityEngine;

[Serializable]
public class Achievement
{
    public Achievement(string name, string description, string key)
    {
        this.Name = name;
        this.Description = description;
        this.Key = key;
        _value = PlayerPrefs.GetInt(Key, 0);
        if (Value < 0) Value = 0;
    }

    public void LoadValues(Achievement toCopy)
    {
        // do not set by Property!
        _achieved = toCopy.Achieved;

        _failed = toCopy.Failed;
        _tracker = toCopy.Tracker;
    }

    public string Name { get; }
    public string Description { get; }

    [field: SerializeField]
    public string Key { get; private set; }

    private int _value;
    public int Value
    {
        get => _value;
        set
        {
            if (value == _value) return;
            _value = value;
            PlayerPrefs.SetInt(Key, value);
        }
    }

    [SerializeField]
    private bool _achieved;
    public bool Achieved
    {
        get => _achieved;
        set
        {
            if (_achieved == value) return;
            _achieved = value;
            if (value)
            {
                Tracker = Actions.MoveTracker;
                if (!PersistentSettings.AchievementPopupsEnabled) return;
                AchievementPopup.Instance.ShowAchievement(this);
            }
        }
    }

    [SerializeField]
    private bool _failed;
    public bool Failed
    {
        get => _failed;
        set
        {
            if (value == _failed) return;
            if (value) Debug.Log($"failed achievement {Name}");
            _failed = value;
        }
    }

    [SerializeField]
    private int _tracker;
    public int Tracker
    {
        get => _tracker;
        private set
        {
            if (_tracker == value) return;
            _tracker = value;
            if (value != 0)
                AchievementsManager.PushAchievement(this);
        }
    }

    public void Reset()
    {
        Achieved = false;
        Failed = false;
        Tracker = 0;
    }

    public void TryGameWinAchieved()
    {
        if (!Achieved) return;
        Debug.Log($"achieved {Name}");
        if (Config.Instance.CurrentDifficulty.Equals(Difficulties.cheat)) return;
        Value++;
    }
    
    public void TryGameWinNoFail()
    {
        if (Failed) return;
        Achieved = true;
    }
}
