using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneLoader.instance.LoadActiveScene("GameHub");
    }

    public void OpenSettings()
    {
        // GameManager Open Settings
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
