using UnityEngine;
using UnityEngine.UI;

public class About : MonoBehaviour
{
    public Text info;

    private void Start()
    {
        info.text =
            $"Game Version: {Application.version}\n" +
            $"Unity Version: {Application.unityVersion}\n" +
            $"Device Vibration: {Vibration.HasVibrator}\n" +
            $"Save File Path: {SaveFile.SaveFilePath}";
    }

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
