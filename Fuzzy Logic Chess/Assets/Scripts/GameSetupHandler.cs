using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class GameSetupHandler : MonoBehaviour
{
    /*
     * Contains Functions to handle the game setup
     * OnPlayButtonClick() - handles UI animation when game starts or new game begins
     * GetGameSetup() - stores game choices into public array setupChoices[]
     */



    public Toggle asWhite, asBlack, asRandom;
    public Dropdown gameTypeSelection;
    public Button playButton;
    public bool gameInProgress = false;
    public Text buttonText;
    public ToggleGroup playAs;
    public string[] setupChoices = new string[2];


    public void OnPlayButtonClick()
    {
        if (gameInProgress == true)
        {
            buttonText.text = "PLAY";
        }
        else
        {
            buttonText.text = "NEW GAME";

        }
        gameTypeSelection.interactable = gameInProgress;
        asWhite.interactable = gameInProgress;
        asBlack.interactable = gameInProgress;
        asRandom.interactable = gameInProgress;
        gameInProgress = !gameInProgress;
    }


    public void GetGameSetup()
    {
        setupChoices[0] = gameTypeSelection.options[gameTypeSelection.value].text;

        Toggle selectedToggle = playAs.ActiveToggles().FirstOrDefault();
        setupChoices[1] = selectedToggle.name;

        foreach (string x in setupChoices)
        {
            Debug.Log(x);
        }
    }
}
