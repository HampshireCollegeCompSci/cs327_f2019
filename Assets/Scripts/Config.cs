using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config : MonoBehaviour
{
    public static Config config; //Creates a new instance if one does not yet exist

    //Variables go here
    public Stack<Move> moveLog = new Stack<Move>();

    //card scale
    public Vector3 cardScale;


    //foundations
    public float foundationStackDensity;

    //wastepile
    public bool onlyShowTopWastepileCards = true;
    public int cardsAtTopOfWastePile = 3;

    public GameObject foundation1;
    public GameObject foundation2;
    public GameObject foundation3;
    public GameObject foundation4;

    private GameObject[] foundationList;

    public GameObject wastePile;
    public GameObject deck;

    private int foundationCount = 0;
    private void Awake()
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
        SetCards();
    }

    public void SetCards()
    {
        foundation1 = GameObject.Find("Foundation (0)");
        foundation2 = GameObject.Find("Foundation (1)");
        foundation3 = GameObject.Find("Foundation (2)");
        foundation4 = GameObject.Find("Foundation (3)");
        wastePile = GameObject.Find("WastePile");
        deck = GameObject.Find("Deck");
        foundationList = new GameObject[] { foundation1, foundation2, foundation3, foundation4 };
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

        if (foundationCount == foundationList.Length 
            && deck.GetComponent<DeckScript>().cardList.Count == 0 
            && wastePile.GetComponent<WastepileScript>().cardList.Count == 0)
        {
            Application.Quit();
        }
        else
        {
            foundationCount = 0;
        }
    }
}
