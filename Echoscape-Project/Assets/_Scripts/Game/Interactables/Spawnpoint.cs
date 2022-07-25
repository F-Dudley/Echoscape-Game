using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnpoint : MonoBehaviour
{
    [SerializeField] private bool RunOnStart;
    [SerializeField] private Transform spawnPoint;

    private void Start()
    {
        if (RunOnStart)
        {
            MovePlayersToPoint();
        }
    }

    public void MovePlayersToPoint()
    {
        PlayerManager.instance?.MoveIntoPosition(spawnPoint);
        PlayerManager.instance?.EnablePlayerInput();
    }
}
