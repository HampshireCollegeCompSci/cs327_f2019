using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SummaryTransition : MonoBehaviour
{
    [SerializeField]
    private GameObject transitionPanel;
    [SerializeField]
    private Sprite spaceShipDebrisSprite;
    [SerializeField]
    private GameObject spaceShipObject;
    [SerializeField]
    private GameObject explosionPrefab;
    [SerializeField]
    private WinSequence winSequence;

    private GameObject[] explosions;

    private bool exploded;

    public SummaryTransition()
    {
        exploded = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FadeIn());

        if (!Actions.GameWin)
        {
            SpaceBabyController.Instance.PlayLoseAnimation();
        }
    }

    public void SpaceShipExplode()
    {
        if (exploded) return;

        exploded = true;
        StartCoroutine(Explode());
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

    private IEnumerator FadeIn()
    {
        transitionPanel.SetActive(true);
        yield return Animate.FadeImage(transitionPanel.GetComponent<Image>(),
            Actions.GameWin ? GameValues.FadeColors.grayFadeOut : GameValues.FadeColors.blackFadeOut,
            GameValues.AnimationDurataions.summaryFadeIn);
        transitionPanel.SetActive(false);

        if (Actions.GameWin)
        {
            winSequence.StartWinSequence();
        }
        else
        {
            SpaceShipExplode();
        }
    }

    private IEnumerator Explode()
    {
        explosions = new GameObject[4];
        explosions[0] = Instantiate(explosionPrefab, spaceShipObject.transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
        explosions[0].GetComponent<SpriteRenderer>().sortingOrder = 2;
        explosions[0].transform.localScale = new Vector3(0.1f, 0.1f);
        explosions[0].transform.position += new Vector3(0.3f, 0.3f, 0);
        explosions[0].GetComponent<Animator>().Play(Constants.AnimatorIDs.loseExplosionID);

        yield return new WaitForSeconds(0.2f);
        explosions[1] = Instantiate(explosionPrefab, spaceShipObject.transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
        explosions[1].GetComponent<SpriteRenderer>().sortingOrder = 2;
        explosions[1].transform.localScale = new Vector3(0.1f, 0.1f);
        explosions[1].transform.position += new Vector3(0.3f, -0.1f, 0);
        explosions[1].GetComponent<Animator>().Play(Constants.AnimatorIDs.loseExplosionID);

        yield return new WaitForSeconds(0.2f);
        explosions[2] = Instantiate(explosionPrefab, spaceShipObject.transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
        explosions[2].GetComponent<SpriteRenderer>().sortingOrder = 2;
        explosions[2].transform.localScale = new Vector3(0.1f, 0.1f);
        explosions[2].transform.position += new Vector3(-0.25f, 0.1f, 0);
        explosions[2].GetComponent<Animator>().Play(Constants.AnimatorIDs.loseExplosionID);

        yield return new WaitForSeconds(5);
        explosions[3] = Instantiate(explosionPrefab, spaceShipObject.transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
        explosions[3].GetComponent<SpriteRenderer>().sortingOrder = 2;
        explosions[3].transform.localScale = new Vector3(0.5f, 0.5f);

        yield return new WaitForSeconds(0.1f);
        Destroy(explosions[0]);
        Destroy(explosions[1]);
        Destroy(explosions[2]);
        SoundEffectsController.Instance.ExplosionSound();
        spaceShipObject.GetComponent<Image>().sprite = spaceShipDebrisSprite;
        spaceShipObject.transform.localScale = new Vector3(1.5f, 1.5f, 1);

        yield return new WaitForSeconds(1);
        Destroy(explosions[3]);
    }
}
