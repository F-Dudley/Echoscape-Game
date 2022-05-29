using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

namespace TerrainGeneration
{
    [System.Serializable]
    [BurstCompile]
    public struct Chunk
    {
        public ChunkAttributes attributes;
        public GameObject meshHolder;

        public void CreateMesh()
        {
            
        }
    }

    [System.Serializable]
    [BurstCompile]
    public struct ChunkAttributes
    {
        public int3 id;
        public float3 centre;
        public float size;
    }
}