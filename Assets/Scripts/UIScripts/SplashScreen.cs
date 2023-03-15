using System.Collections;
using UnityEngine;

public class SplashScreen : MonoBehaviour
{
    private static bool firstRun = true;

#if UNITY_WEBGL
    private bool musicCanBeStarted = false;
    private bool hasBeenFocused = false;
#endif

    [SerializeField]
    private GameObject splashScreen;
    private Coroutine splashScreenFade;

    // Start is called before the first frame update
    private void Start()
    {
        if (firstRun)
        {
            firstRun = false;
            splashScreen.SetActive(true);
            splashScreenFade = StartCoroutine(DisplayLogo());
        }
    }

    private IEnumerator DisplayLogo()
    {
        yield return new WaitForSeconds(2);
        #if UNITY_WEBGL
            TryStartMusic();
        #else
            MusicController.Instance.MainMenuMusic();
        #endif
        yield return Animate.FadeCanvasGroup(splashScreen.GetComponent<CanvasGroup>(),
            1, 0, GameValues.AnimationDurataions.logoDelay);
        splashScreen.SetActive(false);
    }

    [SerializeField]
    private void SkipSplashScreen()
    {
        // the splash screen acts as a big button and clicking it calls this
        if (splashScreenFade != null)
        {
            StopCoroutine(splashScreenFade);
        }
        splashScreen.SetActive(false);
        #if UNITY_WEBGL
            TryStartMusic();
        #else
            MusicController.Instance.MainMenuMusic();
        #endif
    }


#if UNITY_WEBGL
    private void TryStartMusic()
    {
        if (hasBeenFocused)
        {
            MusicController.Instance.MainMenuMusic();
        }
        else
        {
            musicCanBeStarted = true;
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (musicCanBeStarted)
        {
            MusicController.Instance.MainMenuMusic();
        }
        else if (hasFocus)
        {
            hasBeenFocused = true;
        }
    }
#endif
}
