using System.Collections;
using UnityEngine;

public class LetterBoxer : MonoBehaviour
{
    // started from here: https://github.com/rabidgremlin/LetterBoxer

    private const float minX = 9;
    private const float minY = 20;

    private const float maxX = 9;
    private const float maxY = 15;

    private const float minRatio = minX / minY;
    private const float maxRatio = maxX / maxY;

    private Camera cam, letterBoxerCamera;
#if UNITY_WEBGL
    private int currentScreenWidth, currentScreenHeight;
#endif

    private void Awake()
    {
        // store reference to the camera
        cam = GetComponent<Camera>();
        // add the letterboxing camera
        AddLetterBoxingCamera();

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

    private IEnumerator CheckScreenChangeContinuous()
    {
        while(true)
        {
            yield return new WaitForSecondsRealtime(1);
            if (currentScreenWidth != Screen.width ||
                currentScreenHeight != Screen.height)
            {
                currentScreenWidth = Screen.width;
                currentScreenHeight = Screen.height;
                PerformSizing();
            }
        }
    }
#endif

    private void AddLetterBoxingCamera()
    {
        // create a camera to render bcakground used for matte bars
        letterBoxerCamera = new GameObject().AddComponent<Camera>();
        letterBoxerCamera.backgroundColor = Color.black;
        letterBoxerCamera.cullingMask = 0;
        letterBoxerCamera.depth = -10;
        letterBoxerCamera.farClipPlane = 1;
        letterBoxerCamera.useOcclusionCulling = false;
        letterBoxerCamera.allowHDR = false;
        letterBoxerCamera.allowMSAA = false;
        letterBoxerCamera.clearFlags = CameraClearFlags.Color;
        letterBoxerCamera.name = "Letter Boxer Camera";
    }

    // based on logic here from http://gamedesigntheory.blogspot.com/2010/09/controlling-aspect-ratio-in-unity.html
    private void PerformSizing()
    {
        // determine the game window's current aspect ratio
        float windowaspect = (float)Screen.width / (float)Screen.height;

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
        else
        {
            return;
        }

        Rect rect = cam.rect;
        // if scaled height is less than current height, add letterbox
        if (scaleheight < 1.0f)
        {
            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;
        }
        else // add pillarbox
        {
            float scalewidth = 1.0f / scaleheight;

            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;
        }
        cam.rect = rect;
    }
}
