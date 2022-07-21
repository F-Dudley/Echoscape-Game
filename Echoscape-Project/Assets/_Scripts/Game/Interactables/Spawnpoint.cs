using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnpoint : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    private void Start()
    {
        PlayerManager.instance?.MoveIntoPosition(spawnPoint);
        PlayerManager.instance?.EnablePlayerInput();
    }
}
