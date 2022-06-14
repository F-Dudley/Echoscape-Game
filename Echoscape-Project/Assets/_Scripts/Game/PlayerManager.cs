using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance { get; private set; }

    [Header("Players")]
    private bool hidingPlayers = false;
    public bool HidingPlayers
    {
        get { return hidingPlayers; }
        set
        {
            hidingPlayers = value;
            if (value) HidePlayers();
            else ShowPlayers();
        }
    }
    [SerializeField] private List<GameObject> players = new List<GameObject>();

    [Header("Scene Grouping")]
    [SerializeField] private Transform playerHolder;

    #region Unity Functions
    private void Awake()
    {
        if (instance == null) instance = this;

        GameObject.DontDestroyOnLoad(playerHolder.gameObject);
    }
    #endregion

    #region Player Management
    public void ShowPlayers()
    {
        foreach (GameObject player in players)
        {
            player.SetActive(true);
        }
    }

    public void HidePlayers()
    {
        foreach (GameObject player in players)
        {
            player.SetActive(false);
        }
    }

    private void OnPlayerJoined(PlayerInput pInput)
    {
        GameObject playerRoot = pInput.transform.root.gameObject;
        GameObject.DontDestroyOnLoad(playerRoot);

        players.Add(playerRoot);
    }

    private void OnPlayerLeft(PlayerInput pInput)
    { 
    
    }
    #endregion
}
