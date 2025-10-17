using UnityEngine;

public class TutorialHighlightScript : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.UI.Image windowImage;
    [SerializeField]
    private SpriteRenderer windowSprite;

    void OnEnable()
    {
        if (windowImage != null)
            TutorialHighlighting.Instance.AddHighlight(windowImage);
        if (windowSprite != null)
            TutorialHighlighting.Instance.AddHighlight(windowSprite);
    }

    void OnDisable()
    {
        if (TutorialHighlighting.Instance == null) return;
        if (windowImage != null)
            TutorialHighlighting.Instance.RemoveHighlight(windowImage);
        if (windowSprite != null)
            TutorialHighlighting.Instance.RemoveHighlight(windowSprite);
    }
}
