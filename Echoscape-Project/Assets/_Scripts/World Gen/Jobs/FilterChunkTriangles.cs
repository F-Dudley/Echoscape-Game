using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;

namespace TerrainGeneration
{
    using ComputeStructs;

    public enum FlatShadingConfig
    {
        NO_FLATSHADING = 0,
        USE_FLATSHADING = 1,
    }

    [BurstCompile]
    public struct FilterChunkTriangles : IJob
    {
        // Triangle Containers
        [ReadOnly] NativeList<ComputeStructs.Triangle> triangleList;

        NativeList<float3> processedVertices;
        NativeList<float3> processedNormals;
        NativeList<int> processedTriangles;

        // Filtering
        NativeHashMap<int2, int> vertexIndexMap;

        [ReadOnly] FlatShadingConfig useFlatShading;

        public FilterChunkTriangles(NativeList<Triangle> triangles,
                                    NativeList<float3> processedVertices, NativeList<float3> processedNormals, NativeList<int> processedTriangles,
                                    NativeHashMap<int2, int> vertexIndexMap, bool useFlatShading)
        {
            this.triangleList = triangles;
            this.processedVertices = processedVertices;
            this.processedNormals = processedNormals;
            this.processedTriangles = processedTriangles;

            this.vertexIndexMap = vertexIndexMap;

            this.useFlatShading = useFlatShading ? FlatShadingConfig.USE_FLATSHADING : FlatShadingConfig.NO_FLATSHADING;
        }

        void IJob.Execute()
        {

            int triangleIndex = 0;
            for (int i = 0; i < triangleList.Length; i++)
            {
                Triangle triangle = triangleList[i];

                ProcessVertex(triangle.vertexA, ref triangleIndex);
                ProcessVertex(triangle.vertexB, ref triangleIndex);
                ProcessVertex(triangle.vertexC, ref triangleIndex);
            }
        }

        private void ProcessVertex(Vertex vertex, ref int triangleIndex)
        {
            if (!usingFlatShading() && vertexIndexMap.TryGetValue(vertex.id, out int sharedVertexIndex))
            {
                processedTriangles[triangleIndex] = sharedVertexIndex;
            }
            else
            {
                if (!usingFlatShading())
                {
                    vertexIndexMap.TryAdd(vertex.id, triangleIndex);
                }

                processedVertices.Add(vertex.position);
                processedNormals.Add(vertex.normal);
                processedTriangles.Add(triangleIndex);
                triangleIndex++;
            }
        }

        private bool usingFlatShading() => useFlatShading == FlatShadingConfig.USE_FLATSHADING;
    }
}