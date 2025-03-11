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
            timeElapsed += Time.unscaledDeltaTime; // avoid being effected by setting timeScale = 0 when pausing
            yield return null;
        }
        toUpdate.alpha = end;
    }

    public static IEnumerator MoveRectTransformSmoothStep(RectTransform rectTransform, Vector2 targetPosition, float duration)
    {
        Vector2 startPosition = rectTransform.anchoredPosition;
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            rectTransform.anchoredPosition = Vector2SmoothStep(startPosition, targetPosition, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        rectTransform.anchoredPosition = targetPosition;
    }

    public static IEnumerator MoveTransformSmoothDamp(Transform transform, Vector2 targetPosition, float smoothTime)
    {
        Vector2 velocity = Vector2.zero;
        while (Vector2.Distance(transform.position, targetPosition) > 0.05)
        {
            transform.position = Vector2.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            yield return null;
        }
        transform.position = targetPosition;
    }

    public static IEnumerator MoveTransformsSmoothDamp(Transform[] transforms, Vector2 targetPosition, float smoothTime)
    {
        if (transforms.Length == 0) yield break;

        Vector2 velocity = Vector2.zero;
        Vector2 currentPosition = transforms[0].position;
        Vector3 newPosition;
        while (Vector2.Distance(currentPosition, targetPosition) > 0.05)
        {
            currentPosition = Vector2.SmoothDamp(currentPosition, targetPosition, ref velocity, smoothTime);
            newPosition = currentPosition;
            for (int i = 0; i < transforms.Length; i++)
            {
                transforms[i].position = newPosition;
                newPosition.y += GameValues.Transforms.draggedCardYOffset;
                newPosition.z += GameValues.Transforms.draggedCardZOffset;
            }
            yield return null;
        }

        newPosition = targetPosition;
        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i].position = newPosition;
            newPosition.y += GameValues.Transforms.draggedCardYOffset;
            newPosition.z += GameValues.Transforms.draggedCardZOffset;
        }
    }

    public static Vector2 Vector2SmoothStep(Vector2 start, Vector2 end, float durationFraction)
    {
        return new Vector2(
            Mathf.SmoothStep(start.x, end.x, durationFraction),
            Mathf.SmoothStep(start.y, end.y, durationFraction)
        );
    }

    public static Vector3 Vector3SmoothStep(Vector3 start, Vector3 end, float durationFraction)
    {
        return new Vector3(
            Mathf.SmoothStep(start.x, end.x, durationFraction),
            Mathf.SmoothStep(start.y, end.y, durationFraction),
            start.z
        );
    }
}
