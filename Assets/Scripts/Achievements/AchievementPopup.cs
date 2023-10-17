using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPopup : MonoBehaviour
{
    // Singleton instance.
    public static AchievementPopup Instance { get; private set; }
    private static readonly WaitForSeconds popupDelay = new(1),
        popupDuration = new(GameValues.AnimationDurataions.achievementPopup);

    [SerializeField]
    private GameObject popupPrefab;
    [SerializeField]
    private Transform UICanvasTransform;

    private Queue<Achievement> popupQueue;
    private Coroutine achievementCoroutine;

    // Initialize the singleton instance.
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            popupQueue = new Queue<Achievement>();
        }
        else if (Instance != this)
        {
            throw new System.ArgumentException("there should not already be an instance of this");
        }
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
            yield return Animate.FadeCanvasGroup(popupCG, 0, 1, GameValues.AnimationDurataions.achievementPopupFade);
            yield return popupDuration;
            yield return Animate.FadeCanvasGroup(popupCG, 1, 0, GameValues.AnimationDurataions.achievementPopupFade);
            Destroy(popup);
        }
        achievementCoroutine = null;
    }
}