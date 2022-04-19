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

    // Singleton instance.
    public static ReactorScoreSetScript Instance = null;

    // Initialize the singleton instance.
    private void Awake()
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

    public void SetToDefault()
    {
        reactorScore0.text = "0/" + Config.Instance.maxReactorValMinus1;
        reactorScore1.text = "0/" + Config.Instance.maxReactorValMinus1;
        reactorScore2.text = "0/" + Config.Instance.maxReactorValMinus1;
        reactorScore3.text = "0/" + Config.Instance.maxReactorValMinus1;
        reactorScore0.color = Color.black;
        reactorScore1.color = Color.black;
        reactorScore2.color = Color.black;
        reactorScore3.color = Color.black;
    }

    public void SetReactorScore()
    {
        reactorScore0.text = reactor0.GetComponent<ReactorScript>().CountReactorCard() + "/" + Config.Instance.maxReactorValMinus1;
        reactorScore1.text = reactor1.GetComponent<ReactorScript>().CountReactorCard() + "/" + Config.Instance.maxReactorValMinus1;
        reactorScore2.text = reactor2.GetComponent<ReactorScript>().CountReactorCard() + "/" + Config.Instance.maxReactorValMinus1;
        reactorScore3.text = reactor3.GetComponent<ReactorScript>().CountReactorCard() + "/" + Config.Instance.maxReactorValMinus1;
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