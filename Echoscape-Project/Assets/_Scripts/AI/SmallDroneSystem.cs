using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallDroneSystem : EnemySystem
{
    public static SmallDroneSystem instance;

    [Header("Pooling")]
    [SerializeField] protected GameObject systemPrefab;
    [SerializeField] protected Stack<PhysicsAgent> pool = new Stack<PhysicsAgent>();

    [Header("References")]
    [SerializeField] private Transform droneHolder;
    [SerializeField] private Transform playerTarget;

    [Space]

    [SerializeField] private TerrainGeneration.TerrainGenerator terrainGenerator;

    #region Unity Functions
    private void Start()
    {
        playerTarget = PlayerManager.instance.GetPlayerTransform();

        AddToPool(10);
    }
    private void Update()
    {
        
    }
    #endregion

    #region System Functions
    public void Spawn()
    {
        PhysicsAgent agent = PopFromPool();

        agent.SetPosition(PlayerManager.instance.GetPositionNearPlayer());
        agent.ResetVelocity();

        agent.gameObject.SetActive(true);
    }
    #endregion

    #region Pooling Functions
    private PhysicsAgent PopFromPool()
    {
        if (pool.Count == 0) AddToPool();

        return pool.Pop();
    }

    private void AddToPool()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject instantiatedPrefab = Instantiate<GameObject>(systemPrefab, droneHolder);
            instantiatedPrefab.SetActive(false);

            PhysicsAgent instantiatedAgent = instantiatedPrefab.GetComponent<PhysicsAgent>();

            instantiatedAgent.SetTarget(playerTarget);
            instantiatedAgent.SetParentSystem(this);

            pool.Push(instantiatedAgent);
        }
    }

    private void AddToPool(int number)
    {
        for (int i = 0; i < number; i++)
        {
            GameObject instantiatedPrefab = Instantiate<GameObject>(systemPrefab, droneHolder);
            instantiatedPrefab.SetActive(false);

            PhysicsAgent instantiatedAgent = instantiatedPrefab.GetComponent<PhysicsAgent>();

            instantiatedAgent.SetTarget(playerTarget);
            instantiatedAgent.SetParentSystem(this);

            pool.Push(instantiatedAgent);
        }
    }

    public void ReturnToPool(PhysicsAgent agent)
    {
        if (!pool.Contains(agent))
        {
            pool.Push(agent);
        }
    }
    #endregion
}
