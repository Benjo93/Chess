using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSelect : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Chess");
    }
    public void NewGame()
    {
        SceneManager.LoadScene("Game Setup");
    }
    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Start Menu 2");
    }
    public void Credits()
    {
        SceneManager.LoadScene("Credits");
    }
    public void Quit()
    {
        Application.Quit();
    }
}
