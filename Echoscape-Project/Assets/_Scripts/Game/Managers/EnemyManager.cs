using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Systems")]
    [SerializeField] private SmallDroneSystem smallDroneSystem;

    private Coroutine spawnCycleCoroutine;

    #region Unity Functions
    private void Awake()
    {
        smallDroneSystem = GetComponentInChildren<SmallDroneSystem>();

        if (spawnCycleCoroutine == null)
        {
            spawnCycleCoroutine = StartCoroutine(SpawnCycle());
        }
    }
    #endregion

    #region Spawning

    private IEnumerator SpawnCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1, 5));

           smallDroneSystem.Spawn();
        }

        yield return null;
    }

    #endregion
}