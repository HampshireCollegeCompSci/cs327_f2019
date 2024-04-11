using UnityEngine;

public class TutorialHighlightScript : MonoBehaviour
{
    private const int numFlashTimes = 5;
    private static readonly WaitForSeconds flashDuration = new(0.5f);
    private Coroutine flashCoroutine;

    [SerializeField]
    private UnityEngine.UI.Image windowImage;
    [SerializeField]
    private SpriteRenderer windowSprite;
    [SerializeField]
    private bool isImage;

    void OnEnable()
    {
        //Debug.Log("object was enabled", gameObject);
        StopFlash();

        if (isImage)
        {
            if (windowImage == null)
            {
                Debug.LogError("there is no image assigned", gameObject);
                return;
            }
            flashCoroutine = StartCoroutine(FlashImage());
        }
        else
        {
            if (windowSprite == null)
            {
                Debug.LogError("there is no sprite renderer assigned", gameObject);
                return;
            }
            flashCoroutine = StartCoroutine(FlashSpriteRenderer());
        }
    }

    void OnDisable()
    {
        //Debug.Log("object was disabled", gameObject);
        StopFlash();
    }

    private void StopFlash()
    {
        if (flashCoroutine == null) return;
        StopCoroutine(flashCoroutine);
        flashCoroutine = null;
    }

    private System.Collections.IEnumerator FlashImage()
    {
        for (int i = 0; i < numFlashTimes; i++)
        {
            yield return flashDuration;
            windowImage.enabled = false;
            yield return flashDuration;
            windowImage.enabled = true;
        }
        flashCoroutine = null;
    }

    private System.Collections.IEnumerator FlashSpriteRenderer()
    {
        for (int i = 0; i < numFlashTimes; i++)
        {
            yield return flashDuration;
            windowSprite.enabled = false;
            yield return flashDuration;
            windowSprite.enabled = true;
        }
        flashCoroutine = null;
    }
}
