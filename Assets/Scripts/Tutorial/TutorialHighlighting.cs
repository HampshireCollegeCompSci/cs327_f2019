using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialHighlighting : MonoBehaviour
{
    public static TutorialHighlighting Instance { get; private set; }

    private readonly List<Image> images = new();
    private readonly List<SpriteRenderer> sprites = new();

    private Coroutine flashCoroutine;
    private readonly WaitForSeconds flashWait = new(0.1f);
    private const float startAlpha = 0.1f, endAlpha = 0.7f;
    private const float duration = 1.2f;
    private Color currentColor;

    private void Awake()
    {
        if (Instance != null)
            throw new System.ArgumentException("there should not already be an instance of this");
        Instance = this;
        UpdateFadeColor();
    }

    public void UpdateFadeColor()
    {
        currentColor = Config.Instance.CurrentColorMode.Notify.Color;
    }

    public void AddHighlight(Image image)
    {
        images.Add(image);
        flashCoroutine ??= StartCoroutine(Fade());
    }

    public void AddHighlight(SpriteRenderer sprite)
    {
        sprites.Add(sprite);
        flashCoroutine ??= StartCoroutine(Fade());
    }

    public void RemoveHighlight(Image image)
    {
        images.Remove(image);
        if (images.Count == 0 && sprites.Count == 0)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
    }

    public void RemoveHighlight(SpriteRenderer sprite)
    {
        sprites.Remove(sprite);
        if (sprites.Count == 0 && images.Count == 0)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
    }

    private IEnumerator Fade()
    {
        float start = startAlpha, end = endAlpha;
        float timeElapsed = 0;
        while (true)
        {
            while (timeElapsed < duration)
            {
                currentColor.a = Mathf.Lerp(start, end, timeElapsed / duration);

                foreach (var image in images)
                    image.color = currentColor;
                foreach (var sprite in sprites)
                    sprite.color = currentColor;

                timeElapsed += Time.deltaTime;
                yield return null;
            }
            yield return flashWait;
            (start, end) = (end, start);
            timeElapsed = 0;
        }
    }
}
