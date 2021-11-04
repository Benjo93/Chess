using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionMenu : MonoBehaviour
{
    [SerializeField] Slider volume;
    [SerializeField] Dropdown resolution;
    [SerializeField] Toggle fullscreen;
    // Start is called before the first frame update
    void Start()
    {
        LoadFromChess();
        volume.onValueChanged.AddListener(delegate { ValueChangeCheck(volume); });
        fullscreen.onValueChanged.AddListener(delegate { ToggleValueChanged(fullscreen); });
        resolution.onValueChanged.AddListener(delegate { DropdownValueChanged(resolution); });
        gameObject.SetActive(false);
    }

    private void ValueChangeCheck(Slider change)
    {
        Chess.volume = change.value;
    }
    private void DropdownValueChanged(Dropdown change)
    {
        Chess.resolution = change.value;
        Chess.RefreshScreen();
    }

    private void ToggleValueChanged(Toggle change)
    {
        Chess.fullscreen = change.isOn;
        Chess.RefreshScreen();
    }

    public void ToggleOptionsMenu()
    {
        Chess.SaveSetting();
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void RevertSettings()
    {
        Chess.LoadSetting();
        LoadFromChess();
    }

    private void LoadFromChess()
    {
        volume.value = Chess.volume;
        resolution.value = Chess.resolution;
        fullscreen.isOn = Chess.fullscreen;
    }
}

