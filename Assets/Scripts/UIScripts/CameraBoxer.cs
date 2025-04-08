using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraBoxer : MonoBehaviour
{
    // modified and improved script that started from here: https://github.com/rabidgremlin/LetterBoxer
    public static CameraBoxer Instance { get; private set; }

    private const float minX = 9;
    private const float minY = 20;

    private const float maxX = 9;
    private const float maxY = 15;

    private const float minRatio = minX / minY;
    private const float maxRatio = maxX / maxY;

    private List<Camera> cameras;
    private Camera currentCamera;
    private int lastScreenWidth, lastScreenHeight;

#if UNITY_WEBGL || UNITY_IOS || UNITY_EDITOR
    private static readonly WaitForSecondsRealtime screenCheckWait = new(0.5f);
#endif
#if UNITY_ANDROID
    private static readonly WaitForSecondsRealtime androidConfigDelay = new(0.1f);
#endif

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;

        cameras = new List<Camera>(SceneManager.sceneCountInBuildSettings);
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }

#if UNITY_WEBGL || UNITY_IOS || UNITY_EDITOR
    private void Start()
    {
        if (Instance != this) return;
        StartCoroutine(CheckScreenChangeContinuous());
    }
#endif

    public void AddCamera(Camera newCamera)
    {
        if (cameras.Count != 0)
            cameras[^1].enabled = false;
        currentCamera = newCamera;
        cameras.Add(currentCamera);
        currentCamera.enabled = true;
        PerformSizing();
        AchievementPopup.Instance.CameraChange(currentCamera);
    }

    public void RemoveCamera(Camera oldCamera)
    {
        int oldCameraIndex = cameras.LastIndexOf(oldCamera);
        if (oldCameraIndex == -1)
        {
            Debug.LogError("tried to remove a camera that is not being tracked");
            return;
        }

        cameras.RemoveAt(oldCameraIndex);
        if (cameras.Count == 0) return;
        currentCamera = cameras[^1];
        currentCamera.enabled = true;
        PerformSizing(true);
        AchievementPopup.Instance.CameraChange(currentCamera);
    }


#if UNITY_ANDROID
    public void AndroidCheckScreenChange()
    {
        StartCoroutine(WaitForAndroid());
    }

    private IEnumerator WaitForAndroid()
    {
        yield return androidConfigDelay;
        CheckScreenSizeChange();
    }
#endif

#if UNITY_WEBGL || UNITY_IOS || UNITY_EDITOR
    private IEnumerator CheckScreenChangeContinuous()
    {
        while (true)
        {
            yield return screenCheckWait;
            CheckScreenSizeChange();
        }
    }
#endif

    private void CheckScreenSizeChange()
    {
        if (lastScreenWidth != Screen.width ||
            lastScreenHeight != Screen.height)
        {
            Debug.Log($"Screen size change. Width: {Screen.width}, Height: {Screen.height}");
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            PerformSizing(true);
        }
    }

    // based on logic here from http://gamedesigntheory.blogspot.com/2010/09/controlling-aspect-ratio-in-unity.html
    private void PerformSizing(bool again = false)
    {
        // determine the game window's current aspect ratio
        float screenAspectRatio = (float) Screen.width / Screen.height;

        Rect rect = currentCamera.rect;
        if (screenAspectRatio < minRatio)
        {
            // add letter boxer
            float scaleheight = screenAspectRatio / minRatio;
            rect.width = 1;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1 - scaleheight) / 2;
        }
        else if (screenAspectRatio > maxRatio)
        {
            // add pillar boxer
            float scaleWidth = 1 / (screenAspectRatio / maxRatio);
            rect.width = scaleWidth;
            rect.height = 1;
            rect.x = (1 - scaleWidth) / 2;
            rect.y = 0;
        }
        else if (again)
        {
            // resize back to supported ratio
            rect.width = 1;
            rect.height = 1;
            rect.x = 0;
            rect.y = 0;
        }
        else
        {
            return;
        }
        currentCamera.rect = rect;
    }
}
