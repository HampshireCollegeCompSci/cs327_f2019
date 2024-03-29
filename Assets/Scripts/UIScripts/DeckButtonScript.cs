using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DeckButtonScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private static readonly WaitForSeconds buttonWait = new(0.04f);

    private int buttonAnimationIndex;

    // Singleton instance.
    public static DeckButtonScript Instance { get; private set; }

    [SerializeField]
    private Image buttonImage;
    [SerializeField]
    private Sprite[] buttonAnimation;

    private bool mouseOverButton, mousePressingButton;
    private Coroutine buttonCoroutine;

    public bool ButtonReady { get; set; } = true;

    // Initialize the singleton instance.
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            throw new System.ArgumentException("there should not already be an instance of this");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOverButton = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOverButton = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!ButtonReady || GameInput.Instance.InputStopped) return;
        ButtonReady = false;
        mousePressingButton = true;
        GameInput.Instance.InputStopped = true;

        if (buttonCoroutine != null)
        {
            StopCoroutine(buttonCoroutine);
        }
        buttonCoroutine = StartCoroutine(ButtonDown());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!mousePressingButton) return;
        mousePressingButton = false;
        GameInput.Instance.InputStopped = false;
        if (mouseOverButton)
        {
            DeckScript.Instance.TryDealing();
        }
        else
        {
            StartButtonUp();
        }
    }

    public void StartButtonUp()
    {
        if (buttonCoroutine != null)
        {
            StopCoroutine(buttonCoroutine);
        }
        buttonCoroutine = StartCoroutine(ButtonUp());
    }

    private IEnumerator ButtonDown()
    {
        if (buttonAnimationIndex <= 0)
        {
            buttonAnimationIndex = 1;
        }

        for (; buttonAnimationIndex < buttonAnimation.Length; buttonAnimationIndex++)
        {
            buttonImage.sprite = buttonAnimation[buttonAnimationIndex];
            yield return buttonWait;
        }
        buttonCoroutine = null;
    }

    private IEnumerator ButtonUp()
    {
        if (buttonAnimationIndex >= buttonAnimation.Length - 1)
        {
            buttonAnimationIndex = buttonAnimation.Length - 2;
        }

        for (; buttonAnimationIndex >= 0; buttonAnimationIndex--)
        {
            buttonImage.sprite = buttonAnimation[buttonAnimationIndex];
            yield return buttonWait;
        }
        buttonCoroutine = null;
        ButtonReady = true;
    }
}
