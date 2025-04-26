using System;
using System.Collections.Generic;
using UnityEngine;

public static class AchievementsManager
{
    private static readonly Stack<Achievement> achievementStack = new(AchievementList.achievements.Count);
    private static readonly List<Achievement> achievementFailList = new(AchievementList.achievements.Count);

    public static List<Achievement> GetCurrentAchievements
    {
        get 
        {
            List<Achievement> result = new(achievementStack);
            result.AddRange(achievementFailList);
            return result;
        }
    }

    public static void LoadAchievementValues(List<Achievement> savedAchievements)
    {
        // achievements that need to be sorted though to put in order at the end
        List<Achievement> trackedAchievements = new(savedAchievements.Count);

        foreach (Achievement savedAchievement in savedAchievements)
        {
            Achievement achievementToLoad = AchievementList.achievements.Find(x => x.ID == savedAchievement.ID);
            if (achievementToLoad == null)
            {
                Debug.LogWarning($"Achievement not found, Name: {savedAchievement.Name}, ID: {savedAchievement.ID}");
                continue;
            }

            achievementToLoad.LoadValues(savedAchievement);

            if (achievementToLoad.IsAchieveBased)
            {
                trackedAchievements.Add(achievementToLoad);
            }
            else
            {
                AddFailedAchievement(achievementToLoad);
            }
        }

        trackedAchievements.Sort((x, y) => x.Tracker.CompareTo(y.Tracker));
        trackedAchievements.ForEach(achievement => PushAchievement(achievement));

        if (AchievementList.noHints.Status)
            AchievementList.noHints.Status = !(Config.Instance.HintsEnabled || AutoPlacement.Enabled);
        if (AchievementList.superHard.Status)
            AchievementList.superHard.Status = AchievementList.noHints.Status;
    }

    public static void PushAchievement(Achievement achievement)
    {
        Debug.Log($"adding achievement to the stack: {achievement.Name}, {achievement.Tracker}");
        achievementStack.Push(achievement);
    }

    public static void AddFailedAchievement(Achievement achievement)
    {
        Debug.Log($"failed {achievement.Name}");
        achievementFailList.Add(achievement);
    }

    public static void TryRemoveAchievement(int move)
    {
        if (achievementStack.Count == 0) return;
        if (achievementStack.Peek().Tracker < move) return;
        Achievement achievement = achievementStack.Pop();
        Debug.Log($"removing achievement from the stack: {achievement.Name}, {achievement.Tracker}");
        achievement.Reset();
        TryRemoveAchievement(move);
    }

    public static void ClearAchievements()
    {
        foreach (Achievement achievement in AchievementList.achievements)
        {
            achievement.Reset();
        }
        achievementStack.Clear();
        achievementFailList.Clear();
    }

    public static void NewGameSetAchievements()
    {
        AchievementList.noHints.Status = !(Config.Instance.HintsEnabled || AutoPlacement.Enabled);
        AchievementList.superHard.Status = AchievementList.noHints.Status && Config.Instance.CurrentDifficulty.Equals(Difficulties.hard);
    }

    public static void GameWinLogAchievements()
    {
        //if (Config.Instance.prettyColors)
        //{
        //    Achievements.prettyColors.Achieved = true;
        //}
        if (Actions.MatchCounter == GameValues.GamePlay.matchCount)
        {
            AchievementList.matchAll.Status = true;
        }

        TimeSpan timeSpan = Timer.GetTimeSpan();
        if (timeSpan.CompareTo(TimeSpan.FromMinutes(2)) <= 0)
        {
            AchievementList.speedrun2.Status = true;
            AchievementList.speedrun5.Status = true;
        }
        else if (timeSpan.CompareTo(TimeSpan.FromMinutes(5)) <= 0)
        {
            AchievementList.speedrun5.Status = true;
        }

        if (Config.Instance.CurrentDifficulty.Equals(Difficulties.hard) &&
            AchievementList.noHints.Status == true &&
            AchievementList.noUndo.Status == true)
        {
            AchievementList.superHard.Status = true;
        }

        AchievementList.achievements.ForEach(achievement => achievement.TryGameWinAchieved());
    }

    public static void TryTripleCombo()
    {
        if (AchievementList.tripleCombo.Status) return;
        if (Actions.ConsecutiveMatches != 3) return;
        AchievementList.tripleCombo.Status = true;
    }

    public static void TryCardStack(List<GameObject> cards)
    {
        if (AchievementList.cardStack.Status) return;
        if (cards.Count < 13) return;
        if (cards[^1].GetComponent<CardScript>().Card.Rank.Value != 1) return;
        if (cards[^13].GetComponent<CardScript>().Hidden) return;
        AchievementList.cardStack.Status = true;
    }

    public static void TryReactorsAtLimit()
    {
        if (AchievementList.reactorsAtLimit.Status) return;
        foreach (ReactorScript script in GameInput.Instance.reactorScripts)
        {
            if (script.CardValueCount != Config.Instance.CurrentDifficulty.ReactorLimit)
                return;
        }
        AchievementList.reactorsAtLimit.Status = true;
    }

    public static void AchievedAllReactorsHighAlert()
    {
        AchievementList.allReactorsHighAlert.Status = true;
    }

    public static void FailedNeverReactorHighAlert()
    {
        AchievementList.neverReactorHighAlert.Status = false;
    }

    public static void FailedReactorSize()
    {
        AchievementList.reactorSize.Status = false;
    }

    public static void FailedNoUndo()
    {
        AchievementList.noUndo.Status = false;
        AchievementList.noUndo.Status = false;
    }

    public static void FailedNoDeckFlip()
    {
        AchievementList.noDeckFlip.Status = false;
    }

    public static void FailedNeverMoves()
    {
        AchievementList.neverMoves.Status = false;
    }

    public static void FailedAlwaysMoves()
    {
        AchievementList.alwaysMoves.Status = false;
    }

    public static void FailedNoHints()
    {
        AchievementList.noHints.Status = false;
        AchievementList.superHard.Status = false;
    }
}
