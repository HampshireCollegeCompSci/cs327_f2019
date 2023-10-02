using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ActionCountScript : MonoBehaviour
{
    // Singleton instance.
    public static ActionCountScript Instance;
    private static readonly WaitForSeconds textDelay = new(0.05f);

    [SerializeField]
    private Text actionText;
    [SerializeField]
    private Image screenImage, lightsImage, sirenImage, buttonImage;

    [SerializeField]
    private Sprite buttonDown, buttonUp;

    private Color originalScreenColor, originalLightsColor, originalSirenColor;
    private HighLightColor _currentAlertLevel;

    private Coroutine actionCoroutine;
    private Coroutine flasherCoroutine;

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

    private void Start()
    {
        originalScreenColor = screenImage.color;
        originalLightsColor = lightsImage.color;
        originalSirenColor = sirenImage.color;
        _currentAlertLevel = GameValues.Colors.normal;
        flasherCoroutine = null;
    }

    public HighLightColor AlertLevel
    {
        get => _currentAlertLevel;
        set
        {
            if (_currentAlertLevel.Equals(value)) return;
            _currentAlertLevel = value;

            if (flasherCoroutine != null)
            {
                StopCoroutine(flasherCoroutine);
                flasherCoroutine = null;
                sirenImage.color = originalSirenColor;
            }

            if (value.ColorLevel != Constants.ColorLevel.None)
            {
                // when hints are disabled do not show high alerts
                if (!Config.Instance.HintsEnabled && value.ColorLevel == Constants.ColorLevel.Over)
                {
                    value = Config.Instance.CurrentColorMode.Move;
                }
                flasherCoroutine = StartCoroutine(Flash(value.Color));
                screenImage.color = value.ScreenColor;
                lightsImage.color = value.Color;
            }
            else
            {
                screenImage.color = originalScreenColor;
                lightsImage.color = originalLightsColor;
            }
        }
    }

    public void UpdateActionText(string setTo = null)
    {
        if (setTo == null)
        {
            int newValue = Config.Instance.CurrentDifficulty.MoveLimit - Actions.ActionsDone;
            if (int.TryParse(actionText.text, out int oldValue) &&
                oldValue + 1 < newValue)
            {
                actionCoroutine = StartCoroutine(AddToActionText(oldValue, newValue));
            }
            else
            {
                if (actionCoroutine != null)
                {
                    StopCoroutine(actionCoroutine);
                    actionCoroutine = null;
                }

                actionText.text = newValue.ToString();
            }
        }
        else
        {
            actionText.text = setTo;
        }
    }

    private IEnumerator AddToActionText(int currentValue, int newValue)
    {
        while (currentValue < newValue)
        {
            yield return textDelay;
            currentValue++;
            actionText.text = currentValue.ToString();
        }

        // just in case quick moves or undos occur
        actionText.text = (Config.Instance.CurrentDifficulty.MoveLimit - Actions.ActionsDone).ToString();
    }

    public void KnobDown()
    {
        buttonImage.sprite = buttonDown;
    }

    public void KnobUp()
    {
        buttonImage.sprite = buttonUp;
    }

    private IEnumerator Flash(Color flashColor)
    {
        FadeColorPair alertFadeIn = new(originalSirenColor, flashColor);
        FadeColorPair alertFadeOut = new(flashColor, originalSirenColor);

        while (true)
        {
            yield return Animate.FadeImage(sirenImage, alertFadeIn, GameValues.AnimationDurataions.alertFade);
            yield return Animate.FadeImage(sirenImage, alertFadeOut, GameValues.AnimationDurataions.alertFade);
        }
    }
}
