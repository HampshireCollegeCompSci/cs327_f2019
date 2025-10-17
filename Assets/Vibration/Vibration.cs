////////////////////////////////////////////////////////////////////////////////
//
// @author Benoît Freslon @benoitfreslon
// https://github.com/BenoitFreslon/Vibration
// https://benoitfreslon.com
//
////////////////////////////////////////////////////////////////////////////////

// CHANGES NOT MADE BY INITIAL AUTHOR
// 1. Formatted Lines
// 2. Removed unused code and moved conditional compilation segments around

using UnityEngine;

#if UNITY_IOS
using System.Runtime.InteropServices.ComTypes;
using System.Collections;
using System.Runtime.InteropServices;

#elif UNITY_WEBGL
using System.Runtime.InteropServices;
#endif

public static class Vibration
{
#if UNITY_IOS
    [DllImport ( "__Internal" )]
    private static extern bool _HasVibrator ();

    [DllImport ( "__Internal" )]
    private static extern void _Vibrate ();

    [DllImport ( "__Internal" )]
    private static extern void _VibratePop ();

    [DllImport ( "__Internal" )]
    private static extern void _VibratePeek ();

    [DllImport ( "__Internal" )]
    private static extern void _VibrateNope ();

    [DllImport("__Internal")]
    private static extern void _impactOccurred(string style);

    [DllImport("__Internal")]
    private static extern void _notificationOccurred(string style);

    [DllImport("__Internal")]
    private static extern void _selectionChanged();

#elif UNITY_ANDROID
    public static AndroidJavaClass unityPlayer;
    public static AndroidJavaObject currentActivity;
    public static AndroidJavaObject vibrator;
    public static AndroidJavaObject context;
    public static AndroidJavaClass vibrationEffect;

#elif UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool _HasVibrator();

    [DllImport("__Internal")]
    private static extern void _Vibrate(int milliseconds);
#endif

    private static bool initialized = false;
    public static void Init()
    {
        if (initialized) return;

#if UNITY_ANDROID
        AndroidVersion = CheckAndroidVersion();
        if (Application.isMobilePlatform)
        {
            unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");

            if (AndroidVersion >= 26)
            {
                vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect");
            }
        }
#endif

        Debug.Log($"App platform is mobile: {Application.isMobilePlatform}");
        HasVibrator = CheckForVibrator();
        initialized = true;
    }

    public static bool HasVibrator { get; private set; }

    public static bool CheckForVibrator()
    {
        if (!Application.isMobilePlatform) return false;
#if UNITY_ANDROID
        AndroidJavaClass contextClass = new AndroidJavaClass("android.content.Context");
        string Context_VIBRATOR_SERVICE = contextClass.GetStatic<string>("VIBRATOR_SERVICE");
        AndroidJavaObject systemService = context.Call<AndroidJavaObject>("getSystemService", Context_VIBRATOR_SERVICE);
        return systemService.Call<bool>("hasVibrator");
#elif UNITY_IOS
        return _HasVibrator ();
#elif UNITY_WEBGL
        return _HasVibrator();
#else
        return false;
#endif
    }

    public static void Vibrate()
    {
        if (!HasVibrator) return;
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#elif UNITY_WEBGL
        _Vibrate(200);
#endif
    }

    ///<summary>
    /// Tiny pop vibration
    ///</summary>
    public static void VibratePop()
    {
        if (!HasVibrator) return;
#if UNITY_IOS
        _VibratePop();
#elif UNITY_ANDROID
        VibrateAndroid(50);
#elif UNITY_WEBGL
        _Vibrate(50);
#endif
    }

    ///<summary>
    /// Small peek vibration
    ///</summary>
    public static void VibratePeek()
    {
        if (!HasVibrator) return;
#if UNITY_IOS
        _VibratePeek();
#elif UNITY_ANDROID
        VibrateAndroid(100);
#elif UNITY_WEBGL
        _Vibrate(100);
#endif
    }

#if UNITY_IOS          
    public static void VibrateIOS(ImpactFeedbackStyle style)
    {
        _impactOccurred(style.ToString());
    }

    public static void VibrateIOS(NotificationFeedbackStyle style)
    {
        _notificationOccurred(style.ToString());
    }

    public static void VibrateIOS_SelectionChanged()
    {
        _selectionChanged();
    }

#elif UNITY_ANDROID
    public static int AndroidVersion { get; private set; }

    public static int CheckAndroidVersion()
    {
        if (Application.platform != RuntimePlatform.Android) return 0;
        string androidVersion = SystemInfo.operatingSystem;
        int sdkPos = androidVersion.IndexOf("API-");
        return int.Parse(androidVersion.Substring(sdkPos + 4, 2).ToString());
    }

    /// https://developer.android.com/reference/android/os/Vibrator.html#vibrate(long)
    public static void VibrateAndroid(long milliseconds)
    {
        if (!HasVibrator) return;
        if (AndroidVersion >= 26)
        {
            AndroidJavaObject createOneShot = vibrationEffect.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, -1);
            vibrator.Call("vibrate", createOneShot);
        }
        else
        {
            vibrator.Call("vibrate", milliseconds);
        }
    }

    /// https://proandroiddev.com/using-vibrate-in-android-b0e3ef5d5e07
    public static void VibrateAndroid(long[] pattern, int repeat)
    {
        if (!HasVibrator)
        if (AndroidVersion >= 26)
        {
            AndroidJavaObject createWaveform = vibrationEffect.CallStatic<AndroidJavaObject>("createWaveform", pattern, repeat);
            vibrator.Call("vibrate", createWaveform);
        }
        else
        {
            vibrator.Call("vibrate", pattern, repeat);
        }
    }

    public static void CancelAndroid()
    {
        if (!HasVibrator) return;
        vibrator.Call("cancel");
    }

#elif UNITY_WEBGL
    public static void VibrateWebGL(int milliseconds)
    {
        if (!HasVibrator) return;
        _Vibrate(milliseconds);
    }
#endif
}

#if UNITY_IOS
public enum ImpactFeedbackStyle
{
    Heavy,
    Medium,
    Light,
    Rigid,
    Soft
}

public enum NotificationFeedbackStyle
{
    Error,
    Success,
    Warning
}
#endif
