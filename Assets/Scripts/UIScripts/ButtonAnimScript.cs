using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAnimScript : MonoBehaviour
{

    public void ButtonPressed()
    {
        gameObject.GetComponentInChildren<Animator>().enabled = true;
    }

    public void ButtonReleased()
    {
        gameObject.GetComponentInChildren<Animator>().enabled = false;
    }
}
