using UnityEngine;

public class NotAtAllSuspiciousFile : MonoBehaviour
{
    public void Max()
    {
        if (Config.Instance.prettyColors)
        {
            SoundEffectsController.Instance.ExplosionSound();
        }
        else
        {
            Config.Instance.prettyColors = true;
            SoundEffectsController.Instance.PauseMenuButtonSound();
        }
    }
}
