using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionCountScript : MonoBehaviour
{
    public Text actionText;
    public Sprite buttonDown, buttonUp;


    private void Start()
    {
        Config.config.actions = -1; //actions needs to be set to 0, there's 1 deal at te start of the game which adds 1 to actions so I set it to -1 at the start
    }
    void Update()
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

        for (float ft = 0.2f; ft >= 0; ft -= 0.1f)
        {
            yield return new WaitForSeconds(0.1f);
        }

        GameObject.Find("TimerButton").GetComponent<Image>().sprite = buttonUp;

    }
}
