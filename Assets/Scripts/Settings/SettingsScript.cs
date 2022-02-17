﻿using UnityEngine;
using UnityEngine.UI;

public class SettingsScript : MonoBehaviour
{
    public GameObject musicSlider;
    public Text musicVolumeText;

    public GameObject soundEffectsSlider;
    public Text soundEffectsVolumeText;

    public GameObject vibrationToggle;
    public GameObject foodSuitsToggle;

    public GameObject confirmObject;

    private bool lockout;

    // Start is called before the first frame update
    void Start()
    {
        lockout = true;

        PlayerPrefKeys.CheckKeys();

        // music volume
        int volume = (int)(PlayerPrefs.GetFloat(Constants.musicVolumeKey) * 10);
        musicSlider.GetComponent<Slider>().value = volume;
        musicVolumeText.text = volume.ToString() + "0%";

        // sound effects volume
        volume = (int)(PlayerPrefs.GetFloat(Constants.soundEffectsVolumeKey) * 10);
        soundEffectsSlider.GetComponent<Slider>().value = volume;
        soundEffectsVolumeText.text = volume.ToString() + "0%";

        bool isOn;
        // vibration enabled
        if (System.Boolean.TryParse(PlayerPrefs.GetString(Constants.vibrationEnabledKey), out isOn))
        {
            vibrationToggle.GetComponent<Toggle>().isOn = isOn;
        }
        else
        {
            // unable to parse
            vibrationToggle.GetComponent<Toggle>().isOn = false;
        }

        // food suits enabled
        if (System.Boolean.TryParse(PlayerPrefs.GetString(Constants.foodSuitsEnabledKey), out isOn))
        {
            foodSuitsToggle.GetComponent<Toggle>().isOn = isOn;
        }
        else
        {
            // unable to parse
            foodSuitsToggle.GetComponent<Toggle>().isOn = false;
        }

        lockout = false;
    }

    public void MusicVolumeChange(float update)
    {
        if (lockout)
            return;

        musicVolumeText.text = update.ToString() + "0%";
        update /= 10;
        PlayerPrefs.SetFloat(Constants.musicVolumeKey, update);
        MusicController.Instance.UpdateMaxVolume(update);
    }

    public void SoundEffectsVolumeChange(float update)
    {
        if (lockout)
            return;

        soundEffectsVolumeText.text = update.ToString() + "0%";
        update /= 10;
        PlayerPrefs.SetFloat(Constants.soundEffectsVolumeKey, update);
        SoundEffectsController.Instance.UpdateMaxVolume(update);
        SoundEffectsController.Instance.ButtonPressSound();
    }

    public void VibrationEnabledOnToggle(bool update)
    {
        if (lockout)
            return;

        PlayerPrefs.SetString(Constants.vibrationEnabledKey, update.ToString());
        SoundEffectsController.Instance.UpdateVibration(update);
        SoundEffectsController.Instance.ButtonPressSound();
    }

    public void FoodSuitsEnabledOnToggle(bool update)
    {
        if (lockout)
            return;

        PlayerPrefs.SetString(Constants.foodSuitsEnabledKey, update.ToString());
        SoundEffectsController.Instance.ButtonPressSound();
    }

    public void TryToClearRecordsButton()
    {
        if (lockout)
            return;

        SoundEffectsController.Instance.ButtonPressSound();
        confirmObject.SetActive(true);
    }

    public void ClearRecordsConfirmationButton(bool confirm)
    {
        if (confirm)
        {
            Debug.Log("clearing saved records");

            // since ResultsScript.cs detects and auto fills the very first records this is how it must be done
            foreach (string difficulty in Config.GameValues.difficulties)
            {
                PlayerPrefs.DeleteKey(PlayerPrefKeys.GetHighScoreKey(difficulty));
                PlayerPrefs.DeleteKey(PlayerPrefKeys.GetLeastMovesKey(difficulty));
            }
        }

        SoundEffectsController.Instance.ButtonPressSound();
        confirmObject.SetActive(false);
    }
}