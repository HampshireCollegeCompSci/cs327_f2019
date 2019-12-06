using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIScript : MonoBehaviour
{
    private IEnumerator coroutine;

    private void Start()
    {
        //stop animation from start
        Animator[] animators = gameObject.GetComponentsInChildren<Animator>();
        foreach (Animator anim in animators)
            anim.enabled = false;


        if (GameObject.Find("Spacebaby Loading") != null)
            GameObject.Find("Spacebaby Loading").GetComponent<Animator>().enabled = true;

        //update main menu button txt
        if (GameObject.Find("Play") != null)
        {
            GameObject button = GameObject.Find("Play");
            button.GetComponentInChildren<Text>().text = Config.config.menuSceneButtonsTxtEnglish[0].ToUpper();
        }
        if (GameObject.Find("Tutorial") != null)
        {
            GameObject button = GameObject.Find("Tutorial");
            button.GetComponentInChildren<Text>().text = Config.config.menuSceneButtonsTxtEnglish[1].ToUpper();
        }
        if (GameObject.Find("Credits") != null)
        {
            GameObject button = GameObject.Find("Credits");
            button.GetComponentInChildren<Text>().text = Config.config.menuSceneButtonsTxtEnglish[2].ToUpper();
        }
        if (GameObject.Find("Loading") != null)
        {
            GameObject txt = GameObject.Find("Loading");
            txt.GetComponent<Text>().text = Config.config.loadingSceneTxtEnglish.ToUpper();
        }

        //update level txt
        if (GameObject.Find("Easy") != null)
        {
            GameObject button = GameObject.Find("Easy");
            button.GetComponentInChildren<Text>().text = Config.config.levelSceneButtonsTxtEnglish[0].ToUpper();
        }
        if (GameObject.Find("Normal") != null)
        {
            GameObject button = GameObject.Find("Normal");
            button.GetComponentInChildren<Text>().text = Config.config.levelSceneButtonsTxtEnglish[1].ToUpper();
        }
        if (GameObject.Find("Hard") != null)
        {
            GameObject button = GameObject.Find("Hard");
            button.GetComponentInChildren<Text>().text = Config.config.levelSceneButtonsTxtEnglish[2].ToUpper();
        }
        // update return txt
        if (GameObject.Find("Return") != null)
        {
            GameObject button = GameObject.Find("Return");
            button.GetComponentInChildren<Text>().text = Config.config.levelSceneButtonsTxtEnglish[3].ToUpper();
        }
        //update pause menu txt
        if (GameObject.Find("Resume") != null)
        {
            GameObject button = GameObject.Find("Resume");
            button.GetComponentInChildren<Text>().text = Config.config.pauseSceneButtonsTxtEnglish[0].ToUpper();
        }
        if (GameObject.Find("Restart") != null)
        {
            GameObject button = GameObject.Find("Restart");
            button.GetComponentInChildren<Text>().text = Config.config.pauseSceneButtonsTxtEnglish[1].ToUpper();
        }
        if (GameObject.Find("Settings") != null)
        {
            GameObject button = GameObject.Find("Settings");
            button.GetComponentInChildren<Text>().text = Config.config.pauseSceneButtonsTxtEnglish[2].ToUpper();
        }
        //update summary txt
        if (GameObject.Find("MainMenu") != null)
        {
            GameObject button = GameObject.Find("MainMenu");
            button.GetComponentInChildren<Text>().text = Config.config.summarySceneButtonsTxtEnglish[0].ToUpper();
        }
        if (GameObject.Find("PlayAgain") != null)
        {
            GameObject button = GameObject.Find("PlayAgain");
            button.GetComponentInChildren<Text>().text = Config.config.summarySceneButtonsTxtEnglish[1].ToUpper();
        }

    }

    public void Play()
    {
        coroutine = ButtonPressedAnim(GameObject.Find("Play"), "LevelSelectScene");
        StartCoroutine(coroutine);
    }

    public void NewGame()
    {
        Config.config.gameOver = false;
        Config.config.gameWin = false;
        Config.config.gamePaused = false;
        Config.config.GetComponent<MusicController>().LoadGap();

        if (Config.config.difficulty == "easy")
            coroutine = ButtonPressedAnim(GameObject.Find("Easy"), "LoadingScene");
        else if (Config.config.difficulty == "medium")
            coroutine = ButtonPressedAnim(GameObject.Find("Normal"), "LoadingScene");
        else
            coroutine = ButtonPressedAnim(GameObject.Find("Hard"), "LoadingScene");

        StartCoroutine(coroutine);

    }

    public void UndoButton()
    {
        UndoScript.undoScript.undo();
        Config.config.GetComponent<SoundController>().ButtonPressSound2();

        Animator undoAnim = GameObject.Find("Undo").GetComponentInChildren<Animator>();
        if (!undoAnim.enabled)
            undoAnim.enabled = true;
        else
            undoAnim.Play("");

    }

    public void PlayAgain()
    {
        Config.config.GetComponent<MusicController>().GameMusic();
        Config.config.gameOver = false;
        Config.config.gameWin = false;
        Config.config.gamePaused = false;

        coroutine = ButtonPressedAnim(GameObject.Find("PlayAgain"), "GameplayScene");
        StartCoroutine(coroutine);
    }

    public void Restart()
    {
        Config.config.GetComponent<MusicController>().GameMusic();
        Config.config.gameOver = false;
        Config.config.gameWin = false;
        Config.config.gamePaused = false;

        coroutine = ButtonPressedAnim(GameObject.Find("Restart"), "GameplayScene");
        StartCoroutine(coroutine);
    }

    public void MainMenu()
    {
        Config.config.gamePaused = false;
        if (Config.config != null)
        {
            Config.config.gameOver = false;
            Config.config.gameWin = false;
        }

        coroutine = ButtonPressedAnim(GameObject.Find("MainMenu"), "MainMenuScene");
        StartCoroutine(coroutine);

        Config.config.GetComponent<MusicController>().MainMenuMusic();
    }


    //possibly be renamed to settings
    public void Settings()
    {
        coroutine = ButtonPressedAnim(GameObject.Find("Settings"), "SoundScene", true);
        StartCoroutine(coroutine);
    }

    public void Credits()
    {
        coroutine = ButtonPressedAnim(GameObject.Find("Credits"), "CreditScene");
        StartCoroutine(coroutine);
    }

    public void PauseGame()
    {
        Config.config.GetComponent<SoundController>().PauseMenuButtonSound();
        //TODO save the game scene
        Config.config.gamePaused = true;
        SceneManager.LoadScene("PauseScene", LoadSceneMode.Additive);

    }

    public void ResumeGame()
    {
        Config.config.gamePaused = false;
        //TODO load the saved game scene then uncomment the above code
        coroutine = ButtonPressedAnim(GameObject.Find("Resume"), "PauseScene", false, true);
        StartCoroutine(coroutine);
    }

    public void Return()
    {
        //Config.config.GetComponent<SoundController>().ButtonPressSound();
        if (Config.config.gamePaused)
        {
            coroutine = ButtonPressedAnim(GameObject.Find("Return"), "SoundScene", false, true);
        }

        else
        {
            coroutine = ButtonPressedAnim(GameObject.Find("Return"), "MainMenuScene");
        }
        StartCoroutine(coroutine);
    }

    public void Tutorial()
    {
        coroutine = ButtonPressedAnim(GameObject.Find("Tutorial"), "TutorialScene");
        StartCoroutine(coroutine);
    }

    public void HardDifficulty()
    {
        Config.config.setDifficulty("hard");
        NewGame();
    }
    public void EasyDifficulty()
    {
        Config.config.setDifficulty("easy");
        NewGame();
    }
    public void MediumDifficulty()
    {
        Config.config.setDifficulty("medium");
        NewGame();
    }

    public void MakeActionsMax()
    {
        Config.config.deck.GetComponent<DeckScript>().NextCycle(manuallyTriggered: true);
    }

    IEnumerator ButtonPressedAnim(GameObject button, string scene, bool additive = false, bool unload = false)
    {
        Config.config.GetComponent<SoundController>().ButtonPressSound();
        button.GetComponentInChildren<Animator>().enabled = true;

        for (float ft = 0.2f; ft >= 0; ft -= 0.1f)
        {
            yield return new WaitForSeconds(0.08f);
        }

        button.GetComponentInChildren<Animator>().enabled = false;

        if (!additive && !unload)
            SceneManager.LoadScene(scene);
        else if (additive)
            SceneManager.LoadScene(scene, LoadSceneMode.Additive);
        else if (unload)
            SceneManager.UnloadSceneAsync(scene);
    }
}
