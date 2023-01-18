using System;
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
    private Toggle saveGameStateToggle;
    [SerializeField]
    private InputField movesUntilSaveInputField;

    [SerializeField]
    private GameObject suitArtNoticeObject;

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

        // music volume
        musicSlider.maxValue = GameValues.Settings.musicVolumeDenominator;
        musicMultiplier = 100 / GameValues.Settings.musicVolumeDenominator;
        int volume = PersistentSettings.MusicVolume;
        musicSlider.value = volume;
        musicVolumeText.text = $"{volume * musicMultiplier}%";

        // sound effects volume
        soundEffectsSlider.maxValue = GameValues.Settings.soundEffectsVolumeDenominator;
        soundEffectsMultiplier = 100 / GameValues.Settings.soundEffectsVolumeDenominator;
        volume = PersistentSettings.SoundEffectsVolume;
        soundEffectsSlider.value = volume;
        soundEffectsVolumeText.text = $"{volume * soundEffectsMultiplier}%";

        if (Vibration.HasVibrator())
        {
            vibrationToggle.isOn = PersistentSettings.VibrationEnabled;
        }
        else
        {
            // disable vibration toggle
            vibrationToggle.interactable = false;
            vibrationToggle.isOn = false;
        }

        foodSuitsToggle.isOn = PersistentSettings.VibrationEnabled;

        SetupFrameRateSettings();

        saveGameStateToggle.isOn = PersistentSettings.SaveGameStateEnabled;
        movesUntilSaveInputField.text = PersistentSettings.MovesUntilSave.ToString();

        lockout = false;
    }

    public void BackButton()
    {
        Debug.Log("UI Button Settings Back");
        SceneManager.UnloadSceneAsync(Constants.ScenesNames.settings);
    }

    public void MusicVolumeChange(float update)
    {
        if (lockout) return;

        int volumeUpdate = (int)update;
        musicVolumeText.text = $"{volumeUpdate * musicMultiplier}%";
        PersistentSettings.MusicVolume = volumeUpdate;
        MusicController.Instance.UpdateMaxVolume(volumeUpdate);
    }

    public void SoundEffectsVolumeChange(float update)
    {
        if (lockout) return;

        int volumeUpdate = (int)update;
        soundEffectsVolumeText.text = $"{volumeUpdate * soundEffectsMultiplier}%";
        PersistentSettings.SoundEffectsVolume = volumeUpdate;
        SoundEffectsController.Instance.UpdateMaxVolume(volumeUpdate);
        SoundEffectsController.Instance.ButtonPressSound(vibrate: false);

        if (SpaceBabyController.Instance != null)
        {
            SpaceBabyController.Instance.UpdateMaxVolume(volumeUpdate);
        }
    }

    public void VibrationEnabledOnToggle(bool update)
    {
        if (lockout) return;

        PersistentSettings.VibrationEnabled = update;
        SoundEffectsController.Instance.ButtonPressSound();
    }

    public void FoodSuitsEnabledOnToggle(bool update)
    {
        if (lockout) return;

        PersistentSettings.FoodSuitsEnabled = update;
        SoundEffectsController.Instance.ButtonPressSound();

        if (SceneManager.GetActiveScene().name.Equals(Constants.ScenesNames.gameplay))
        {
            suitArtNoticeObject.SetActive(true);
        }
    }

    public void TryToClearRecordsButton()
    {
        if (lockout) return;

        confirmYesButton.interactable = false;
        Debug.Log("trying to clear records");
        confirmObject.SetActive(true);
        StartCoroutine(ButtonDelay());
    }

    public void ClearRecordsConfirmationButton()
    {
        Debug.Log("clearing saved records");
        PersistentSettings.ClearScores();
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
        Debug.Log($"seting the targetFrameRate to: {frameRateSetting}");
        Application.targetFrameRate = frameRateSetting;
        PersistentSettings.FrameRate = frameRateSetting;
        UpdateFrameRateText(frameRateSetting);
    }

    public void SaveGameStateOnToggle(bool update)
    {
        if (lockout) return;
        Debug.Log($"seting save game state to: {update}");
        PersistentSettings.SaveGameStateEnabled = update;
        SoundEffectsController.Instance.ButtonPressSound();

        if (SceneManager.GetActiveScene().name.Equals(Constants.ScenesNames.gameplay))
        {
            StateLoader.Instance.SetGameStateSaving(update);
        }
        if (!update)
        {
            SaveFile.Delete();
        }
    }

    public void MovesUntilSaveOnEndEdit(string update)
    {
        if (lockout) return;
        Debug.Log($"seting moves until save to: {update}");

        if (Int32.TryParse(update, out int movesUntilSave) && movesUntilSave > 0)
        {
            PersistentSettings.MovesUntilSave = movesUntilSave;
            if (SceneManager.GetActiveScene().name.Equals(Constants.ScenesNames.gameplay))
            {
                StateLoader.Instance.UpdateMovesUntilSave(movesUntilSave);
            }
        }
        else
        {
            movesUntilSaveInputField.text = PersistentSettings.MovesUntilSave.ToString();
        }
    }

    private void SetupFrameRateSettings()
    {
        // target frame rate settings
        // https://docs.unity3d.com/ScriptReference/Application-targetFrameRate.html
        // make a list of most of the supported target frame rates
        // supported means that the screen's maximum refresh rate is divisible by the target
        int maxFrameRate = Screen.currentResolution.refreshRate;
        frameRates = maxFrameRate switch
        {
            240 => new List<int>(7) { -1, 30, 40, 60, 80, 120, 240 },
            144 => new List<int>(5) { -1, 36, 48, 72, 144 },
            120 => new List<int>(5) { -1, 30, 40, 60, 120 },
            90 => new List<int>(4) { -1, 30, 45, 90 },
            60 => new List<int>(3) { -1, 30, 60 },
            48 => new List<int>(3) { -1, 24, 48 },
            30 => new List<int>(3) { -1, 15, 30 },
            _ => new List<int>(2) { -1, maxFrameRate },
        };

        // -1 is the default for the platform
        int frameRateSetting = PersistentSettings.FrameRate;

        // figure out if the frame rate setting exists in our list of target frame rates
        int frameRateIndex = frameRates.IndexOf(frameRateSetting);
        if (frameRateIndex == -1)
        {
            Debug.LogWarning($"the frame rate of {frameRateSetting} was not found in our list of target frame rates, adding it to them now.");
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

        frameRateSlider.minValue = 0;
        frameRateSlider.maxValue = frameRates.Count - 1;
        frameRateSlider.value = frameRateIndex;

        UpdateFrameRateText(frameRateSetting);
    }

    private void UpdateFrameRateText(int frameRate)
    {
        frameRateText.text = frameRate switch
        {
            -1 => "Default",
            _ => frameRate.ToString()
        };
    }

    private IEnumerator ButtonDelay()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        confirmYesButton.interactable = true;
    }
}
