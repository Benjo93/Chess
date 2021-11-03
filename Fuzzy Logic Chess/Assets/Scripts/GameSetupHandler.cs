using System.IO;
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

    public BoardManager bm;
    public GameManager gm;

    public Toggle asWhite, asBlack, asRandom;
    public Dropdown gameTypeSelection;
    public Button playButton;
    public bool gameInProgress = false;
    public Text buttonText;
    public ToggleGroup playAs;
    public string[] setupChoices = new string[2];

    public void OnPlayButtonClick()
    {
        if (gameInProgress == true) // this is the actual New Game function
        {
            Debug.Log("A");
            gm.SetAutoSaveActive(false);
            gm.EraseSave();
            gm.StartGame();
            gm.SetAutoSaveActive(true);
            buttonText.text = "PLAY";
        }
        else // this will be called first after first click and will turn "Play" into "New Game" 
        {
            Debug.Log("B");
            GetGameSetup();
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

        string game_type_choice = gameTypeSelection.options[gameTypeSelection.value].text;
        string[] choices = game_type_choice.Split(new string[] { " vs. " }, System.StringSplitOptions.RemoveEmptyEntries);

        Session.players[0] = choices[0].ToLower();
        Session.players[1] = choices[1].ToLower();
    }
}