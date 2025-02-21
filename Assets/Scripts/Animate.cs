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
            toUpdate.anchoredPosition = SmoothstepVector2(start, end, timeElapsed / duration);
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
            toUpdate.position = SmoothstepVector2(start, end, timeElapsed / duration);
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
            newPosition = SmoothstepVector2(start, end, timeElapsed / duration);
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

    private static Vector2 SmoothstepVector2(Vector2 start, Vector2 end, float durationFraction)
    {
        return new Vector2(
            Mathf.SmoothStep(start.x, end.x, durationFraction),
            Mathf.SmoothStep(start.y, end.y, durationFraction)
        );
    }
}
