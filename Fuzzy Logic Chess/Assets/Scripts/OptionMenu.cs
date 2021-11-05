using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionMenu : MonoBehaviour
{
    [SerializeField] private Slider volume;
    [SerializeField] private Dropdown resolution;
    [SerializeField] private Toggle fullscreen;
    [SerializeField] private Button player1;
    [SerializeField] private Button player2;
    [SerializeField] private Button square1;
    [SerializeField] private Button square2;
    // Start is called before the first frame update
    void Start()
    {
        LoadFromChess();
    }

    public void ToggleOptionsMenu()
    {
        gameObject.SetActive(true);
    }

    public void SetVolume()
    {
        Chess.volume = volume.value;
    }
    public void SetResolution()
    {
        Chess.resolution = resolution.value;
        Chess.RefreshScreen();
    }
    public void SetFullescreen()
    {
        Chess.fullscreen = fullscreen.isOn;
        Chess.RefreshScreen();
    }

    public void SetPlayerOneColor()
    {
        Chess.IncrementPlayer1Color();
        player1.GetComponent<Image>().color = Chess.Colors.PLAYER_ONE;
    }

    public void SetPlayerTwoColor()
    {
        Chess.IncrementPlayer2Color();
        player2.GetComponent<Image>().color = Chess.Colors.PLAYER_TWO;
    }

    public void SetBlockOneColor()
    {
        Chess.IncrementBlock1Color();
        square1.GetComponent<Image>().color = Chess.Colors.BOARD_LIGHT;
    }

    public void SetBlockTwoColor()
    {
        Chess.IncrementBlock2Color();
        square2.GetComponent<Image>().color = Chess.Colors.BOARD_DARK;
    }
    public void SaveOptions()
    {
        Chess.SaveSetting();
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void RevertSettings()
    {
        Chess.LoadSetting();
        LoadFromChess();
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void RecolorTurnIndicator()
    {

    }

    private void LoadFromChess()
    {
        volume.value = Chess.volume;
        resolution.value = Chess.resolution;
        fullscreen.isOn = Chess.fullscreen;
        player1.GetComponent<Image>().color = Chess.Colors.PLAYER_ONE;
        player2.GetComponent<Image>().color = Chess.Colors.PLAYER_TWO;
        square1.GetComponent<Image>().color = Chess.Colors.BOARD_LIGHT;
        square2.GetComponent<Image>().color = Chess.Colors.BOARD_DARK;
    }
}