using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuFunctionality : MonoBehaviour
{
    [SerializeField] private GameObject firstSelected;

    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    public void StartGame()
    {
        SceneLoader.instance.LoadScene("GameHub");
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
