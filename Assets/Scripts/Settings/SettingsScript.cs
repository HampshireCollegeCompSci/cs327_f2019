using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsScript : MonoBehaviour
{
    [SerializeField]
    private Slider musicSlider, soundEffectsSlider;
    [SerializeField]
    private Text musicVolumeText, soundEffectsVolumeText;

    [SerializeField]
    private Toggle vibrationToggle, foodSuitsToggle;

    [SerializeField]
    private Slider frameRateSlider;
    [SerializeField]
    private Text frameRateText;
    private List<int> frameRates;

    [SerializeField]
    private GameObject frameRateInfoObject, suitArtNoticeObject;

    [SerializeField]
    private GameObject confirmObject;
    [SerializeField]
    private Button confirmYesButton;

    private bool lockout;

    private int musicMultiplier;
    private int soundEffectsMultiplier;

    // Start is called before the first frame update
    void Start()
    {
        lockout = true;

        PlayerPrefKeys.CheckKeys();

        // music volume
        musicSlider.maxValue = Constants.musicVolumeDenominator;
        musicMultiplier = 100 / Constants.musicVolumeDenominator;
        int volume = PlayerPrefs.GetInt(Constants.musicVolumeKey);
        musicSlider.value = volume;
        musicVolumeText.text = $"{volume * musicMultiplier}%";

        // sound effects volume
        soundEffectsSlider.maxValue = Constants.soundEffectsVolumeDenominator;
        soundEffectsMultiplier = 100 / Constants.soundEffectsVolumeDenominator;
        volume = PlayerPrefs.GetInt(Constants.soundEffectsVolumeKey);
        soundEffectsSlider.value = volume;
        soundEffectsVolumeText.text = $"{volume * soundEffectsMultiplier}%";

        // boolean settings
        bool isOn;

        if (Vibration.HasVibrator())
        {
            // vibration enabled
            if (bool.TryParse(PlayerPrefs.GetString(Constants.vibrationEnabledKey), out isOn))
            {
                vibrationToggle.isOn = isOn;
            }
            else
            {
                // unable to parse
                PlayerPrefs.SetString(Constants.vibrationEnabledKey, false.ToString());
                vibrationToggle.isOn = false;
            }
        }
        else
        {
            // disable vibration toggle
            vibrationToggle.interactable = false;
            vibrationToggle.isOn = false;
        }

        // food suits enabled
        if (bool.TryParse(PlayerPrefs.GetString(Constants.foodSuitsEnabledKey), out isOn))
        {
            foodSuitsToggle.isOn = isOn;
        }
        else
        {
            // unable to parse
            PlayerPrefs.SetString(Constants.foodSuitsEnabledKey, false.ToString());
            foodSuitsToggle.isOn = false;
        }

        // target frame rate settings
        // https://docs.unity3d.com/ScriptReference/Application-targetFrameRate.html
        // WebGL has it's own thing
        //if (Application.isMobilePlatform || Constants.inEditor)
        if (true)
        {
            // make a list of most of the supported target frame rates
            // supported means that the screen's maximum refresh rate is divisible by the target
            int maxFrameRate = Screen.currentResolution.refreshRate;
            frameRates = maxFrameRate switch
            {
                240 => new List<int>() { -1, 30, 40, 60, 80, 120, 240 },
                144 => new List<int>() { -1, 36, 48, 72, 144 },
                120 => new List<int>() { -1, 30, 40, 60, 120 },
                90 => new List<int>() { -1, 30, 45, 90 },
                60 => new List<int>() { -1, 30, 60 },
                30 => new List<int>() { -1, 30 },
                _ => new List<int>() { -1, maxFrameRate },
            };

            // -1 is the default for the platform
            int frameRateSetting = PlayerPrefs.GetInt(Constants.frameRateKey, -1);

            // figure out if the frame rate setting exists in our list of target frame rates
            int frameRateIndex = frameRates.IndexOf(frameRateSetting);
            if (frameRateIndex == -1)
            {
                // if the setting is valid add it to the list
                if (frameRateSetting > 0 && maxFrameRate % frameRateSetting == 0)
                {
                    Debug.LogWarning($"the valid frame rate of {frameRateSetting} was not found in our list of target frame rates, adding it to them now.");
                    bool addedToList = false;
                    for (int i = 1; i < frameRates.Count; i++)
                    {
                        if (frameRateSetting < frameRates[i])
                        {
                            frameRates.Insert(i, frameRateSetting);
                            frameRateIndex = i;
                            addedToList = true;
                            break;
                        }
                    }
                    if (!addedToList)
                    {
                        frameRates.Add(frameRateSetting);
                        frameRateIndex = frameRates.Count - 1;
                    }
                }
                else
                {
                    Debug.LogError($"the an unsupported frame rate of {frameRateSetting} was saved, defaulting to our minimum");
                    frameRateSetting = frameRates[0];
                    Application.targetFrameRate = frameRateSetting;
                    PlayerPrefs.SetInt(Constants.frameRateKey, frameRateSetting);
                    frameRateIndex = 0;
                }
            }

            frameRateSlider.minValue = 0;
            frameRateSlider.maxValue = frameRates.Count - 1;
            frameRateSlider.value = frameRateIndex;

            if (frameRateSetting == -1)
            {
                frameRateText.text = "DEFAULT";
            }
            else
            {
                frameRateText.text = frameRateSetting.ToString();
            }
        }
        else
        {
            // disable frame rate setting
            frameRateSlider.interactable = false;
            frameRateText.text = Screen.currentResolution.refreshRate.ToString();
        }

        lockout = false;
    }

    public void MusicVolumeChange(float update)
    {
        if (lockout)
            return;

        int volumeUpdate = (int)update;
        musicVolumeText.text = $"{volumeUpdate * musicMultiplier}%";
        PlayerPrefs.SetInt(Constants.musicVolumeKey, volumeUpdate);
        update /= Constants.musicVolumeDenominator;
        MusicController.Instance.UpdateMaxVolume(update);
    }

    public void SoundEffectsVolumeChange(float update)
    {
        if (lockout)
            return;

        int volumeUpdate = (int)update;
        soundEffectsVolumeText.text = $"{volumeUpdate * soundEffectsMultiplier}%";
        PlayerPrefs.SetInt(Constants.soundEffectsVolumeKey, volumeUpdate);
        update /= Constants.soundEffectsVolumeDenominator;
        SoundEffectsController.Instance.UpdateMaxVolume(update);
        SoundEffectsController.Instance.ButtonPressSound(vibrate: false);

        if (SpaceBabyController.Instance != null)
        {
            SpaceBabyController.Instance.UpdateMaxVolume(update);
        }

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

        if (SceneManager.GetActiveScene().name.Equals(Constants.gameplayScene))
        {
            suitArtNoticeObject.SetActive(true);
        }
    }

    public void TryToClearRecordsButton()
    {
        if (lockout)
            return;

        confirmYesButton.interactable = false;
        confirmObject.SetActive(true);
        StartCoroutine(ButtonDelay());
    }

    public void ClearRecordsConfirmationButton()
    {
        Debug.Log("clearing saved records");

        // since ResultsScript.cs detects and auto fills the very first records this is how it must be done
        foreach (string difficulty in Config.GameValues.difficulties)
        {
            PlayerPrefs.DeleteKey(PlayerPrefKeys.GetHighScoreKey(difficulty));
            PlayerPrefs.DeleteKey(PlayerPrefKeys.GetLeastMovesKey(difficulty));
        }
    }

    public void FrameRateChange(float update)
    {
        if (lockout) return;

        int frameRateIndex = (int)update;
        if (frameRateIndex < 0 || frameRateIndex >= frameRates.Count)
        {
            Debug.LogError($"an invalid frame rate index update of {frameRateIndex} was inputted.");
            frameRateIndex = 0;
        }

        int frameRateSetting = frameRates[frameRateIndex];
        PlayerPrefs.SetInt(Constants.frameRateKey, frameRateSetting);
        Debug.Log($"seting the targetFrameRate to: {frameRateSetting}");
        Application.targetFrameRate = frameRateSetting;
        if (frameRateSetting == -1)
        {
            frameRateText.text = "DEFAULT";
        }
        else
        {
            frameRateText.text = frameRateSetting.ToString();
        }
    }

    private IEnumerator ButtonDelay()
    {
        yield return new WaitForSeconds(2);
        confirmYesButton.interactable = true;
    }
}
