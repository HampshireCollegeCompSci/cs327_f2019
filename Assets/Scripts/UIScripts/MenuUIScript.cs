using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIScript : MonoBehaviour
{
    [SerializeField]
    private Image spaceShip;
    [SerializeField]
    private Button spaceShipButton;
    [SerializeField]
    private Sprite spaceShipOn, debris;
    [SerializeField]
    private GameObject continueButton;

    public void Play()
    {
        //Preprocessor Directive to make builds work
        #if (UNITY_EDITOR)
                UnityEditor.AssetDatabase.Refresh();
        #endif

        continueButton.SetActive(SaveFile.Exists());
    }

    public void Tutorial()
    {
        NewGame(isTutorial: true);
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

            NewGame(isContinue: true);
        }
    }

    public void CheatMode()
    {
        spaceShipButton.interactable = false;
        spaceShip.sprite = debris;
        SoundEffectsController.Instance.ExplosionSound();
        Config.Instance.SetDifficulty(Difficulties.cheat);
        NewGame(isCheating: true);
    }

    private void NewGame(Difficulty difficulty)
    {
        Config.Instance.SetDifficulty(difficulty);
        NewGame();
    }

    private void NewGame(bool isContinue = false, bool isTutorial = false, bool isCheating = false)
    {
        Config.Instance.continuing = isContinue;
        Config.Instance.SetTutorialOn(isTutorial);

        if (!isCheating)
        {
            spaceShip.sprite = spaceShipOn;
        }

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
