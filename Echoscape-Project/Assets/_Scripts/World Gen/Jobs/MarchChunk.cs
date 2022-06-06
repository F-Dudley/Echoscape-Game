using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;

using TerrainGeneration.ComputeStructs;

namespace TerrainGeneration
{
    namespace ComputeStructs
    {
        [BurstCompile]
        public struct Vertex
        {
            public float3 position;
            public float3 normal;
            public int2 id;
        }

        [BurstCompile]
        public struct Triangle
        {
            public Vertex vertexA;
            public Vertex vertexB;
            public Vertex vertexC;
        }
    }

    [BurstCompile]
    public struct MarchChunk : IJobParallelFor
    {
        // Planet
        [ReadOnly] PlanetAttributes planetAttributes;
        [ReadOnly] ChunkAttributes chunkAttributes;

        // March tables
        [ReadOnly] NativeArray<int> triangulationTable;
        [ReadOnly] NativeArray<int> cornerIndexAFromEdge;
        [ReadOnly] NativeArray<int> cornerIndexBFromEdge;

        // Density Texture
        [ReadOnly] NativeArray<float> textureData;
        [ReadOnly] int textureSize;
        [ReadOnly] int textureLayerOffset;

        // Other
        [ReadOnly] NativeArray<int3> ids;

        // Outputs
        NativeList<Triangle>.ParallelWriter triangles;

        public MarchChunk(PlanetAttributes planetAttributes, ChunkAttributes chunkAttributes,
                          NativeArray<int3> ids,
                          NativeArray<int> triangulationTable, NativeArray<int> cornerIndexATable, NativeArray<int> cornerIndexBTable,
                          NativeArray<float> textureData, int textureSize, int textureLayerOffset,
                          NativeList<Triangle>.ParallelWriter trianlgesWriter)
        {
            this.planetAttributes = planetAttributes;
            this.chunkAttributes = chunkAttributes;
            this.ids = ids;

            this.triangulationTable = triangulationTable;
            this.cornerIndexAFromEdge = cornerIndexATable;
            this.cornerIndexBFromEdge = cornerIndexBTable;

            this.textureData = textureData;
            this.textureSize = textureSize;
            this.textureLayerOffset = textureLayerOffset;

            this.triangles = trianlgesWriter;
        }

        public void Execute(int index)
        {
            int numCubesPerAxis = planetAttributes.pointsPerAxis - 1;
            int3 coord = ids[index] + chunkAttributes.GetChunkCoord(numCubesPerAxis);

            // Calculate Coords of Current Cubes Corners
            NativeArray<int3> cornerCoords = new NativeArray<int3>(8, Allocator.Temp);
            cornerCoords[0] = coord + new int3(0, 0, 0);
            cornerCoords[1] = coord + new int3(1, 0, 0);
            cornerCoords[2] = coord + new int3(1, 0, 1);
            cornerCoords[3] = coord + new int3(0, 0, 1);
            cornerCoords[4] = coord + new int3(0, 1, 0);
            cornerCoords[5] = coord + new int3(1, 1, 0);
            cornerCoords[6] = coord + new int3(1, 1, 1);
            cornerCoords[7] = coord + new int3(0, 1, 1);

            // Determine Cube Configuration - Cube configuration always comes back as 0;
            int cubeConfiguration = 0;
            for (int i = 0; i < 8; i++)
            {
                if (SampleDensity(cornerCoords[i]) < planetAttributes.isoLevel)
                {
                    cubeConfiguration |= (1 << i);
                }
            }
            //Debug.Log("Determining Cube Config: " + cubeConfiguration);

            int configIndex = cubeConfiguration * 16;
            // Create Triangles for Cube Config
            for (int i = 0; i < 16; i += 3)
            {
                if (triangulationTable[configIndex + i] == -1)
                {
                    break;
                }

                int edgeIndexA = triangulationTable[configIndex + i];
                int a0 = cornerIndexAFromEdge[edgeIndexA];
                int a1 = cornerIndexBFromEdge[edgeIndexA];

                int edgeIndexB = triangulationTable[configIndex + (i + 1)];
                int b0 = cornerIndexAFromEdge[edgeIndexB];
                int b1 = cornerIndexBFromEdge[edgeIndexB];

                int edgeIndexC = triangulationTable[configIndex + (i + 2)];
                int c0 = cornerIndexAFromEdge[edgeIndexC];
                int c1 = cornerIndexBFromEdge[edgeIndexC];

                // Get Vertex Positions
                Vertex vA = CreateVertex(cornerCoords[a0], cornerCoords[a1]);
                Vertex vB = CreateVertex(cornerCoords[b0], cornerCoords[b1]);
                Vertex vC = CreateVertex(cornerCoords[c0], cornerCoords[c1]);

                Triangle triangle = new Triangle
                {
                    vertexA = vA,
                    vertexB = vB,
                    vertexC = vC,
                };
                triangles.AddNoResize(triangle);
                //Debug.Log("Adding Triangle"); // Have To Turn of Burst Compilation to View Debug Messages - Takes a lot longer also.
            }

            cornerCoords.Dispose();
        }

        #region Marching Helpers
        Vertex CreateVertex(int3 coordA, int3 coordB)
        {
            float3 posA = CoordToWorld(coordA);
            float3 posB = CoordToWorld(coordB);

            float densityA = SampleDensity(coordA);
            float densityB = SampleDensity(coordB);

            // Interpolation between Two Corner Points
            float t = (planetAttributes.isoLevel - densityA) / (densityB - densityA);
            float3 position = posA + t * (posB - posA);

            // Normal
            float3 normalA = CalculateNormal(coordA);
            float3 normalB = CalculateNormal(coordB);
            float3 normal = math.normalize(normalA + t * (normalB - normalA));

            // ID
            int indexA = IndexFromCoord(coordA);
            int indexB = IndexFromCoord(coordB);

            // Create Vertex
            return new Vertex
            {
                position = position,
                normal = normal,
                id = new int2(math.min(indexA, indexB), math.max(indexA, indexB)),
            };
        }

        float3 CalculateNormal(int3 coord)
        {
            int3 offsetX = new int3(1, 0, 0);
            int3 offsetY = new int3(0, 1, 0);
            int3 offsetZ = new int3(0, 0, 1);

            float dx = SampleDensity(coord + offsetX) - SampleDensity(coord - offsetX);
            float dy = SampleDensity(coord + offsetY) - SampleDensity(coord - offsetY);
            float dz = SampleDensity(coord + offsetZ) - SampleDensity(coord - offsetZ);

            return math.normalize(new float3(dx, dy, dz));
        }
        #endregion

        #region Coord Helpers
        float3 CoordToWorld(float3 coord)
        {
            return (coord / (textureSize - 1.0f) - 0.5f) * planetAttributes.terrainSize;
        }

        int IndexFromCoord(int3 coord)
        {
            coord = coord - chunkAttributes.GetChunkCoord(planetAttributes.pointsPerAxis - 1);

            return coord.z * planetAttributes.pointsPerAxis * planetAttributes.pointsPerAxis + coord.y * planetAttributes.pointsPerAxis + coord.x;
        }
        #endregion

        #region Texture Helpers
        private float SampleDensity(int3 coord)
        {
            coord = math.max(0, math.min(coord, textureSize));

            return textureData[coord.x + coord.y + coord.z];
        }
        #endregion
    }
}