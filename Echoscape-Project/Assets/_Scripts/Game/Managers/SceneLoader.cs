using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance { get; private set; }

    [Header("Scene Loading")]
    public UnityEvent SceneLoaded;
    public UnityAction<float> SceneLoadProgress;
    public List<ISceneLoadProcess> sceneLoadProcesses = new List<ISceneLoadProcess>();

    [SerializeField] private Coroutine loadingCoroutine;

    #region Unity Functions
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void OnEnable()
    {
    
    }

    private void OnDisable()
    {
        
    }
    #endregion

    #region Scene Loading
    public void LoadScene(string sceneName)
    {        
        loadingCoroutine = StartCoroutine(SceneTransitionHandler(sceneName));
    }
    #endregion

    #region Scene Transition
    private IEnumerator SceneTransitionHandler(string sceneName)
    {
        GameManager.instance.IsActive = false;
        DisablePlayers();

        SceneManager.LoadScene("TransitionScene", LoadSceneMode.Single);

        
        AsyncOperation desiredScene = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        desiredScene.allowSceneActivation = false;

        yield return new WaitUntil(() => desiredScene.isDone);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

        // Wait Until Possible Scene Gen Set-Up
        yield return new WaitUntil(() => EvaluateSceneInitProcesses());
        
        desiredScene.allowSceneActivation = true;
        GameManager.instance.IsActive = true;
        EnablePlayers();

        Debug.Log("Unloading Transition Scene");
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("TransitionScene"));

        SceneLoaded.Invoke();

        sceneLoadProcesses.Clear();
    }

    private bool EvaluateSceneInitProcesses()
    {
        if (sceneLoadProcesses.Count == 0) return true;

        foreach (ISceneLoadProcess sceneLoadProcess in sceneLoadProcesses)
        {
            if (sceneLoadProcess.FinishedProcess()) continue;
            else return false;
        }

        return true;
    }
    #endregion

    #region Player State Transitions
    private void EnablePlayers()
    {
        PlayerInputManager.instance.EnableJoining();
        PlayerManager.instance.ShowPlayers();
    }

    private void DisablePlayers()
    {
        PlayerInputManager.instance.DisableJoining();
        PlayerManager.instance.HidePlayers();
    }
    #endregion
}
