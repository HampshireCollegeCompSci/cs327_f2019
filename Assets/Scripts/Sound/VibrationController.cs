using System.Collections;
using UnityEngine;

public class VibrationController : MonoBehaviour
{
    // Singleton instance.
    public static VibrationController Instance { get; private set; }
    
    // Initialize the singleton instance.
    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    public void VibrateSmall()
    {
        if (PersistentSettings.VibrationEnabled) Vibration.VibratePop();
    }

    public void VibrateMedium()
    {
        if (PersistentSettings.VibrationEnabled) Vibration.VibratePeek();
    }

    public void VibrateLarge()
    {
        if (PersistentSettings.VibrationEnabled) Vibration.Vibrate();
    }
}
