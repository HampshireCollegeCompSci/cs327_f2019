using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUIScript : MonoBehaviour
{
	public void Restart()
	{
		Application.LoadLevel(0);//resets the level
		return;
	}

	public void SetGameMenu(GameObject gameMenu)
	{
        Debug.Log(gameMenu.activeSelf);

        if (gameMenu.activeSelf)
        {
            gameMenu.SetActive(false);
        }

        else if (!gameMenu.activeSelf)
        {
            gameMenu.SetActive(true);
        }
	}
}
