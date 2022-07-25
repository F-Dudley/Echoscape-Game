using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

[BurstCompile]
public struct Bullet
{
    public float3 position;
    public float3 direction;

    public Bullet(float3 position, float3 direction)
    {
        this.position = position;
        this.direction = direction;
    }
}

public struct BulletTransform : IJobParallelFor
{
    private NativeArray<Bullet> bullets;
    private NativeArray<float4x4> bulletMatricies;

    public BulletTransform(NativeArray<Bullet> bullets, NativeArray<float4x4> bulletMatricies)
    {
        this.bullets = bullets;
        this.bulletMatricies = bulletMatricies;
    }

    public void Execute(int index)
    {
        Bullet bullet = bullets[index];

        bullet.position += bullet.direction * 2f;

        float4x4 newMatrix = float4x4.TRS(bullet.position, quaternion.Euler(bullet.direction), new float3(1, 1, 1));

        bullets[index] = bullet;
        bulletMatricies[index] = newMatrix;
    }
}

public class BulletManager : MonoBehaviour
{
    public static BulletManager instance;

    [Header("Current Bullets")]
    private NativeList<Bullet> bullets;
    private NativeList<float4x4> bulletMatrices;

    [Header("Bullet Render Components")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Mesh bulletMesh;
    [SerializeField] private Material bulletMat;

    [Header("Jobs")]
    JobHandle transformJob;

    #region Unity Functions
    private void Start()
    {
        instance = this;

        bullets = new NativeList<Bullet>(100, Allocator.Persistent);
        bulletMatrices = new NativeList<float4x4>(100, Allocator.Persistent);
    }

    private void Update()
    {
        BulletTransform bulletJob = new BulletTransform(bullets, bulletMatrices);
        transformJob = bulletJob.Schedule(bullets.Length, 100);
        transformJob.Complete();

        Matrix4x4[] converts = new Matrix4x4[bulletMatrices.Length];

        for (int i = 0; i < bulletMatrices.Length; i++)
        {
            converts[i] = bulletMatrices[i];
        }

        NativeArray<Matrix4x4> reinterprets = bulletMatrices.AsArray().Reinterpret<Matrix4x4>();

        Graphics.DrawMeshInstanced(bulletMesh, 0, bulletMat, reinterprets.ToArray());
    }

    private void OnDestroy()
    {
        bullets.Dispose();
        bulletMatrices.Dispose();
    }
    #endregion

    #region Manager Functions
    public void SpawnBullet(Transform spawnPoint)
    {
        Bullet newBullet = new Bullet(spawnPoint.position, spawnPoint.forward);
        bullets.Add(newBullet);
    }
    #endregion
}
