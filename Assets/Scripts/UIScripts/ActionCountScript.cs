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
    private GameObject screen, timerSiren, timerButton;

    [SerializeField]
    private Sprite sirenOff, sirenOn, sirenAlert,
        gameTimerOff, gameTimerOn, gameTimerAlert,
        screenOn, screenAlert,
        buttonDown, buttonUp;

    private Image screenImage, gameTimerImage, buttonImage, sirenImage;

    private byte currentState;

    private Coroutine actionCoroutine;
    //private Coroutine fader;
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
        screenImage = screen.GetComponent<Image>();
        gameTimerImage = gameObject.GetComponent<Image>();
        buttonImage = timerButton.GetComponent<Image>();
        sirenImage = timerSiren.GetComponent<Image>();
        currentState = 0;
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
        //if (fader != null)
        //{
        //  StopCoroutine(fader);
        //}
        if (flasherCoroutine != null)
        {
            StopCoroutine(flasherCoroutine);
        }

        screen.SetActive(false);
        gameTimerImage.sprite = gameTimerOff;
        sirenImage.sprite = sirenOff;
        currentState = 0;
        //fader = null;
        flasherCoroutine = null;
    }

    public bool TurnSirenOn(byte alertLevel)
    {
        if (currentState == alertLevel)
        {
            return false;
        }

        //if (fader != null)
        //{
        //StopCoroutine(fader);
        //}
        if (flasherCoroutine != null)
        {
            StopCoroutine(flasherCoroutine);
        }

        currentState = alertLevel;

        screen.SetActive(true);
        if (alertLevel == 1)
        {
            screenImage.sprite = screenOn;
            gameTimerImage.sprite = gameTimerOn;
            sirenImage.sprite = sirenOn;
        }
        else
        {
            screenImage.sprite = screenAlert;
            gameTimerImage.sprite = gameTimerAlert;
            sirenImage.sprite = sirenAlert;
        }

        //fader = StartCoroutine(ScreenFade());
        flasherCoroutine = StartCoroutine(Flash());
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

    [SerializeField]
    private void PressKnob()
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

    private IEnumerator ScreenFade()
    {
        screenImage.color = Color.white;
        Color screenColor = Color.white;
        while (true)
        {
            while (screenColor.a > 0)
            {
                screenColor.a -= 0.02f;
                screenImage.color = screenColor;
                yield return new WaitForSeconds(0.05f);
            }

            while (screenColor.a < 1)
            {
                screenColor.a += 0.02f;
                screenImage.color = screenColor;
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    private IEnumerator Flash()
    {
        Sprite currentSiren = sirenImage.sprite;
        while (true)
        {
            yield return new WaitForSeconds(1);
            sirenImage.sprite = sirenOff;
            yield return new WaitForSeconds(1);
            sirenImage.sprite = currentSiren;
        }
    }
}
