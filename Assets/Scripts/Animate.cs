using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class Animate
{
    public static IEnumerator FadeImage(Image toUpdate, FadeColorPair fadeColor, float duration)
    {
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            toUpdate.color = Color.Lerp(fadeColor.startColor, fadeColor.endColor, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        toUpdate.color = fadeColor.endColor;
    }

    public static IEnumerator FadeCanvasGroup(CanvasGroup toUpdate, float start, float end, float duration)
    {
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            toUpdate.alpha = Mathf.Lerp(start, end, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        toUpdate.alpha = end;
    }

    public static IEnumerator SmoothstepRectTransform(RectTransform toUpdate, Vector2 start, Vector2 end, float duration)
    {
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            t = t * t * (3f - 2f * t); // Smoothstep formula
            toUpdate.anchoredPosition = Vector2.Lerp(start, end, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        toUpdate.anchoredPosition = end;
    }

    public static IEnumerator SmoothstepTransform(Transform toUpdate, Vector2 start, Vector2 end, float duration)
    {
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            t = t * t * (3f - 2f * t); // Smoothstep formula
            toUpdate.position = Vector2.Lerp(start, end, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        toUpdate.position = end;
    }
}
