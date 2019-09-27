using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartButtonScript : MonoBehaviour
{
	public GameObject config;

    void Start()
	{
		config = GameObject.Find("Config");
		config.GetComponent<Config>().SetCards();
	}

    public void ProcessAction(GameObject input)
    {
        Application.LoadLevel(0);//resets the level
        Debug.Log("hit button");
        return;
    }
}
