using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ActionCountScript : MonoBehaviour
{
    // Singleton instance.
    public static ActionCountScript Instance;

    [SerializeField]
    private Text actionText;
    [SerializeField]
    private Image screenImage, lightsImage, sirenImage, buttonImage;

    [SerializeField]
    private Sprite buttonDown, buttonUp;

    private Color originalScreenColor;
    private AlertLevel _currentAlertLevel;

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
        _currentAlertLevel = GameValues.AlertLevels.none;
        flasherCoroutine = null;
    }

    public AlertLevel AlertLevel
    {
        get => _currentAlertLevel;
        set
        {
            if (_currentAlertLevel.Equals(value)) return;
            _currentAlertLevel = value;
            TryStopFlashing();

            if (!value.Equals(GameValues.AlertLevels.none))
            {
                // when hints are disabled do not show high alerts
                if (!Config.Instance.HintsEnabled && value.Equals(GameValues.AlertLevels.high))
                {
                    value = GameValues.AlertLevels.low;
                }
                flasherCoroutine = StartCoroutine(Flash(value));
            }
            screenImage.color = value.screenColor;
            lightsImage.color = value.lightColor;
        }
    }

    public void UpdateActionText(string setTo = null)
    {
        if (setTo == null)
        {
            int newValue = Config.Instance.CurrentDifficulty.MoveLimit - Config.Instance.actions;
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
            yield return new WaitForSeconds(0.05f);
            currentValue++;
            actionText.text = currentValue.ToString();
        }

        // just in case quick moves or undos occur
        actionText.text = (Config.Instance.CurrentDifficulty.MoveLimit - Config.Instance.actions).ToString();
    }

    public void KnobDown()
    {
        buttonImage.sprite = buttonDown;
    }

    public void KnobUp()
    {
        buttonImage.sprite = buttonUp;
    }

    private void TryStopFlashing()
    {
        if (flasherCoroutine != null)
        {
            StopCoroutine(flasherCoroutine);
            flasherCoroutine = null;
            sirenImage.color = GameValues.AlertLevels.none.lightColor;
        }
    }

    private IEnumerator Flash(AlertLevel alertLevel)
    {
        FadeColorPair alertFadeIn = new(GameValues.AlertLevels.none.lightColor, alertLevel.lightColor);
        FadeColorPair alertFadeOut = new(alertLevel.lightColor, GameValues.AlertLevels.none.lightColor);

        while (true)
        {
            yield return Animate.FadeImage(sirenImage, alertFadeIn, GameValues.AnimationDurataions.alertFade);
            yield return Animate.FadeImage(sirenImage, alertFadeOut, GameValues.AnimationDurataions.alertFade);
        }
    }
}
