using UnityEngine;
using UnityEngine.UI;

public class MenuUIScript : MonoBehaviour
{
    [SerializeField]
    private Image spaceShip;
    [SerializeField]
    private Button spaceShipButton;
    [SerializeField]
    private Sprite spaceShipOff, spaceShipOn, debris;
    [SerializeField]
    private GameObject continueButton;
    [SerializeField]
    private GameObject explosionPrefab;

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
        GameObject explosion = Instantiate(explosionPrefab, spaceShip.gameObject.transform);
        explosion.transform.localScale = new Vector3(75, 75, 1);
        explosion.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));

        spaceShip.sprite = debris;
        SoundEffectsController.Instance.ExplosionSound();
        Config.Instance.SetDifficulty(Difficulties.cheat);
        NewGame(isCheating: true);
    }

    public void ResetSpaceShip()
    {
        spaceShip.sprite = spaceShipOff;
        spaceShipButton.interactable = true;
    }

    private void NewGame(Difficulty difficulty)
    {
        Config.Instance.SetDifficulty(difficulty);
        NewGame();
    }

    private void NewGame(bool isContinue = false, bool isTutorial = false, bool isCheating = false)
    {
        spaceShipButton.interactable = false;
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
