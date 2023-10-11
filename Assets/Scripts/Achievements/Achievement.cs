using System;
using UnityEngine;

[Serializable]
public class Achievement
{
    public enum AchieveType
    {
        Achieve,
        Failure
    }

    public Achievement(string name, string description, string key, AchieveType type)
    {
        this.Name = name;
        this.Description = description;
        this.Key = key;
        _value = PlayerPrefs.GetInt(Key, 0);
        if (Value < 0) Value = 0;
        this.Type = type;
        IsFailureBased = type.Equals(AchieveType.Failure);
        IsAchieveBased = type.Equals(AchieveType.Achieve); ;
    }

    public void LoadValues(Achievement toCopy)
    {
        // do not set by Property!
        _status = toCopy.Status;
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

    private AchieveType Type { get; set; }

    public bool IsFailureBased { get; private set;}
    public bool IsAchieveBased { get; private set; }

    [SerializeField]
    private bool _status;
    /// <summary>
    /// The current status of the achievement which depends on the achievement's type.
    /// </summary>
    public bool Status
    {
        get => _status;
        set
        {
            if (value == _status) return;
            _status = value;

            if (IsAchieveBased)
            {
                if (!value) return;
                Tracker = Actions.MoveTracker;
                TryShowPopup();
            }
            else
            {
                if (value) return;
                Debug.Log($"failed {Name}");
            }
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
        Status = Type.Equals(AchieveType.Failure);
        Tracker = 0;
    }

    public void TryGameWinAchieved()
    {
        if (!Status) return;
        Debug.Log($"achieved {Name}");
        if (IsFailureBased) TryShowPopup();
        if (Config.Instance.CurrentDifficulty.Equals(Difficulties.cheat)) return;
        Value++;
    }

    private void TryShowPopup()
    {
        if (!PersistentSettings.AchievementPopupsEnabled) return;
        AchievementPopup.Instance.ShowAchievement(this);
    }
}
