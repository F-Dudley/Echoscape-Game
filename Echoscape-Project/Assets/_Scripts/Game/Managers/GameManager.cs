using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [SerializeField] private bool isActive = false;
    public bool IsActive 
    { 
        get => isActive; 
        set => isActive = value;
    }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);

        GameObject.DontDestroyOnLoad(this.gameObject);
    }
}
