using System.Collections;
using UnityEngine;

public class SplashScreen : MonoBehaviour
{
    private static bool firstRun = true;

    public GameObject splashScreen;
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

    IEnumerator DisplayLogo()
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

    public void SkipSplashScreen()
    {
        // the splash screen acts as a big button and clicking it calls this

        StopCoroutine(splashScreenFade);
        splashScreen.SetActive(false);
        MusicController.Instance.MainMenuMusic();
    }
}
