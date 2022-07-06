using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneLoader.instance.LoadScene("GameHub");
    }

    public void StartDemoGame()
    {
        SceneLoader.instance.LoadScene("DemoScene");
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
