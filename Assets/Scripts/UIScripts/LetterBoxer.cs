using System;
#if UNITY_WEBGL
using System.Collections;
#endif
using UnityEngine;

public class LetterBoxer : MonoBehaviour
{
    // modified and improved script that started from here: https://github.com/rabidgremlin/LetterBoxer

    private const float minX = 9;
    private const float minY = 20;

    private const float maxX = 9;
    private const float maxY = 15;

    private const float minRatio = minX / minY;
    private const float maxRatio = maxX / maxY;

    private Camera cam;

#if UNITY_WEBGL
    private static readonly WaitForSecondsRealtime screenCheckWait = new(1);
    private int currentScreenWidth, currentScreenHeight;
#endif

    private void Awake()
    {
        // store reference to the camera
        cam = GetComponent<Camera>();

        PerformSizing();
#if UNITY_WEBGL
        currentScreenWidth = Screen.width;
        currentScreenHeight = Screen.height;
#endif
    }

#if UNITY_WEBGL
    private void Start()
    {
        StartCoroutine(CheckScreenChangeContinuous());   
    }

    /// <summary>
    /// Continuously checks for screen size changes and updates the letterbox accordingly
    /// </summary>
    private IEnumerator CheckScreenChangeContinuous()
    {
        while(true)
        {
            yield return screenCheckWait;
            if (currentScreenWidth != Screen.width ||
                currentScreenHeight != Screen.height)
            {
                Debug.LogWarning("screen size has changed!");
                currentScreenWidth = Screen.width;
                currentScreenHeight = Screen.height;
                PerformSizing(again : true);
            }
        }
    }
#endif

    // based on logic here from http://gamedesigntheory.blogspot.com/2010/09/controlling-aspect-ratio-in-unity.html
    private void PerformSizing(bool again = false)
    {
        // determine the game window's current aspect ratio
        float windowaspect = Screen.width / (float)Screen.height;
        windowaspect = (float) Math.Round(windowaspect, 2);

        // current viewport height should be scaled by this amount
        float scaleheight;
        if (windowaspect < minRatio)
        {
            scaleheight = windowaspect / minRatio;
        }
        else if (windowaspect > maxRatio)
        {
            scaleheight = windowaspect / maxRatio;
        }
        else if (again)
        {
            Rect rect1 = cam.rect;
            rect1.width = 1;
            rect1.height = 1;
            rect1.x = 0;
            rect1.y = 0;
            cam.rect = rect1;
            return;
        }
        else
        {
            return;
        }

        Rect rect = cam.rect;
        // if scaled height is less than current height, add letterbox
        if (scaleheight < 1)
        {
            rect.width = 1;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1 - scaleheight) / 2;
        }
        else // add pillarbox
        {
            float scalewidth = 1 / scaleheight;
            rect.width = scalewidth;
            rect.height = 1;
            rect.x = (1 - scalewidth) / 2;
            rect.y = 0;
        }
        cam.rect = rect;
    }
}
