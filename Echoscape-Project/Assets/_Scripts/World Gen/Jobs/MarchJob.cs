using System.Collections.Generic;
using UnityEngine.Rendering;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

using TerrainGeneration.ComputeStructs;

namespace TerrainGeneration
{
    [BurstCompile]
    public struct MarchJob : IJobParallelFor
    {
        // Planet Attributes
        [ReadOnly] PlanetAttributes planetAttributes;
        [ReadOnly] NativeArray<ChunkAttributes> chunks;

        // Main Arrays
        NativeHashMap<int, ListBuffer<Triangle>>.ParallelWriter triangles;
        [ReadOnly] NativeArray<int> triangulationTable;
        [ReadOnly] NativeArray<int> cornerIndexAFromEdge;
        [ReadOnly] NativeArray<int> cornerIndexBFromEdge;

        // Density Texture
        [ReadOnly] NativeArray<float> textureData;
        [ReadOnly] int textureSize;
        [ReadOnly] int textureLayerOffset;


        public MarchJob(PlanetAttributes planetAttributes, NativeArray<ChunkAttributes> chunks,
                         NativeArray<float> textureData, int textureLayerOffset, int textureSize,
                         NativeArray<int> triangulationTable, NativeArray<int> cornerIndexATable, NativeArray<int> cornerIndexBTable,
                         NativeHashMap<int, ListBuffer<Triangle>>.ParallelWriter triangles)
        {
            this.planetAttributes = planetAttributes;
            this.chunks = chunks;

            this.textureData = textureData;
            this.textureSize = textureSize;
            this.textureLayerOffset = textureLayerOffset;

            this.triangulationTable = triangulationTable;
            this.cornerIndexAFromEdge = cornerIndexATable;
            this.cornerIndexBFromEdge = cornerIndexBTable;
            this.triangles = triangles;
        }

        public void Execute(int index)
        {
            int numCubesPerAxis = planetAttributes.pointsPerAxis - 1;
            NativeArray<int3> cornerCoords = new NativeArray<int3>(8, Allocator.Temp);

            int3 currId = new int3();
            int3 coord = new int3();
            int cubeConfig;

            ListBuffer<Triangle> triangleList = new ListBuffer<Triangle>();
 
            for (int x = 0; x < numCubesPerAxis; x++)
            {
                for (int y = 0; y < numCubesPerAxis; y++)
                {
                    for (int z = 0; z < numCubesPerAxis; z++)
                    {
                        currId.x = x; currId.y = y; currId.z = z;

                        coord = currId + (chunks[index].id * numCubesPerAxis);

                        cornerCoords[0] = coord + (new int3(0, 0, 0));
                        cornerCoords[1] = coord + (new int3(1, 0, 0));
                        cornerCoords[2] = coord + (new int3(1, 0, 1));
                        cornerCoords[3] = coord + (new int3(0, 0, 1));
                        cornerCoords[4] = coord + (new int3(0, 1, 0));
                        cornerCoords[5] = coord + (new int3(1, 1, 0));
                        cornerCoords[6] = coord + (new int3(1, 1, 1));
                        cornerCoords[7] = coord + (new int3(0, 1, 1));

                        cubeConfig = 0;
                        for (int i = 0; i < 8; i++)
                        {
                            if (SampleDensityMap(cornerCoords[i]) < planetAttributes.isoLevel)
                            {
                                cubeConfig |= (1 << i);
                            }
                        }
                      
                        int triangulationStartIndex = cubeConfig * 256;
                        for (int i = triangulationStartIndex; i < (triangulationStartIndex + 16); i++)
                        {
                            if (triangulationTable[i] == -1) { break; }

                            int edgeIndexA = triangulationTable[i];
                            int a0 = cornerIndexAFromEdge[edgeIndexA];
                            int a1 = cornerIndexBFromEdge[edgeIndexA];

                            int edgeIndexB = triangulationTable[i + 1];
                            int b0 = cornerIndexAFromEdge[edgeIndexB];
                            int b1 = cornerIndexBFromEdge[edgeIndexB];

                            int edgeIndexC = triangulationTable[i + 2];
                            int c0 = cornerIndexAFromEdge[edgeIndexC];
                            int c1 = cornerIndexBFromEdge[edgeIndexC];

                            Vertex vA = CreateVertex(cornerCoords[a0], cornerCoords[a1]);
                            Vertex vB = CreateVertex(cornerCoords[b0], cornerCoords[b1]);
                            Vertex vC = CreateVertex(cornerCoords[c0], cornerCoords[c1]);

                            Triangle triangle = new Triangle
                            {
                                vertexA = vA,
                                vertexB = vB,
                                vertexC = vC
                            };

                            triangleList.TryAdd(triangle);
                        }
                    }
                    // Z Loop

                }
                // Y Loop

            }
            // X Loop

            triangles.TryAdd(index, triangleList);
            cornerCoords.Dispose();
        }

        private Vertex CreateVertex(int3 coordA, int3 coordB)
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

            return coord.z * planetAttributes.pointsPerAxis * planetAttributes.pointsPerAxis + coord.y * planetAttributes.pointsPerAxis + coord.x;
        }

        // Figure out reading 3D RenderTexture Data. --------------------------------- NEED TO FINISh
        private float SampleDensityMap(int3 coord)
        {
            return textureData[(coord.z * textureSize) + (coord.y * textureSize) + coord.x];
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