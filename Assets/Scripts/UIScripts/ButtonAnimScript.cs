using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAnimScript : MonoBehaviour
{
    public GameObject pressAnimation;

    public void ButtonPressed()
    {
        pressAnimation.SetActive(true);
    }

    public void ButtonReleased()
    {
        pressAnimation.SetActive(false);
    }
}
