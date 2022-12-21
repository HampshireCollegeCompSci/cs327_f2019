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
    private AlertLevel currentAlertLevel;

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
        currentAlertLevel = GameValues.AlertLevels.none;
        flasherCoroutine = null;
    }

    public void UpdateActionText(string setTo = null)
    {
        if (setTo == null)
        {
            int newValue = Config.Instance.CurrentDifficulty.MoveLimit - Config.Instance.actions;
            int oldValue;
            if (int.TryParse(actionText.text, out oldValue) &&
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

    public void TurnSirenOff()
    {
        TryStopFlashing();
        screenImage.color = originalScreenColor;
        lightsImage.color = GameValues.AlertLevels.none.lightColor;
        currentAlertLevel = GameValues.AlertLevels.none;
    }

    public bool TurnSirenOn(AlertLevel newAlertLevel)
    {
        if (currentAlertLevel.Equals(newAlertLevel)) return false;

        currentAlertLevel = newAlertLevel;
        TryStopFlashing();

        screenImage.color = currentAlertLevel.screenColor;
        lightsImage.color = currentAlertLevel.lightColor;

        flasherCoroutine = StartCoroutine(Flash(currentAlertLevel));
        return true;
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

    public void PressKnob()
    {
        if (UtilsScript.Instance.InputStopped) return;
        // the make actions max button calls this
        SoundEffectsController.Instance.VibrateMedium();
        buttonImage.sprite = buttonDown;
        StartCoroutine(ButtonAnimTrans());
    }

    private IEnumerator ButtonAnimTrans()
    {
        yield return new WaitForSeconds(0.3f);
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
        float timeElapsed = 0;
        float duration = 1.5f;

        bool turnOn = true;
        Color startColor = GameValues.AlertLevels.none.lightColor;
        Color endColor = alertLevel.lightColor;

        while (true)
        {
            while (timeElapsed < duration)
            {
                sirenImage.color = Color.Lerp(startColor, endColor, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            timeElapsed = 0;
            startColor = sirenImage.color;
            endColor = turnOn ? GameValues.AlertLevels.none.lightColor : alertLevel.lightColor;
            turnOn = !turnOn;
        }
    }
}
