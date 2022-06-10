using UnityEngine;
using Unity.Jobs;
using Unity.Burst;

namespace TerrainGeneration
{
    [BurstCompile]
    [System.Serializable]
    public struct PlanetAttributes
    {
        [Tooltip("Planets Overall Size")]
        [Range(1, 1000)]
        public int terrainSize;

        [Tooltip("Amount of Chunks the Terrain is divided into")]
        [Range(1, 50)]
        public int numChunks;

        [Tooltip("Vertex Points Per Axis")]
        public int pointsPerAxis;

        [Range(0, 1)]
        [Tooltip("How Isometric the Terrain is")]
        public float isoLevel;        

        [Space]

        [Range(0.01f, 2f)]
        [Tooltip("Scale of the Noise Overall")]
        public float noiseScale;

        [Range(0.01f, 2f)]
        [Tooltip("Scale of the Noises Height")]
        public float noiseHeightMultiplier;

        public PlanetAttributes(int terrainSize, int numChunks, int pointsPerAxis, float isoLevel, float noiseHeightMultiplier, float noiseScale)
        {
            this.terrainSize = terrainSize;
            this.numChunks = numChunks;
            this.pointsPerAxis = pointsPerAxis;
            this.isoLevel = isoLevel;
            this.noiseHeightMultiplier = noiseHeightMultiplier;
            this.noiseScale = noiseScale;
        }
    }
}