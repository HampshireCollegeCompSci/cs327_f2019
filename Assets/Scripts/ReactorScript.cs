using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactorScript : MonoBehaviour
{
    //helloWorld
    public List<GameObject> cardList;
    public UtilsScript utils;
    int positionCounter;
    int cardMax;
    int ReactorVal;
    GameObject myPrefab;
    public string suit;

    private GUIStyle guiStyle;
    Vector3 position;
    Vector2 textSize;
    string GUItext;

    void Start()
    {
        //because typing is yucky :)
        utils = UtilsScript.global;
        ReactorVal = 0;
        guiStyle = new GUIStyle();
        guiStyle.fontSize = 20;
        UpdateGUI();
    }


    void Update()
    {
        return;
    }

    private void OnGUI()
    {
        position = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        Vector3 reactorPos = Camera.main.WorldToScreenPoint(gameObject.GetComponent<SpriteRenderer>().bounds.size);
        float reactorHeight = position.y - reactorPos.y;
        textSize = GUI.skin.label.CalcSize(new GUIContent(GUItext));
        GUI.Label(new Rect(position.x - 25, Screen.height - reactorPos.y + reactorHeight, textSize.x, textSize.y), GUItext, guiStyle);
    }

    private void UpdateGUI()
    {
        // Debug.Log("RS UpdateGUI");
        ReactorVal = CountReactorCard();
        GUItext = ReactorVal.ToString() + "/" + Config.config.maxReactorVal.ToString();
    }

    private void CheckGameOver()
    {
        // Debug.Log("RS CheckGameOver");
        if (CountReactorCard() >= Config.config.maxReactorVal)
        {
            myPrefab = (GameObject)Resources.Load("Prefabs/Explosion", typeof(GameObject));
            Instantiate(myPrefab, gameObject.transform.position, gameObject.transform.rotation);
            Destroy(gameObject);
        }
    }


    public void RemoveCard(GameObject card)
    {
        // Debug.Log("RS RemoveCard");
        cardList.Remove(card);
        UpdateGUI();
    }

    public void SetCardPositions()
    {
        // Debug.Log("RS SetCardPositions");
        positionCounter = 0;

        for (int indexCounter = cardList.Count - 1; indexCounter > -1; indexCounter--)
        {
            cardList[indexCounter].transform.position = gameObject.transform.position + new Vector3(0, Config.config.foundationStackDensity * positionCounter, -0.5f * positionCounter) + new Vector3(0, 0, -0.5f);

            positionCounter += 1;
        }

        UpdateGUI();
        CheckGameOver();
    }

    //this function is run on selected card's container
    //if click reactor then click other card,
    //click method gets run on container of first card clicked
    //know first card is from reactor
    //selectedCards = list of the currently selected cards
    //selectedCard[0] is the first card (from Reactor)
    //check if has more than 1 card -> shouldn't 
    //DON'T USE CLICKED CARD
    //take input (inputCard), which is the second card
    //match with them if they match


    //TODO: rename this goddamn function and all the other
    public void ProcessAction(GameObject input)
    {
        // Debug.Log("RS ProcessAction");
        GameObject card1 = utils.selectedCards[0];

        if (input.CompareTag("Card"))
        {
            //list needs to only be 1, something wrong if not -> skip to return
            if (utils.selectedCards.Count == 1)
            {
                if (utils.IsMatch(input, card1) && utils.selectedCards.Count == 1)
                {
                    GameObject inputContainer = input.GetComponent<CardScript>().container;

                    if (inputContainer.CompareTag("Foundation"))
                    {
                        if (inputContainer.GetComponent<FoundationScript>().cardList[0] == input)
                        {
                            utils.Match(input, utils.selectedCards[0]); //removes the two matched cards
                        }

                        return;
                    }

                    if (inputContainer.CompareTag("Reactor"))
                    {
                        if (inputContainer.GetComponent<ReactorScript>().cardList[0] == input)
                        {
                            utils.Match(input, utils.selectedCards[0]); //removes the two matched cards
                        }

                        return;
                    }

                    if (inputContainer.CompareTag("Wastepile"))
                    {
                        if (inputContainer.GetComponent<WastepileScript>().cardList[0] == input)
                        {
                            utils.Match(input, utils.selectedCards[0]); //removes the two matched cards
                        }

                        return;
                    }

                    else
                    {
                        utils.Match(input, utils.selectedCards[0]); //removes the two matched cards
                    }
                }

                else
                {
                    utils.DeselectCard(card1);
                }
            }
        }


        //this is just the return call to end after having clicked
        return;



    }


    //this is just meant to iterate through the list of cards in the stack
    //sum the amounts of them, and then return whatever that sum is
    //in order to be used in the update function
    //basically just in case it goes over 18, in which case end game
    private int CountReactorCard()
    {
        // Debug.Log("RS CountReactorCard");
        //sum the values into totalSum, return
        int totalSum = 0;
        int cardListVal = cardList.Count;
        for (int i = 0; i < cardListVal; i++)
        {
            totalSum += cardList[i].gameObject.GetComponent<CardScript>().cardVal;
        }

        return totalSum;
    }
}
