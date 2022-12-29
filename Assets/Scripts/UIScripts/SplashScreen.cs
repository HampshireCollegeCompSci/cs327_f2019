using System.Collections;
using UnityEngine;

public class SplashScreen : MonoBehaviour
{
    private static bool firstRun = true;

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
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                splashScreenFade = StartCoroutine(DisplayLogo());
            }
        }
    }

    private IEnumerator DisplayLogo()
    {
        yield return new WaitForSeconds(2);
        MusicController.Instance.MainMenuMusic();
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
        MusicController.Instance.MainMenuMusic();
    }
}
