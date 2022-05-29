using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

using TerrainGeneration.ComputeStructs;

namespace TerrainGeneration
{
    namespace ComputeStructs
    {
        public struct Vertex
        {
            public float3 position;
            public float3 normal;
            public int2 id;
        }

        public struct Triangle
        {
            Vertex vertexA;
            Vertex vertexB;
            Vertex vertexC;
        }        
    }

    [BurstCompile]
    public struct MarchJob : IJobParallelFor
    {
        // Planet Attributes
        [ReadOnly] PlanetAttributes planetAttributes;
        [ReadOnly] NativeArray<ChunkAttributes> chunks;

        // Main Arrays
        NativeArray<Triangle> triangles;
        [ReadOnly] NativeArray<int> triangulationTable;

        // Density Texture
        
        // [ReadOnly] NativeArray<T> textureData;
        [ReadOnly] int textureSize;

        public MarchJob(NativeArray<ChunkAttributes> chunks, PlanetAttributes planetAttributes, int textureSize,
                        NativeArray<int> triangulationTable, NativeArray<Triangle> triangles)
        {
            this.planetAttributes = planetAttributes;
            this.chunks = chunks;
            this.triangles = triangles;
            this.triangulationTable = triangulationTable;

            this.textureSize = textureSize;
        }

        public void Execute(int index)
        {
            int numCubesPerAxis = planetAttributes.pointsPerChunk - 1;
            // if (index >= numCubesPerAxis) return;

            int3 coord = chunks[index].id * (planetAttributes.pointsPerChunk - 1);

            NativeArray<int3> cornerCoords = new NativeArray<int3>(8, Allocator.Temp);
            cornerCoords[0] = coord + new int3(0, 0, 0);
            cornerCoords[1] = coord + new int3(1, 0, 0);
            cornerCoords[2] = coord + new int3(1, 0, 1);
            cornerCoords[3] = coord + new int3(0, 0, 1);
            cornerCoords[4] = coord + new int3(0, 1, 0);
            cornerCoords[5] = coord + new int3(1, 1, 0);
            cornerCoords[6] = coord + new int3(1, 1, 1);
            cornerCoords[7] = coord + new int3(0, 1, 1);

            int cubeConfig = 0;
            for (int i = 0; i < 8; i++)
            {
                if (SampleDensityMap(cornerCoords[i]) < planetAttributes.isoLevel)
                {
                    cubeConfig |= (1 << i);
                }
            }

            cornerCoords.Dispose();
        }

        private Vertex CreateVertex(int3 coordA, int coordB)
        {
            float3 posA = CoordToWorld(coordA);
            float3 posB = CoordToWorld(coordB);
            float densityA = SampleDensityMap(coordA);
            float densityB = SampleDensityMap(coordB);

            // Point Interpolation
            float t = (planetAttributes.isoLevel - densityA) / (densityB - densityA);
            float3 vPosition = posA + t * (posB - posA);

            // Normals
            float3 normalA = CalculateNormal(coordA);
            float3 normalB = CalculateNormal(coordB);
            float3 vNormal = math.normalize((normalA + t * (normalB - normalA)));
        
            // IDs
            int indexA = IndexFromCoord(coordA);
            int indexB = IndexFromCoord(coordB);

            // Create Vertex
            return new Vertex
            {
                position = vPosition,
                normal = vNormal,
                id = new int2(math.min(indexA, indexB), math.max(indexA, indexB))
            };
        }

        // Changed Parameter as intial source had int3 so wanted to minimize casting.
        private float3 CoordToWorld(float3 coord)
        {
            return (coord / (textureSize - 1.0f) - 0.5f) * planetAttributes.terrainSize;
        }

        // Requires Chunk Implementation
        private int IndexFromCoord(int3 coord)
        {
            coord = coord;

            return coord.z * planetAttributes.pointsPerChunk * planetAttributes.pointsPerChunk + coord.y * planetAttributes.pointsPerChunk + coord.x;
        }

        // Figure out reading 3D RenderTexture Data. --------------------------------- NEED TO FINISh
        private float SampleDensityMap(int3 coord)
        {
            coord = math.max(0, math.min(coord, textureSize));

            return 0.0f;
        }

        private float3 CalculateNormal(int3 coord)
        {
            int3 offsetX = new int3(1, 0, 0);
            int3 offsetY = new int3(0, 1, 0);
            int3 offsetZ = new int3(0, 0, 1);

            float dx = SampleDensityMap(coord + offsetX) - SampleDensityMap(coord - offsetX);
            float dy = SampleDensityMap(coord + offsetY) - SampleDensityMap(coord - offsetY);
            float dz = SampleDensityMap(coord + offsetZ) - SampleDensityMap(coord - offsetZ);

            return math.normalize(new float3(dx, dy, dz));
        }
    }
    
}