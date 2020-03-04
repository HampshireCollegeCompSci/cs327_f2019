using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinLoseScript : MonoBehaviour
{
    public GameObject spaceBaby;

    void Start()
    {
        if (true)
        {
            //gameObject.GetComponent<Image>().enabled = true;
            spaceBaby.GetComponent<SpaceBabyController>().BabyWin(0);
        }
    }
}
