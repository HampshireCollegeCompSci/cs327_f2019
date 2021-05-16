using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsScript : MonoBehaviour
{
    public GameObject effectsVolumeText;
    public GameObject musicVolumeText;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("effectsVolume"))
        {

        }
    }

    public void EffectsVolumeChange(float update)
    {
        PlayerPrefs.SetFloat("effectsVolume", update);
    }

    public void MusicVolumeChange(float update)
    {
        PlayerPrefs.SetFloat("musicVolume", update);
    }

    public void VibrationOnToggle(bool update)
    {
        PlayerPrefs.SetString("vibrationEnabled", update.ToString());
    }
}
