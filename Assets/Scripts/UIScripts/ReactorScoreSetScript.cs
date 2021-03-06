﻿using UnityEngine;
using UnityEngine.UI;

public class ReactorScoreSetScript : MonoBehaviour
{
    private void Start()
    {
        SetReactorScore();
    }

    public GameObject reactor0;
    public Text reactorScore0;

    public GameObject reactor1;
    public Text reactorScore1;

    public GameObject reactor2;
    public Text reactorScore2;

    public GameObject reactor3;
    public Text reactorScore3;

    public void SetReactorScore()
    {
        reactorScore0.text = reactor0.GetComponent<ReactorScript>().CountReactorCard() + "/" + Config.config.maxReactorVal;
        reactorScore1.text = reactor1.GetComponent<ReactorScript>().CountReactorCard() + "/" + Config.config.maxReactorVal;
        reactorScore2.text = reactor2.GetComponent<ReactorScript>().CountReactorCard() + "/" + Config.config.maxReactorVal;
        reactorScore3.text = reactor3.GetComponent<ReactorScript>().CountReactorCard() + "/" + Config.config.maxReactorVal;
    }

    public void ChangeTextColor(GameObject reactor, bool on)
    {
        if (reactor == reactor0)
            if (on)
                reactorScore0.color = Color.red;
            else
                reactorScore0.color = Color.black;
        else if (reactor == reactor1)
            if (on)
                reactorScore1.color = Color.red;
            else
                reactorScore1.color = Color.black;
        else if (reactor == reactor2)
            if (on)
                reactorScore2.color = Color.red;
            else
                reactorScore2.color = Color.black;
        else
            if (on)
                reactorScore3.color = Color.red;
            else
                reactorScore3.color = Color.black;
    }
}