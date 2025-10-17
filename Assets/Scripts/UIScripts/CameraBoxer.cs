using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraBoxer : MonoBehaviour
{
    // modified and improved script that started from here: https://github.com/rabidgremlin/LetterBoxer
    public static CameraBoxer Instance { get; private set; }

    private const double minX = 9;
    private const double minY = 20;

    private const double maxX = 9;
    private const double maxY = 15;

    private const double minRatio = minX / minY;
    private const double maxRatio = maxX / maxY;

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
        PerformSizing();
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
            //Debug.Log($"Screen size change. Width: {Screen.width}, Height: {Screen.height}");
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            PerformSizing();
        }
    }

    // based on logic here from http://gamedesigntheory.blogspot.com/2010/09/controlling-aspect-ratio-in-unity.html
    private void PerformSizing()
    {
        Rect rect = Screen.safeArea;

        // Convert safe area to normalized viewport
        rect.width /= Screen.width;
        rect.height /= Screen.height;
        rect.x /= Screen.width;
        rect.y /= Screen.height;

        double screenAspectRatio = Math.Round(Screen.safeArea.width / Screen.safeArea.height, 2);

        if (screenAspectRatio < minRatio)
        {
            // add letter boxer
            float scaleheight = (float)(screenAspectRatio / minRatio);
            rect.height = scaleheight;
            rect.y = (1 - scaleheight) / 2;
        }
        else if (screenAspectRatio > maxRatio)
        {
            // add pillar boxer
            float scaleWidth = 1 / (float)(screenAspectRatio / maxRatio);
            rect.width = scaleWidth;
            rect.x = (1 - scaleWidth) / 2;
        }
        currentCamera.rect = rect;
    }
}
