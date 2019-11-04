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

    private int reactor0Score;

    private void Start()
    {
        SetReactorScore();
    }

    //private void OnGUI()
    //{
    //    GUI.Label(new Rect(Camera.main.WorldToScreenPoint(reactor0.transform.position).x,
    //        Screen.height - Camera.main.WorldToScreenPoint(reactor0.transform.position).y, 100, 20),
    //        reactor0.GetComponent<ReactorScript>().CountReactorCard() + "/" + Config.config.maxReactorVal);
    //}

    public void SetReactorScore()
    {

        reactorScore0.text = reactor0.GetComponent<ReactorScript>().CountReactorCard() + "/" + Config.config.maxReactorVal;
        reactorScore1.text = reactor1.GetComponent<ReactorScript>().CountReactorCard() + "/" + Config.config.maxReactorVal;
        reactorScore2.text = reactor2.GetComponent<ReactorScript>().CountReactorCard() + "/" + Config.config.maxReactorVal;
        reactorScore3.text = reactor3.GetComponent<ReactorScript>().CountReactorCard() + "/" + Config.config.maxReactorVal;
    }

    void OnGUI()
    {
        reactorScore0.transform.position = Camera.main.WorldToScreenPoint(reactor0.transform.position);
        reactorScore1.transform.position = Camera.main.WorldToScreenPoint(reactor1.transform.position);
        reactorScore2.transform.position = Camera.main.WorldToScreenPoint(reactor2.transform.position);
        reactorScore3.transform.position = Camera.main.WorldToScreenPoint(reactor3.transform.position);
    }

}
