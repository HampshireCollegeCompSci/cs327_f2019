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
    [SerializeField]
    private CanvasGroup splashScreenGroup, logoGroup;
    private Coroutine splashScreenFade;

    // Start is called before the first frame update
    private void Start()
    {
        if (firstRun)
        {
            firstRun = false;
            splashScreen.SetActive(true);
            logoGroup.alpha = 0;
            splashScreenFade = StartCoroutine(DisplayLogo());
        }
    }

    private IEnumerator DisplayLogo()
    {
        yield return new WaitForSeconds(0.25f);
        yield return Animate.FadeCanvasGroup(logoGroup,
            0, 1, GameValues.AnimationDurataions.logoDelay);
        yield return new WaitForSeconds(GameValues.AnimationDurataions.logoDelay);
#if UNITY_WEBGL
        TryStartMusic();
#else
        MusicController.Instance.MainMenuMusic();
#endif
        yield return Animate.FadeCanvasGroup(splashScreenGroup,
            1, 0, GameValues.AnimationDurataions.logoDelay);
        splashScreen.SetActive(false);
    }

    public void SkipSplashScreen()
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
