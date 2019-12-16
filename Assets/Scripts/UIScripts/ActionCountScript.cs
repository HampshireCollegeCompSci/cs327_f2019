using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionCountScript : MonoBehaviour
{
    public Text actionText;
    public GameObject timerSiren;
    private Image siren;
    public Sprite sirenOff, sirenOn, sirenAlert; 
    public Sprite buttonDown, buttonUp;
    private byte currentState;
    private Coroutine flasher;

    private void Start()
    {
        UpdateActionText();
        siren = timerSiren.GetComponent<Image>();
        currentState = 0;
        flasher = null;
    }

    public void UpdateActionText()
    {
        actionText.text = (Config.config.actionMax - Config.config.actions).ToString();
    }

    public void PressKnob()
    {
        GameObject.Find("TimerButton").GetComponent<Image>().sprite = buttonDown;
        StartCoroutine("ButtonAnimTrans");

    }

    IEnumerator ButtonAnimTrans()
    {
        yield return new WaitForSeconds(0.3f);
        GameObject.Find("TimerButton").GetComponent<Image>().sprite = buttonUp;
    }

    public void TurnSirenOff()
    {
        StopCoroutine(flasher);
        siren.sprite = sirenOff;
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
        siren.sprite = sirenOn;
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
        siren.sprite = sirenAlert;
        flasher = StartCoroutine(Flash());
        return true;
    }

    IEnumerator Flash()
    {
        Sprite currentSprite = siren.sprite;
        while (true)
        {
            yield return new WaitForSeconds(1);
            siren.sprite = sirenOff;
            yield return new WaitForSeconds(1);
            siren.sprite = currentSprite;
        }
    }
}
