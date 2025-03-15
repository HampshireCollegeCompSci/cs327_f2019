using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPopup : MonoBehaviour
{
    // Singleton instance.
    public static AchievementPopup Instance { get; private set; }
    private static readonly WaitForSecondsRealtime popupDelay = new(GameValues.Achievements.delayDuration),
        popupDuration = new(GameValues.Achievements.fullVisibleDuration);

    [SerializeField]
    private GameObject popup;
    private CanvasGroup popupCG;
    private Canvas canvas;
    private Queue<Achievement> popupQueue;
    private Coroutine achievementCoroutine;

    // Initialize the singleton instance.
    void Awake()
    {
        if (Instance != null) return;
        Instance = this;

        popupCG = popup.GetComponent<CanvasGroup>();
        canvas = GetComponent<Canvas>();
        popupQueue = new Queue<Achievement>(Achievements.achievementList.Count);
    }

    public void CameraChange(Camera newCam)
    {
        if (canvas == null) return;
        canvas.worldCamera = newCam;
    }

    public void ShowAchievement(Achievement achievement)
    {
        popupQueue.Enqueue(achievement);
        if (achievementCoroutine != null) return;
        achievementCoroutine = StartCoroutine(AnimateAchievements());
    }

    private IEnumerator AnimateAchievements()
    {
        yield return popupDelay;
        popupCG.alpha = 0;
        popup.SetActive(true);
        while (popupQueue.Count > 0)
        {
            Achievement achievement = popupQueue.Dequeue();
            if (!achievement.Status) continue;
            popup.GetComponentInChildren<Text>().text = $"Achievement: {achievement.Name}";
            SoundEffectsController.Instance.AchievementSound();
            yield return Animate.FadeCanvasGroup(popupCG, 0, 1, GameValues.Achievements.fadeDuration);
            yield return popupDuration;
            yield return Animate.FadeCanvasGroup(popupCG, 1, 0, GameValues.Achievements.fadeDuration);
        }
        popup.SetActive(false);
        achievementCoroutine = null;
    }
}
