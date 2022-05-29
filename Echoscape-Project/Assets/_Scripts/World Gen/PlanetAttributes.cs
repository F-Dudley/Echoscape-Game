using UnityEngine;
using Unity.Jobs;
using Unity.Burst;

namespace TerrainGeneration
{
    [BurstCompile]
    [System.Serializable]
    public struct PlanetAttributes
    {
        public int terrainSize;
        public int numChunks;        
        public int pointsPerChunk;
        public float isoLevel;        

        [Space]

        public float noiseScale;
        public float noiseHeightMultiplier;

        public PlanetAttributes(int terrainSize, int numChunks, int pointsPerChunk, float isoLevel, float noiseHeightMultiplier, float noiseScale)
        {
            this.terrainSize = terrainSize;
            this.numChunks = numChunks;
            this.pointsPerChunk = pointsPerChunk;
            this.isoLevel = isoLevel;
            this.noiseHeightMultiplier = noiseHeightMultiplier;
            this.noiseScale = noiseScale;
        }
    }
}