using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance { get; private set; }

    [Header("Players")]
    private bool hidingPlayers = true;
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

        playerHolder = new GameObject("PlayerHolder").transform;
        playerHolder.parent = this.transform;
    }
    #endregion

    #region Player Management
    private void OnPlayerJoined(PlayerInput pInput)
    {
        GameObject playerRoot = pInput.transform.root.gameObject;
        players.Add(playerRoot);

        playerRoot.transform.parent = playerHolder;
    }

    private void OnPlayerLeft(PlayerInput pInput)
    { 
        
    }
    
    public void ShowPlayers()
    {
        hidingPlayers = false;

        foreach (GameObject player in players)
        {
            player.SetActive(true);
        }
    }

    public void HidePlayers()
    {
        hidingPlayers = true;

        foreach (GameObject player in players)
        {
            player.SetActive(false);
        }
    }
    #endregion
}
