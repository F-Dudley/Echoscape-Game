using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace TerrainGeneration
{
    using ComputeStructs;

    [BurstCompile]
    public struct FilteredMeshData
    {
        // Triangles to be processed
        public NativeList<ComputeStructs.Triangle> triangleList;

        // Vertex Store
        public NativeHashMap<int2, int> vertexIndexMap;
        
        // Processed Values
        public NativeList<float3> processedVertices;
        public NativeList<float3> processedNormals;
        public NativeList<int> processedTriangles;

        public FilteredMeshData(NativeList<Triangle> triangles,
                                    NativeList<float3> processedVertices, NativeList<float3> processedNormals, NativeList<int> processedTriangles,
                                    NativeHashMap<int2, int> vertexIndexMap, bool useFlatShading)
        {
            this.triangleList = triangles;
            this.processedVertices = processedVertices;
            this.processedNormals = processedNormals;
            this.processedTriangles = processedTriangles;

            this.vertexIndexMap = vertexIndexMap;
        }
    }
}