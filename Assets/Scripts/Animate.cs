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
            toUpdate.anchoredPosition = Vector2.Lerp(start, end, Smoothstep(timeElapsed, duration));
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
            toUpdate.position = Vector2.Lerp(start, end, Smoothstep(timeElapsed, duration));
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        toUpdate.position = end;
    }

    public static IEnumerator SmoothstepTransformCards(Transform[] toUpdate, Vector2 start, Vector2 end, float duration)
    {
        float timeElapsed = 0;
        Vector3 newPosition;
        while (timeElapsed < duration)
        {
            newPosition = Vector2.Lerp(start, end, Smoothstep(timeElapsed, duration));
            for (int i = 0; i < toUpdate.Length; i++)
            {
                toUpdate[i].position = newPosition;
                newPosition.y += GameValues.Transforms.draggedCardYOffset;
                newPosition.z += GameValues.Transforms.draggedCardXOffset;
            }
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        newPosition = end;
        for (int i = 0; i < toUpdate.Length; i++)
        {
            toUpdate[i].position = newPosition;
            newPosition.y += GameValues.Transforms.draggedCardYOffset;
            newPosition.z += GameValues.Transforms.draggedCardXOffset;
        }
    }

    private static float Smoothstep(float timeElapsed, float duration)
    {
        float t = timeElapsed / duration;
        t = t * t * (3f - 2f * t); // Smoothstep formula
        return t;
    }
}
