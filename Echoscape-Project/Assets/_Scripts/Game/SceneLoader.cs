using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance { get; private set; }

    [SerializeField] private Coroutine loadingCoroutine;

    #region Unity Functions
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += GameSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= GameSceneLoaded;
    }
    #endregion

    #region Scene Loading
    public void LoadActiveScene(string sceneName)
    {
        loadingCoroutine = StartCoroutine(PerformSceneTransition(sceneName));
    }

    public void UnloadActiveScene(string sceneName)
    {
        SceneManager.UnloadSceneAsync(sceneName);
    }
    #endregion

    #region Scene Transition
    private IEnumerator PerformSceneTransition(string desiredScene)
    {
        Scene currScene = SceneManager.GetActiveScene();

        if (currScene.name.Equals(desiredScene)) yield break;

        SceneManager.LoadScene("TransitionScene");
        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(desiredScene, LoadSceneMode.Additive);

        while (!loadingOperation.isDone)
        {
            Debug.Log($"Loading Progress: {loadingOperation.progress}%");
            yield return null;
        }
        SceneManager.UnloadSceneAsync("TransitionScene");

        Scene desiredLoadedScene = SceneManager.GetSceneByName(desiredScene);
        SceneManager.SetActiveScene(desiredLoadedScene);
    }
    #endregion

    #region Player State Transitions
    private void GameSceneLoaded(Scene loadedScene, LoadSceneMode sceneMode)
    {
        switch (loadedScene.name)
        {
            case "MainMenu":
            case "TransitionScene":
                PlayerInputManager.instance.DisableJoining();
                PlayerManager.instance.HidingPlayers = true;
                break;

            default:
                PlayerInputManager.instance.EnableJoining();
                PlayerManager.instance.HidingPlayers = false;
                break;
        }
    }
    #endregion
}
