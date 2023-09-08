using System;
using System.Collections.Generic;
using UnityEngine;

public static class AchievementsManager
{
    private static readonly Stack<Achievement> achievementStack = new(14);

    public static void LoadAchievementValues(List<Achievement> savedAchievements)
    {
        List<Achievement> trackedAchievements = new(savedAchievements.Count);
        foreach (Achievement savedAchievement in savedAchievements)
        {
            Achievement achievementToLoad = Achievements.achievementList.Find(x => x.Key == savedAchievement.Key);
            achievementToLoad?.LoadValues(savedAchievement);
            if (achievementToLoad.Tracker != 0)
            {
                trackedAchievements.Add(achievementToLoad);
            }
        }
        trackedAchievements.Sort((x, y) => x.Tracker.CompareTo(y.Tracker));
        trackedAchievements.ForEach(achievement => PushAchievement(achievement));
    }

    public static void PushAchievement(Achievement achievement)
    {
        Debug.Log($"adding achievement to the stack: {achievement.Name}, {achievement.Tracker}");
        achievementStack.Push(achievement);
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
        foreach (Achievement achievement in Achievements.achievementList)
        {
            achievement.Reset();
        }
        achievementStack.Clear();
    }

    public static void GameWinLogAchievements()
    {
        //if (Config.Instance.prettyColors)
        //{
        //    Achievements.prettyColors.Achieved = true;
        //}
        if (Actions.MatchCounter == GameValues.GamePlay.matchCount)
        {
            Achievements.matchAll.Achieved = true;
        }

        TimeSpan timeSpan = Timer.GetTimeSpan();
        //if (timeSpan.CompareTo(TimeSpan.FromMinutes(2)) <= 0)
        //{
        //    Achievements.speedrun2.Achieved = true;
        //    Achievements.speedrun5.Achieved = true;
        //}
        if (timeSpan.CompareTo(TimeSpan.FromMinutes(5)) <= 0)
        {
            Achievements.speedrun5.Achieved = true;
        }

        Achievements.neverReactorHighAlert.TryGameWinNoFail();
        Achievements.reactorSize.TryGameWinNoFail();
        Achievements.noUndo.TryGameWinNoFail();
        Achievements.noDeckFlip.TryGameWinNoFail();
        Achievements.neverMoves.TryGameWinNoFail();
        Achievements.alwaysMoves.TryGameWinNoFail();

        Achievements.achievementList.ForEach(achievement => achievement.TryGameWinAchieved());
    }

    public static void TryTripleCombo()
    {
        if (Achievements.tripleCombo.Achieved) return;
        if (Actions.ConsecutiveMatches != 3) return;
        Achievements.tripleCombo.Achieved = true;
    }

    public static void TryCardStack(List<GameObject> cards)
    {
        if (Achievements.cardStack.Achieved) return;
        if (cards.Count < 13) return;
        if (cards[^1].GetComponent<CardScript>().Card.Rank.Value != 1) return;
        if (cards[^13].GetComponent<CardScript>().Hidden) return;
        Achievements.cardStack.Achieved = true;
    }

    public static void TryReactorAtLimit(int reactorValue)
    {
        if (reactorValue != Config.Instance.CurrentDifficulty.ReactorLimit) return;
        Achievements.reactorAtLimit.Achieved = true;
    }

    public static void AchievedAllReactorsHighAlert()
    {
        Achievements.allReactorsHighAlert.Achieved = true;
    }

    public static void FailedNeverReactorHighAlert()
    {
        Achievements.neverReactorHighAlert.Failed = true;
    }

    public static void FailedReactorSize()
    {
        Achievements.reactorSize.Failed = true;
    }

    public static void FailedNoUndo()
    {
        Achievements.noUndo.Failed = true;
    }

    public static void FailedNoDeckFlip()
    {
        Achievements.noDeckFlip.Failed = true;
    }

    public static void FailedNeverMoves()
    {
        Achievements.neverMoves.Failed = true;
    }

    public static void FailedAlwaysMoves()
    {
        Achievements.alwaysMoves.Failed = true;
    }
}
