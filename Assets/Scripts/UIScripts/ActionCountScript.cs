using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ActionCountScript : MonoBehaviour
{
    public Text actionText;
    public GameObject timerSiren;
    public GameObject timerButton;
    private Image buttonImage;
    private Image sirenImage;
    public Sprite sirenOff, sirenOn, sirenAlert; 
    public Sprite buttonDown, buttonUp;
    private byte currentState;
    private Coroutine flasher;

    private void Start()
    {
        UpdateActionText();
        buttonImage = timerButton.GetComponent<Image>();
        sirenImage = timerSiren.GetComponent<Image>();
        currentState = 0;
        flasher = null;
    }

    public void UpdateActionText()
    {
        actionText.text = (Config.config.actionMax - Config.config.actions).ToString() + "/" + Config.config.actionMax;
    }

    public void PressKnob()
    {
        Vibration.Vibrate(Config.config.buttonVibration);
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
        if (flasher != null)
        {
            StopCoroutine(flasher);
        }

        sirenImage.sprite = sirenOff;
        currentState = 0;
        flasher = null;
    }

    public bool TurnSirenOn()
    {
        if (currentState == 1)
        {
            return false;
        }
        else if (currentState == 2)
        {
            StopCoroutine(flasher);
        }

        currentState = 1;
        sirenImage.sprite = sirenOn;
        flasher = StartCoroutine(Flash());
        return true;
    }

    public bool TurnAlertOn()
    {
        if (currentState == 2)
        {
            return false;
        }
        else if (currentState == 1)
        {
            StopCoroutine(flasher);
        }

        currentState = 2;
        sirenImage.sprite = sirenAlert;
        flasher = StartCoroutine(Flash());
        return true;
    }

    IEnumerator Flash()
    {
        Sprite currentSprite = sirenImage.sprite;
        while (true)
        {
            yield return new WaitForSeconds(1);
            sirenImage.sprite = sirenOff;
            yield return new WaitForSeconds(1);
            sirenImage.sprite = currentSprite;
        }
    }
}
