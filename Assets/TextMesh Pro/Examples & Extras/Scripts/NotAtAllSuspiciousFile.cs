using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotAtAllSuspiciousFile : MonoBehaviour
{
    public void Max()
    {
        Config.config.prettyColors = !Config.config.prettyColors;
    }
}
