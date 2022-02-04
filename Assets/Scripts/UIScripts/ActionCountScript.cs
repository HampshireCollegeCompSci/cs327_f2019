using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ActionCountScript : MonoBehaviour
{
    public Text actionText;
    public GameObject screen;
    public GameObject timerSiren;
    public GameObject timerButton;
    private Image screenImage;
    private Image gameTimerImage;
    private Image buttonImage;
    private Image sirenImage;
    public Sprite sirenOff, sirenOn, sirenAlert;
    public Sprite gameTimerOff, gameTimerOn, gameTimerAlert;
    public Sprite screenOn, screenAlert;
    public Sprite buttonDown, buttonUp;
    private byte currentState;
    //private Coroutine fader;
    private Coroutine flasher;

    private void Start()
    {
        screenImage = screen.GetComponent<Image>();
        gameTimerImage = gameObject.GetComponent<Image>();
        buttonImage = timerButton.GetComponent<Image>();
        sirenImage = timerSiren.GetComponent<Image>();
        currentState = 0;
        flasher = null;
    }

    public void UpdateActionText(string setTo = null)
    {
        if (setTo == null)
        {
            actionText.text = (Config.config.actionMax - Config.config.actions).ToString();
        }
        else
        {
            actionText.text = setTo;
        }
    }

    public void PressKnob()
    {
        SoundEffectsController.Instance.VibrateMedium();
        buttonImage.sprite = buttonDown;
        StartCoroutine(ButtonAnimTrans());
    }

    IEnumerator ButtonAnimTrans()
    {
        yield return new WaitForSeconds(0.3f);
        buttonImage.sprite = buttonUp;
    }

    public void TurnSirenOff()
    {
        //if (fader != null)
            //StopCoroutine(fader);
        if (flasher != null)
            StopCoroutine(flasher);

        screen.SetActive(false);
        gameTimerImage.sprite = gameTimerOff;
        sirenImage.sprite = sirenOff;
        currentState = 0;
        //fader = null;
        flasher = null;
    }

    public bool TurnSirenOn(byte alertLevel)
    {
        if (currentState == alertLevel)
            return false;

        //if (fader != null)
            //StopCoroutine(fader);
        if (flasher != null)
            StopCoroutine(flasher);

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
            gameTimerImage.sprite = gameTimerAlert ;
            sirenImage.sprite = sirenAlert;
        }

        //fader = StartCoroutine(ScreenFade());
        flasher = StartCoroutine(Flash());
        return true;
    }

    IEnumerator ScreenFade()
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

    IEnumerator Flash()
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
