using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
        Image[] logos = splashScreen.GetComponentsInChildren<Image>(true);

        float fade = 1;
        Color splashScreenColor = new Color(0, 0, 0, 1);
        Color logosColor = new Color(1, 1, 1, 1);

        yield return new WaitForSeconds(2);
        MusicController.Instance.MainMenuMusic();

        while (fade > 0)
        {
            yield return null;

            fade -= Time.deltaTime * 0.5f;
            splashScreenColor.a = fade;
            logosColor.a = fade;

            logos[0].color = splashScreenColor;
            logos[1].color = logosColor;
            logos[2].color = logosColor;
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
