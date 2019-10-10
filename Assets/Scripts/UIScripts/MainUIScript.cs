using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainUIScript : MonoBehaviour
{
    public void Restart()
    {
        SceneManager.LoadScene("MainMenuScene");//resets the level
        return;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");//resets the level
    }

    //This can be used for pause menu in the future.

    //public void SetGameMenu(GameObject gameMenu)
    //{
    //       Debug.Log(gameMenu.activeSelf);

    //       if (gameMenu.activeSelf)
    //       {
    //           gameMenu.SetActive(false);
    //       }

    //       else if (!gameMenu.activeSelf)
    //       {
    //           gameMenu.SetActive(true);
    //       }
    //}
}
