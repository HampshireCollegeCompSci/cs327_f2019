#if UNITY_ANDROID
using UnityEngine;
using UnityEngine.Android;

// started from: https://docs.unity3d.com/ScriptReference/Android.AndroidApplication-onConfigurationChanged.html

public class AndroidScreenChange : MonoBehaviour
{
    public static AndroidScreenChange Instance { get; private set; }
    private ScreenConfig lastConfig;

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;

        // this is true when unity editor is the platform
        if (AndroidApplication.currentConfiguration == null) return;
        lastConfig = new ScreenConfig(AndroidApplication.currentConfiguration);
        AndroidApplication.onConfigurationChanged += OnConfigurationChanged;
    }

    private void OnDisable()
    {
        AndroidApplication.onConfigurationChanged -= OnConfigurationChanged;
    }

    private void OnConfigurationChanged(AndroidConfiguration newConfig)
    {
        Debug.Log($"Android Configuration Change Reported.");
        if (lastConfig.Equals(newConfig)) return;
        CameraBoxer.Instance.AndroidCheckScreenChange();
        lastConfig.CopyFrom(newConfig);
    }

    private class ScreenConfig
    {
        private AndroidOrientation orientation;
        private int screenWidth, screenHeight;

        public ScreenConfig(AndroidConfiguration config)
        {
            CopyFrom(config);
        }

        public void CopyFrom(AndroidConfiguration config)
        {
            orientation = config.orientation;
            screenWidth = config.screenWidthDp;
            screenHeight = config.screenHeightDp;
        }

        public bool Equals(AndroidConfiguration config)
        {
            return orientation == config.orientation &&
                screenWidth == config.screenWidthDp &&
                screenHeight == config.screenHeightDp;
        }
    }
}
#endif
