using System;
using System.Diagnostics;

public static class Timer
{
    private static readonly Stopwatch stopWatch = new();
    public static TimeSpan timerOffset;

    public static void LoadTimerOffset(string timeSpan)
    {
        bool status = TimeSpan.TryParse(timeSpan, out timerOffset);
        if (!status)
        {
            UnityEngine.Debug.LogWarning($"the timespan of \"{timeSpan}\" could not be parsed when loading");
            timerOffset = Config.Instance.CurrentDifficulty.Stats.FastestTime;
        }
    }

    public static void LoadTimerOffset(TimeSpan timeSpan)
    {
        timerOffset = timeSpan;
    }

    public static TimeSpan GetTimeSpan()
    {
        return timerOffset.Add(stopWatch.Elapsed);
    }

    public static void StartWatch()
    {
        stopWatch.Restart();
    }

    public static void PauseWatch()
    {
        stopWatch.Stop();
    }

    public static void UnPauseWatch()
    {
        stopWatch.Start();
    }
}
