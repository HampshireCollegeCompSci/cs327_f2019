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

        CanvasGroup canvasGroup = splashScreen.GetComponent<CanvasGroup>();

        float alpha = canvasGroup.alpha;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * 0.5f;
            canvasGroup.alpha = alpha;
            yield return null;
        }

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
