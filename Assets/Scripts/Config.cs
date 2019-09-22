using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config : MonoBehaviour
{
    public static Config config; //Creates a new instance if one does not yet exist

    //Variables go here
    public Stack<Move> moveLog = new Stack<Move>();

    //foundations
    public GameObject foundation1;
    public GameObject foundation2;
    //public GameObject foundation3;
    //public GameObject foundation4;

    public GameObject[] foundationList;

    int foundationCount = 0;
    void Awake()
    {
        if (config == null)
        {
            DontDestroyOnLoad(gameObject); //makes instance persist across scenes
            config = this;
        }
        else if (config != this)
        {
            Destroy(gameObject); //deletes copies of global which do not need to exist, so right version is used to get info from
        }
    }

    private void Start()
    {
        foundationList = new GameObject[] { foundation1, foundation2/*, foundation3, foundation4*/ };
    }

    private void Update()
    {
        //every frame, check to see if all foundations are empty.
        foreach (GameObject foundation in foundationList)
        {  
            if (foundation.GetComponent<FoundationScript>().cardList.Count == 0)
            {
                foundationCount++;
            }
        }

        if (foundationCount == foundationList.Length)
        {
            Application.Quit();
        }
        else
        {
            foundationCount = 0;
        }
    }
}
