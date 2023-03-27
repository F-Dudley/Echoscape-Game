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
        }
    }

    [SerializeField] private List<PlayerInput> playerInputs;

    [Header("Scene Grouping")]
    [SerializeField] private Transform playerHolder;
    [SerializeField] private Transform playerHoldingCell;

    #region Unity Functions
    private void Awake()
    {
        if (instance == null) instance = this;

        DontDestroyOnLoad(playerHolder);
        PlayerInput[] players = playerHolder.GetComponentsInChildren<PlayerInput>();
        foreach (PlayerInput player in players)
        {
            playerInputs.Add(player);
            player.DeactivateInput();
        }
    }
    #endregion

    #region Player Scene Loading

    #endregion

    #region Player Management
    public void EnablePlayerInput()
    {
        foreach (PlayerInput pInput in playerInputs)
        {
            pInput.ActivateInput();
        }
    }

    public void DisablePlayerInput()
    {
        foreach (PlayerInput pInput in playerInputs)
        {
            pInput.DeactivateInput();
        }
    }

    public void ChangePlayerActionMap(string actionMapName)
    {
        foreach (PlayerInput pInput in playerInputs)
        {
            pInput.SwitchCurrentActionMap(actionMapName);
        }
    }

    public void MoveIntoHolding()
    {
        foreach (Transform player in playerHolder)
        {
            player.position = playerHoldingCell.position;
        }
    }

    public void MoveIntoPosition(Transform spawnPos)
    {
        foreach (Transform player in playerHolder)
        {
            player.position = spawnPos.position;
            player.rotation = spawnPos.rotation;
        }
    }

    public Transform GetPlayerTransform()
    {
        return playerInputs[0].transform;
    }

    public Vector3 GetPositionNearPlayer()
    {
        Transform playerTransform = playerInputs[0].transform;

        return playerTransform.position + (playerTransform.up * 50.0f) + (playerTransform.forward * Random.Range(-50, 50)) + (playerTransform.right * Random.Range(-30, 30.0f));
    }
    #endregion
}
