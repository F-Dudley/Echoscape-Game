using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [SerializeField] private bool isActive = false;
    public bool IsActive 
    { 
        get => isActive; 
        set => isActive = value;
    }

    [Header("Game Time")]
    [SerializeField] private float timeActive;

    [Header("Gravity")]
    [SerializeField] private Transform gravityCentre;
    public UnityEvent<Transform> GravityLocationChanged;

    [Header("Options Menu")]
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject optionsFirstSelected;

    #region Unity Functions
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);

        GameObject.DontDestroyOnLoad(this.gameObject);
    }

    private void FixedUpdate()
    {
        if (isActive) timeActive += 1f * Time.fixedDeltaTime;
    }

    private void OnApplicationFocus(bool focus)
    {
        Cursor.lockState = focus ? CursorLockMode.Locked : CursorLockMode.None;
    }
    #endregion

    #region Game Functions
    private void UpdateGameTime()
    {
        
    }
    #endregion

    #region Gravity Functions
    public void SetGravityCentre(Transform gravityCentre)
    {
        this.gravityCentre = gravityCentre;
        GravityLocationChanged.Invoke(gravityCentre);
    }

    public Transform GetGravityCentre()
    {
        return gravityCentre;
    }
    #endregion

    #region Options Menu
    public void ToggleOptions()
    {
        if (optionsMenu.activeSelf) CloseOptions();
        else OpenOptions();
    }

    public void OpenOptions()
    {
        optionsMenu.SetActive(true);
        GlobalCanvasController.instance.AddInstantiatedMenu(ref optionsMenu);

        Time.timeScale = 0.0f;
    }

    public void CloseOptions()
    {
        optionsMenu.SetActive(false);
        GlobalCanvasController.instance.RemoveMenu(ref optionsMenu);
        PlayerManager.instance.ChangePlayerActionMap("Player");

        Time.timeScale = 1.0f;
    }

    public void ChangedQuality(System.Int32 newQuality)
    {
        QualitySettings.SetQualityLevel(newQuality);
    }
    #endregion
}

