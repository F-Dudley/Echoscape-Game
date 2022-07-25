using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuFunctionality : MonoBehaviour
{
    public void StartGame()
    {
        SceneLoader.instance.LoadScene("GameHub");
    }

    public void OpenSettings()
    {
        GameManager.instance.OpenOptions();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
