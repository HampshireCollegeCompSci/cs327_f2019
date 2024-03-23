using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPopup : MonoBehaviour
{
    // Singleton instance.
    public static AchievementPopup Instance { get; private set; }
    private static readonly WaitForSeconds popupDelay = new(GameValues.Achievements.delayDuration),
        popupDuration = new(GameValues.Achievements.fullVisibleDuration);

    [SerializeField]
    private GameObject popupPrefab;

    private Transform UICanvasTransform;
    private Canvas canvas;
    private Queue<Achievement> popupQueue;
    private Coroutine achievementCoroutine;

    // Initialize the singleton instance.
    void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        // make instance persist across scenes
        DontDestroyOnLoad(this.gameObject);
        UICanvasTransform = GetComponent<Transform>();
        canvas = GetComponent<Canvas>();
        popupQueue = new Queue<Achievement>(Achievements.achievementList.Count);
    }

    public void CameraChange(Camera newCam)
    {
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
        while (popupQueue.Count > 0)
        {
            Achievement achievement = popupQueue.Dequeue();
            if (!achievement.Status) continue;
            GameObject popup = Instantiate(popupPrefab, UICanvasTransform);
            popup.GetComponentInChildren<Text>().text = $"Achievement: {achievement.Name}";
            CanvasGroup popupCG = popup.GetComponent<CanvasGroup>();
            SoundEffectsController.Instance.AchievementSound();
            yield return Animate.FadeCanvasGroup(popupCG, 0, 1, GameValues.Achievements.fadeDuration);
            yield return popupDuration;
            yield return Animate.FadeCanvasGroup(popupCG, 1, 0, GameValues.Achievements.fadeDuration);
            Destroy(popup);
        }
        achievementCoroutine = null;
    }
}
