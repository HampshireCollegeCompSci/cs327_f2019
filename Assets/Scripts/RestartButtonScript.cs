using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartButtonScript : MonoBehaviour
{
    public void ProcessAction(GameObject input)
    {
        Application.LoadLevel(0);
        Debug.Log("hit button");
        return;
    }
}
