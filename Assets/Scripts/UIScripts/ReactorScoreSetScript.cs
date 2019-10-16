using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReactorScoreSetScript : MonoBehaviour
{
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
        reactorScore0.text = reactor0.GetComponent<ReactorScript>().CountReactorCard() + "/18";
        reactorScore1.text = reactor1.GetComponent<ReactorScript>().CountReactorCard() + "/18";
        reactorScore2.text = reactor2.GetComponent<ReactorScript>().CountReactorCard() + "/18";
        reactorScore3.text = reactor3.GetComponent<ReactorScript>().CountReactorCard() + "/18";
    }
}
