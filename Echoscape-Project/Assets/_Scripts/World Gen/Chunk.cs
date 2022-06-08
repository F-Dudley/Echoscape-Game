using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

namespace TerrainGeneration
{
    using ComputeStructs;

    [System.Serializable]
    [BurstCompile]
    public struct ChunkAttributes
    {
        public int3 id;
        public float3 centre;
        public float size;

        public int3 GetChunkCoord(int numCubesPerAxis) => id * numCubesPerAxis;
    }

    [System.Serializable]
    [BurstCompile]
    public struct Chunk
    {
        public ChunkAttributes attributes;
        public GameObject meshHolder;

        [Header("Mesh")]
        [SerializeField] private Mesh mesh;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshCollider meshCollider;

        public Chunk(int3 _id, float3 _centre, float _size, Material terrainMat, GameObject holder)
        {
            attributes = new ChunkAttributes
            {
                id = _id,
                centre = _centre,
                size = _size,
            };

            meshHolder = holder;

            // Create Mesh Objects
            mesh = new Mesh();
            mesh.name = $"Chunk ({_id.x}-{_id.y}-{_id.z})'s Mesh";
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            meshFilter = meshHolder.AddComponent<MeshFilter>();
            meshRenderer = meshHolder.AddComponent<MeshRenderer>();
            meshRenderer.material = terrainMat;

            meshFilter.mesh = mesh;
            meshCollider = meshRenderer.gameObject.AddComponent<MeshCollider>();
        }

        public void CreateMesh(NativeList<Triangle> triangles, bool useFlatShading)
        {
            if (triangles.Length == 0)
            {
                Debug.Log("No Triangles Submitted");
                return;
            }

            NativeHashMap<int2, int> vertexIndexMap = new NativeHashMap<int2, int>(triangles.Length * 3, Allocator.TempJob);
            NativeList<float3> processedVertices = new NativeList<float3>(triangles.Length * 3, Allocator.TempJob);
            NativeList<float3> processedNormals = new NativeList<float3>(triangles.Length * 3, Allocator.TempJob);
            NativeList<int> processedTriangles = new NativeList<int>(triangles.Length * 3, Allocator.TempJob);

            FilterChunkTriangles triangleFilterJob = new FilterChunkTriangles(triangles,
                                                                              processedVertices, processedNormals, processedTriangles,
                                                                              vertexIndexMap, useFlatShading);
            JobHandle handler = triangleFilterJob.Schedule();
            handler.Complete();

            Debug.Log($"Triangle Amount: {processedTriangles.Length}");
            Debug.Log($"Vertex Amount: {processedVertices.Length}");

            mesh.Clear();
            mesh.SetVertices(processedVertices.AsArray());
            mesh.SetTriangles(processedTriangles.ToArray(), 0, true);

            if (useFlatShading) mesh.RecalculateNormals();
            else mesh.SetNormals(processedNormals.AsArray());

            meshCollider.sharedMesh = mesh;

            vertexIndexMap.Dispose(handler);
            processedVertices.Dispose(handler);
            processedNormals.Dispose(handler);
            processedTriangles.Dispose(handler);
        }
    }
}