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

    [Header("Gravity")]
    [SerializeField] private Transform gravityCentre;
    public UnityEvent<Transform> GravityLocationChanged;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);

        GameObject.DontDestroyOnLoad(this.gameObject);
    }

    private void OnApplicationFocus(bool focus)
    {
        Cursor.lockState = focus ? CursorLockMode.Locked : CursorLockMode.None;
    }

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
}
