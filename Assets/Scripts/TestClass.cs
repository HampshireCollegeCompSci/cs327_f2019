using System;
using UnityEngine;
public class TestClass: MonoBehaviour
{

    string JSONToTest = "{\"reactorLimit\":[1,2,3,40],\"startingStack\":[1,1,30,60],\"cardsToDeal\":1}";
    GameInfo gameInfo = new GameInfo();
    public GameInfo ConvertJSON(string JSON)
    {
        return GameInfo.CreateFromJSON(JSON);
    }
    public void TestingJSON()
    {
        GameInfo newInfo = ConvertJSON(JSONToTest);
        print(newInfo.reactorLimit[0] + " Should Equal 1");
        print(newInfo.reactorLimit[1] + " Should Equal 2");
        print(newInfo.reactorLimit[2] + " Should Equal 3");
        print(newInfo.reactorLimit[3] + " Should Equal 40");

        print(newInfo.startingStack[0] + " Should Equal 1");
        print(newInfo.startingStack[1] + " Should Equal 1");
        print(newInfo.startingStack[2] + " Should Equal 30");
        print(newInfo.startingStack[3] + " Should Equal 6");

        print(newInfo.cardsToDeal + " Should Equal 1");
    }

    void Start()
    {
        TestingJSON();

    }


}
