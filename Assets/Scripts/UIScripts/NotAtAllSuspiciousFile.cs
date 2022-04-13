using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotAtAllSuspiciousFile : MonoBehaviour
{
    public void Max()
    {
        Config.Instance.prettyColors = !Config.Instance.prettyColors;
        SoundEffectsController.Instance.PauseMenuButtonSound();
    }
}
