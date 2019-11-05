using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    void OnGUI()
    {
        if (reactorScore0 != null && reactorScore1 != null && reactorScore2 != null && reactorScore3 != null
            && reactor0 != null && reactor1 != null && reactor2 != null && reactor3 != null)
        {
            reactorScore0.transform.position = Camera.main.WorldToScreenPoint(reactor0.transform.position) - new Vector3(0, 140, 0);
            reactorScore1.transform.position = Camera.main.WorldToScreenPoint(reactor1.transform.position) - new Vector3(0, 140, 0);
            reactorScore2.transform.position = Camera.main.WorldToScreenPoint(reactor2.transform.position) - new Vector3(0, 140, 0);
            reactorScore3.transform.position = Camera.main.WorldToScreenPoint(reactor3.transform.position) - new Vector3(0, 140, 0);
        }
    }


}