using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SummaryTransition : MonoBehaviour
{
    public GameObject transitionPanel;
    public Sprite spaceShipDebrisSprite;
    public GameObject spaceShipObject;
    public GameObject explosionPrefab;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FadeIn());
        // spacebaby
        if (Config.Instance.gameWin)
        {
            SpaceBabyController.Instance.BabyWinSummary(Config.Instance.matchCounter);
        }
        else
        {
            SpaceBabyController.Instance.BabyLoseSummary();
        }
    }

    private IEnumerator FadeIn()
    {
        transitionPanel.SetActive(true);
        Image panelImage = transitionPanel.GetComponent<Image>();

        Color panelColor;
        if (Config.Instance.gameWin)
        {
            panelColor = Color.white;
        }
        else
        {
            panelColor = Color.black;
        }

        panelImage.color = panelColor;
        yield return null;

        while (panelColor.a > 0)
        {
            panelColor.a -= Time.deltaTime * Config.GameValues.summaryTransitionSpeed;
            panelImage.color = panelColor;
            yield return null;
        }
        transitionPanel.SetActive(false);

        if (!Config.Instance.gameWin)
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
        GameObject explosion0 = Instantiate(explosionPrefab, spaceShipObject.transform.position, Quaternion.identity);
        explosion0.GetComponent<SpriteRenderer>().sortingOrder = 2;
        explosion0.transform.localScale = new Vector3(0.1f, 0.1f);
        explosion0.transform.position += new Vector3(0.3f, 0.3f, 0);
        explosion0.GetComponent<Animator>().Play("LoseExplosionAnim");
        
        yield return new WaitForSeconds(0.2f);
        GameObject explosion1 = Instantiate(explosionPrefab, spaceShipObject.transform.position, Quaternion.identity);
        explosion1.GetComponent<SpriteRenderer>().sortingOrder = 2;
        explosion1.transform.localScale = new Vector3(0.1f, 0.1f);
        explosion1.transform.position += new Vector3(0.3f, -0.2f, 0);
        explosion1.GetComponent<Animator>().Play("LoseExplosionAnim");

        yield return new WaitForSeconds(0.2f);
        GameObject explosion2 = Instantiate(explosionPrefab, spaceShipObject.transform.position, Quaternion.identity);
        explosion2.GetComponent<SpriteRenderer>().sortingOrder = 2;
        explosion2.transform.localScale = new Vector3(0.1f, 0.1f);
        explosion2.transform.position += new Vector3(-0.3f, 0.1f, 0);
        explosion2.GetComponent<Animator>().Play("LoseExplosionAnim");

        yield return new WaitForSeconds(5);
        GameObject explosion3 = Instantiate(explosionPrefab, spaceShipObject.transform.position, Quaternion.identity);
        explosion3.GetComponent<SpriteRenderer>().sortingOrder = 2;
        explosion3.transform.localScale = new Vector3(0.5f, 0.5f);

        yield return new WaitForSeconds(0.1f);
        Destroy(explosion0);
        Destroy(explosion1);
        Destroy(explosion2);
        SoundEffectsController.Instance.ExplosionSound();
        spaceShipObject.GetComponent<Image>().sprite = spaceShipDebrisSprite;
        spaceShipObject.transform.localScale = new Vector3(2, 2, 1);
    }
}
