using UnityEngine;
using UnityEngine.UI;

public class MenuUIScript : MonoBehaviour
{
    [SerializeField]
    private Image spaceShip;
    [SerializeField]
    private Sprite spaceShipOn, debris;
    [SerializeField]
    private GameObject continueButton;
    [SerializeField]
    private GameObject explosionPrefab;

    public void SetContinueButton()
    {
        continueButton.SetActive(SaveFile.Exists());
    }

    public void ShortTutorial()
    {
        Debug.Log("loading short tutorial");
        NewTutorial(Constants.Tutorial.tutorialShortCommandsFileName);
    }

    public void LongTutorial()
    {
        Debug.Log("loading long tutorial");
        NewTutorial(Constants.Tutorial.tutorialLongCommandsFileName);
    }

    public void HardDifficulty()
    {
        NewGame(Difficulties.hard);
    }

    public void MediumDifficulty()
    {
        NewGame(Difficulties.medium);
    }

    public void EasyDifficulty()
    {
        NewGame(Difficulties.easy);
    }

    public void Continue()
    {
        if (SaveFile.Exists())
        {
            #if (UNITY_EDITOR)
                UnityEditor.AssetDatabase.Refresh();
            #endif

            Config.Instance.continuing = true;
            spaceShip.sprite = spaceShipOn;
            TryStartGame();
        }
        else
        {
            continueButton.SetActive(false);
            Debug.LogWarning("the continue button was pressed even though there was no save file", continueButton);
        }
    }

    public void CheatMode()
    {
        GameObject explosion = Instantiate(explosionPrefab, spaceShip.gameObject.transform);
        explosion.transform.localScale = new Vector3(75, 75, 1);
        explosion.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));

        SoundEffectsController.Instance.ExplosionSound();
        NewGame(Difficulties.cheat);
    }

    private void NewTutorial(string tutorialCommandsFileToLoad)
    {
        Config.Instance.SetTutorialOn(tutorialCommandsFileToLoad);
        spaceShip.sprite = spaceShipOn;
        TryStartGame();
    }

    private void NewGame(Difficulty difficulty)
    {
        spaceShip.sprite = difficulty.Equals(Difficulties.cheat) ? debris : spaceShipOn;
        Config.Instance.SetDifficulty(difficulty);
        Config.Instance.SetTutorialOff();
        TryStartGame();
    }

    private void TryStartGame()
    {
        if (StartGameSequence.Instance != null)
        {
            StartGameSequence.Instance.StartLoadingGame();
        }
        else
        {
            throw new System.NullReferenceException("A sequence Instance does not exist!");
        }
    }
}
