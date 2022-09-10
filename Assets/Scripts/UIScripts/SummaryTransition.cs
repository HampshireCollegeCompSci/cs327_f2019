using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SummaryTransition : MonoBehaviour
{
    public GameObject transitionPanel;
    public Sprite spaceShipDebrisSprite;
    public GameObject spaceShipObject;
    public GameObject explosionPrefab;
    public WinSequence winSequence;

    private GameObject[] explosions;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FadeIn());

        if (!Config.Instance.gameWin)
        {
            SpaceBabyController.Instance.PlayLoseAnimation();
        }
    }

    private IEnumerator FadeIn()
    {
        transitionPanel.SetActive(true);
        Image panelImage = transitionPanel.GetComponent<Image>();
        Color panelColor = Config.Instance.gameWin ? Config.GameValues.fadeLightColor : Config.GameValues.fadeDarkColor;
        panelColor.a = 1;
        panelImage.color = panelColor;
        yield return null;

        while (panelColor.a > 0)
        {
            panelColor.a -= Time.deltaTime * Config.GameValues.summaryTransitionSpeed;
            panelImage.color = panelColor;
            yield return null;
        }
        transitionPanel.SetActive(false);

        if (Config.Instance.gameWin)
        {
            winSequence.StartWinSequence();
        }
        else
        {
            SpaceShipExplode();
        }
    }

    private bool exploded = false;
    public void SpaceShipExplode()
    {
        if (exploded) return;

        exploded = true;
        StartCoroutine(Explode());
    }

    IEnumerator Explode()
    {
        explosions = new GameObject[4];
        explosions[0] = Instantiate(explosionPrefab, spaceShipObject.transform.position, Quaternion.identity);
        explosions[0].GetComponent<SpriteRenderer>().sortingOrder = 2;
        explosions[0].transform.localScale = new Vector3(0.1f, 0.1f);
        explosions[0].transform.position += new Vector3(0.3f, 0.3f, 0);
        explosions[0].GetComponent<Animator>().Play("LoseExplosionAnim");
        
        yield return new WaitForSeconds(0.2f);
        explosions[1] = Instantiate(explosionPrefab, spaceShipObject.transform.position, Quaternion.identity);
        explosions[1].GetComponent<SpriteRenderer>().sortingOrder = 2;
        explosions[1].transform.localScale = new Vector3(0.1f, 0.1f);
        explosions[1].transform.position += new Vector3(0.3f, -0.1f, 0);
        explosions[1].GetComponent<Animator>().Play("LoseExplosionAnim");

        yield return new WaitForSeconds(0.2f);
        explosions[2] = Instantiate(explosionPrefab, spaceShipObject.transform.position, Quaternion.identity);
        explosions[2].GetComponent<SpriteRenderer>().sortingOrder = 2;
        explosions[2].transform.localScale = new Vector3(0.1f, 0.1f);
        explosions[2].transform.position += new Vector3(-0.25f, 0.1f, 0);
        explosions[2].GetComponent<Animator>().Play("LoseExplosionAnim");

        yield return new WaitForSeconds(5);
        explosions[3] = Instantiate(explosionPrefab, spaceShipObject.transform.position, Quaternion.identity);
        explosions[3].GetComponent<SpriteRenderer>().sortingOrder = 2;
        explosions[3].transform.localScale = new Vector3(0.5f, 0.5f);

        yield return new WaitForSeconds(0.1f);
        Destroy(explosions[0]);
        Destroy(explosions[1]);
        Destroy(explosions[2]);
        SoundEffectsController.Instance.ExplosionSound();
        spaceShipObject.GetComponent<Image>().sprite = spaceShipDebrisSprite;
        spaceShipObject.transform.localScale = new Vector3(1.5f, 1.5f, 1);
    }

    public void StopExplosion()
    {
        StopAllCoroutines();
        if (explosions == null) return;
        foreach (GameObject explosion in explosions)
        {
            if (explosion != null)
            {
                explosion.SetActive(false);
            }
        }
    }
}
