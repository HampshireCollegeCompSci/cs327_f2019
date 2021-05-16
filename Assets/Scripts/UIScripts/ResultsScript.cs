using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultsScript : MonoBehaviour
{
    public Text state;

    public Text scoreStat;
    public Text highScore, highScoreStat;

    public Text leastMoves, leastMovesStat;
    public Text movesStat;

    public SpaceBabyController spaceBaby;
    public Sprite spaceShipDebris;
    public GameObject spaceShip;

    // Start is called before the first frame update
    void Start()
    {
        // state
        if (Config.config.gameWin)
            state.text = Config.config.gameStateTxtEnglish[0].ToUpper();
        else
            state.text = Config.config.gameStateTxtEnglish[1].ToUpper();

        // score
        scoreStat.text = Config.config.score.ToString();

        // high score
        highScore.text = Config.config.currentDifficulty + " HIGH SCORE";

        int last = PlayerPrefs.GetInt(Config.config.currentDifficulty + "HighScore");
        if (Config.config.score > last)
        {
            highScore.color = Color.cyan;
            highScoreStat.color = Color.cyan;
            highScoreStat.text = Config.config.score.ToString();
        }
        else
            highScoreStat.text = last.ToString();


        // least moves
        last = PlayerPrefs.GetInt(Config.config.currentDifficulty + "Moves");
        if (Config.config.gameWin && Config.config.moveCounter < last)
        {
            leastMoves.color = Color.cyan;
            leastMovesStat.color = Color.cyan;
            leastMovesStat.text = Config.config.moveCounter.ToString();
        }
        else
            leastMovesStat.text = last.ToString();


        // moves
        movesStat.text = Config.config.moveCounter.ToString();


        // spacebaby
        if (Config.config.gameWin)
            spaceBaby.BabyWin(Config.config.matchCounter);
        else
        {
            spaceBaby.BabyLose();
            SpaceShipExplode();
        }

        UtilsScript.global.SetHighScores();
    }

    private bool exploded = false;
    public void SpaceShipExplode()
    {
        if (exploded)
            return;

        exploded = true;
        StartCoroutine(Explode());
    }

    IEnumerator Explode()
    {
        yield return new WaitForSeconds(2);
        Config.config.GetComponent<SoundController>().ExplosionSound();
        spaceShip.GetComponent<Image>().sprite = spaceShipDebris;
        spaceShip.transform.localScale = new Vector3(2, 2, 1);
    }
}
