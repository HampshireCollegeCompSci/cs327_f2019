using System;
using UnityEngine;
public class TestClass : MonoBehaviour
{

    string JSONToTest;
    GameInfo gameInfo = new GameInfo();

    public GameInfo ConvertJSON(string JSON)
    {
        return GameInfo.CreateFromJSON(JSON);
    }
    public void TestingJSON()
    {
        string path = "Assets/GameConfigurations/gameValues.json";

        JSONToTest = gameInfo.WriteString(path);

        print(JSONToTest);

        GameInfo newInfo = ConvertJSON(JSONToTest);
        print(newInfo.reactorLimit[0] + " Should Equal 1");
        print(newInfo.reactorLimit[1] + " Should Equal 2");
        print(newInfo.reactorLimit[2] + " Should Equal 3");
        print(newInfo.reactorLimit[3] + " Should Equal 40");

        print(newInfo.foundationStartingSize[0] + " Should Equal 1");
        print(newInfo.foundationStartingSize[1] + " Should Equal 1");
        print(newInfo.foundationStartingSize[2] + " Should Equal 30");
        print(newInfo.foundationStartingSize[3] + " Should Equal 60");

        print(newInfo.cardsToWastePilePerClick + " Should Equal 1");
    }

    void Start()
    {
        TestingJSON();
    }


}
