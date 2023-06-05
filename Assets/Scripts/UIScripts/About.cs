using UnityEngine;

public class About : MonoBehaviour
{
    public void OpenProjectWebsite()
    {
        Debug.Log("opening project website");
        Application.OpenURL(Constants.projectWebsite);
    }

    public void Max()
    {
        if (Config.Instance.prettyColors)
        {
            Debug.Log("max mode already activated");
            SoundEffectsController.Instance.ExplosionSound();
        }
        else
        {
            Debug.Log("activating max mode");
            Config.Instance.prettyColors = true;
            SoundEffectsController.Instance.PauseMenuButtonSound();
        }
    }
}
